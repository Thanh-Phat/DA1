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
        // THU NGÂN xem chi tiết
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> GetCTHDById(int id)
        {
            var cthd = await _context.ChiTietHoaDons.FindAsync(id);

            if (cthd == null)
                return NotFound();

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

        // POST: Thêm món vào hóa đơn
        // KHÁCH GỌI MÓN (không cần đăng nhập)
        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> PostCTHD(ChiTietHoaDonCreateVm model)
        {
            var cthd = new ChiTietHoaDon
            {
                MaHD = model.MaHD,
                MaSP = model.MaSP,
                SoLuong = model.SoLuong,
                DonGia = model.DonGia,
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

        // PUT: Cập nhật món
        // BẾP cập nhật trạng thái món
        [Authorize(Roles = "Bep")]
        [HttpPut("{id}/capnhat-trangthai")]
        public async Task<IActionResult> PutCTHD(int id, ChiTietHoaDonUpdataVm model)
        {
            if (id != model.MaCTHD)
                return BadRequest("ID không khớp.");

            var cthd = await _context.ChiTietHoaDons.FindAsync(id);
            if (cthd == null)
                return NotFound();

            cthd.SoLuong = model.SoLuong;
            cthd.TrangThaiMon = model.TrangThaiMon;

            _context.Entry(cthd).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            await HoaDonService.CapNhatTongTien(_context, cthd.MaHD);
            return Ok(new { message = "Cập nhật chi tiết hóa đơn thành công." });
        }

        // DELETE: Xóa món
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
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
