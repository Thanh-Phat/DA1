using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.ThanhToan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public ThanhToanController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GETALL: danh sách thanh toán
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllThanhToan(string? filter = null, int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.ThanhToans.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(tt => tt.HinhThuc.Contains(filter));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(tt => tt.MaTT)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(tt => new ThanhToanViewModels
                {
                    MaTT = tt.MaTT,
                    MaHD = tt.MaHD,
                    HinhThuc = tt.HinhThuc,
                    SoTien = tt.SoTien,
                    NgayTT = tt.NgayTT
                })
                .ToListAsync();

            return Ok(new Pagination<ThanhToanViewModels>
            {
                Items = items,
                TotalRecords = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            });
        }

        // GET theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ThanhToanViewModels>> GetThanhToanById(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);

            if (tt == null)
                return NotFound("Không tìm thấy thông tin thanh toán.");

            return Ok(new ThanhToanViewModels
            {
                MaTT = tt.MaTT,
                MaHD = tt.MaHD,
                HinhThuc = tt.HinhThuc,
                SoTien = tt.SoTien,
                NgayTT = tt.NgayTT
            });
        }

        // GET: Lấy thông tin hóa đơn + chi tiết món trước khi thanh toán
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("hoadon/{maHD}")]
        public async Task<IActionResult> GetHoaDonChiTiet(int maHD)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.ChiTietHoaDons)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(hd => hd.MaHD == maHD);

            if (hoaDon == null)
                return NotFound("Không tìm thấy hóa đơn.");

            return Ok(new
            {
                hoaDon.MaHD,
                hoaDon.MaBan,
                hoaDon.NgayTao,
                hoaDon.TongTien,
                hoaDon.TrangThai,
                ChiTiet = hoaDon.ChiTietHoaDons.Select(ct => new
                {
                    ct.MaCTHD,
                    ct.MaSP,
                    TenSP = ct.SanPham.TenSP,
                    ct.SoLuong,
                    ct.DonGia,
                    ct.ThanhTien,
                    ct.TrangThaiMon
                })
            });
        }


        //POST: tạo thanh toán
        [Authorize(Roles = "ThuNgan")]
        [HttpPost("{id}/thanh-toan")]

        public async Task<IActionResult> PostThanhToan(ThanhToanCreateVm model)
        {
            var hd = await _context.HoaDons.FindAsync(model.MaHD);

            if (hd == null)
            {
                return NotFound(new { message = "Không tìm thấy hóa đơn." });
            }
            if (hd.TrangThai == "Đã thanh toán")
            {
                return BadRequest(new { message = "Hóa đơn đã thanh toán trước đó." });
            }

            if (model.SoTien < hd.TongTien)
            {
                return BadRequest(new { message = "Số tiền thanh toán không hợp lệ." });
            }

            var tienthua = model.SoTien - hd.TongTien;
            var tt = new ThanhToan
            {
                MaHD = model.MaHD,
                HinhThuc = model.HinhThuc,
                SoTien = model.SoTien,
                NgayTT = DateTime.Now,
            };

            _context.ThanhToans.Add(tt);
            //Update trang thái hóa đơn
            hd.TrangThai = "Đã thanh toán";

            //Update Trạng thái bàn
            var ban = await _context.Bans.FindAsync(hd.MaBan);
            if (ban != null)
                ban.TrangThai = "Trống";

            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(
                    nameof(GetThanhToanById), 
                    new { id = tt.MaTT }, 
                    new
                    {
                        message = "Tạo thanh toán thành công",
                        tongtien = hd.TongTien,
                        soTienkhachdua = model.SoTien,
                        tienthua = tienthua       
                    }
                 );
            }
            return BadRequest("Không thể tạo thanh toán.");
        }
        //
        //Put: cập nhật thanh toán
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}/cap-nhat-thanh-toan")]
        public async Task<ActionResult<ThanhToanViewModels>> PutThanhToan(int id, ThanhToanUpdateVm model)
        {
            if (id != model.MaTT)
                return BadRequest(new { message = "ID không khớp." });

            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt == null)
            {
                return NotFound(new { message = "Không tìm thấy thông tin thanh toán." });
            }

            tt.HinhThuc = model.HinhThuc;
            tt.SoTien = model.SoTien;
            tt.NgayTT = DateTime.Now;
            
            _context.Entry(tt).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return Ok(new {message = " Cập nhật thanh toán thành công."});
        }

        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}/Thanh-Toan")]
        public async Task<IActionResult> DeleteThanhToan(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt == null)
                return NotFound("Không tìm thấy thông tin thanh toán.");
            // Lấy hóa đơn liên quan
            var hd = await _context.HoaDons.FindAsync(tt.MaHD);
            if (hd != null)
            {
                hd.TrangThai = "Chưa thanh toán";

                var ban = await _context.Bans.FindAsync(hd.MaBan);
                if (ban != null)
                    ban.TrangThai = "Đang phục vụ";
            }

            _context.ThanhToans.Remove(tt);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa thanh toán và khôi phục trạng thái hóa đơn/bàn." });
        }
    }
}
