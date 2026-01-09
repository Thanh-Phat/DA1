using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        private readonly ApplicationDbcontext _context;
        public QrController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GET: api/qr/{maban}
        [HttpGet("{maban}")]
        [AllowAnonymous]
        public async Task<IActionResult> AccessByQr(int maban)
        {
            var ban = await _context.Bans.FindAsync(maban);
            if (ban == null)
                return NotFound(new { message = "Không tìm thấy bàn." });

            // Kiểm tra bàn có hóa đơn đang mở chưa
            var hoadon = await _context.HoaDons
                .Where(h => h.MaBan == maban && h.TrangThai == "Chưa thanh toán")
                .OrderByDescending(h => h.NgayTao)
                .FirstOrDefaultAsync();

            // Nếu chưa có hóa đơn → tạo hóa đơn mới
            if (hoadon == null)
            {
                hoadon = new HoaDon
                {
                    MaBan = maban,
                    MaND = null, // khách hàng không đăng nhập
                    NgayTao = DateTime.Now,
                    TongTien = 0,
                    TrangThai = "Chưa thanh toán"
                };

                _context.HoaDons.Add(hoadon);

                // cập nhật trạng thái bàn
                ban.TrangThai = "Đang phục vụ";

                await _context.SaveChangesAsync();
            }

            // Lấy menu món còn bán
            var menu = await _context.SanPhams
                .Where(sp => sp.TrangThai == "Đang bán")
                .Select(sp => new
                {
                    sp.MaSP,
                    sp.TenSP,
                    sp.DonGia,
                    sp.LoaiSP
                })
                .ToListAsync();

            return Ok(new
            {
                Ban = new
                {
                    ban.MaBan,
                    ban.SoBan,
                    ban.TrangThai
                },
                HoaDon = new
                {
                    hoadon.MaHD,
                    hoadon.MaBan,
                    hoadon.NgayTao,
                    hoadon.TongTien,
                    hoadon.TrangThai
                },
                Menu = menu
            });

        }
    }
}
