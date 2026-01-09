using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChiTietHoaDonController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;

        public ChiTietHoaDonController(ApplicationDbcontext context)
        {
            _context = context;
        }
        // URL GET: http://localhost:5001/api/chitiethoadon/?filter={searchKeyword}&pageIndex=1&pageSize=20
        // GET ALL CTHD theo MaHD
        [Authorize(Roles = "QuanLy")]
        [HttpGet]
        public async Task<IActionResult> GetAllChiTietHoaDon(string? filter = null, int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.ChiTietHoaDons.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(cthd => cthd.TrangThaiMon.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(cthd => cthd.MaCTHD)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(cthd => new ChiTietHoaDonViewModels
                {
                    MaCTHD = cthd.MaCTHD,
                    MaHD = cthd.MaHD,
                    MaSP = cthd.MaSP,
                    SoLuong = cthd.SoLuong,
                    DonGia = cthd.DonGia,
                    ThanhTien = cthd.ThanhTien,
                    TrangThaiMon = cthd.TrangThaiMon
                })
                .ToListAsync();

            var pagination = new Pagination<ChiTietHoaDonViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            return Ok(pagination);


        }
        // url: http://localhost:5001/api/chitiethoadon/{id}/xem-chi-tiet
        //xem chi tiết
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("{id}/xem-chi-tiet-mon")]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> GetCTHDById(int id)
        {
            var cthd = await _context.ChiTietHoaDons.FindAsync(id);

            if (cthd == null)
            {
                return NotFound();
            }
            var model = new ChiTietHoaDonViewModels
            {
                MaCTHD = cthd.MaCTHD,
                MaHD = cthd.MaHD,
                MaSP = cthd.MaSP,
                SoLuong = cthd.SoLuong,
                DonGia = cthd.DonGia,
                ThanhTien = cthd.ThanhTien,
                TrangThaiMon = cthd.TrangThaiMon
            };
            return Ok(model);
        }

        //url: http://localhost:5001/api/chitiethoadon/them-mon
        // POST: Thêm món vào hóa đơn
        // KHÁCH GỌI MÓN (không cần đăng nhập)
        [AllowAnonymous]
        [HttpPost("{id}/them-mon")]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> PostCTHD(ChiTietHoaDonCreateVm model)
        {

            var hd = await _context.HoaDons.FindAsync(model.MaHD);

            if (hd == null)
            {
                return NotFound("Hóa đơn không tồn tại.");
            }

            var sp = await _context.SanPhams.FindAsync(model.MaSP);
            if (sp == null || sp.TrangThai == "Hết hàng")
            {
                return BadRequest("Sản phẩm không hợp lệ.");
            }

            var cthd = new ChiTietHoaDon
            {
                MaHD = model.MaHD,
                MaSP = model.MaSP,
                SoLuong = model.SoLuong,
                DonGia = sp.DonGia,
                ThanhTien = model.SoLuong * sp.DonGia,
                TrangThaiMon = "Đang nấu"
            };

            _context.ChiTietHoaDons.Add(cthd);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                await HoaDonService.CapNhatTongTien( _context, model.MaHD);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCTHDById), new { id = cthd.MaCTHD }, model);
            }
            return BadRequest("Không thể thêm chi tiết hóa đơn.");
        }
        // url: http://localhost:5001/api/chitiethoadon/{id}/capnhat-trangthai
        // PUT: Cập nhật trạng thái món
        // BẾP cập nhật trạng thái món
        [Authorize(Roles = "Bep")]
        [HttpPut("{id}/capnhat-trangthai")]
        public async Task<IActionResult> PutCTHD(int id, ChiTietHoaDonUpdataVm model)
        {
            if (id != model.MaCTHD)
                return BadRequest("ID không khớp.");

            var cthd = await _context.ChiTietHoaDons.FindAsync(id);
            if (cthd == null)
            {
                return NotFound("Không tồn tại");
            }
            cthd.SoLuong = model.SoLuong;
            cthd.ThanhTien= model.SoLuong * cthd.DonGia;
            cthd.TrangThaiMon = model.TrangThaiMon;

            _context.Entry(cthd).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, cthd.MaHD);
            return Ok(new { message = "Cập nhật chi tiết hóa đơn thành công." });
        }


        //url:http://localhost:5001/api/chitiethoadon/{id}/Xoa-mon
        // DELETE: Xóa món
        [Authorize(Roles = "QuanLy")]
        [HttpDelete(("{id}/Xoa-mon"))]
        public async Task<IActionResult> DeleteCTHD(int id)
        {
            var cthd = await _context.ChiTietHoaDons.FindAsync(id);
            if (cthd == null)
                return NotFound();
            int maHD =cthd.MaHD;
            _context.ChiTietHoaDons.Remove(cthd);
            await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, maHD);
            return Ok(new { message = "Xóa món thành công." });
        }
    }
}
