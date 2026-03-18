using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class QrService
    {
        private readonly ApplicationDbcontext _context;

        public QrService(ApplicationDbcontext context)
        {
            _context = context;
        }

        public async Task<(bool success, string message, object? data)> AccessByQr(int maban)
        {
            var ban = await _context.Bans.FindAsync(maban);
            if (ban == null)
                return (false, "Không tìm thấy bàn.", null);

            var hoadon = await _context.HoaDons
                .Where(h => h.MaBan == maban && h.TrangThai == "Chưa thanh toán")
                .OrderByDescending(h => h.NgayTao)
                .FirstOrDefaultAsync();

            // Nếu chưa có hóa đơn → tạo mới
            if (hoadon == null)
            {
                hoadon = new HoaDon
                {
                    MaBan = maban,
                    MaND = null,
                    NgayTao = DateTime.Now,
                    TongTien = 0,
                    TrangThai = "Chưa thanh toán"
                };

                _context.HoaDons.Add(hoadon);

                // cập nhật trạng thái bàn
                ban.TrangThai = "Đang phục vụ";

                await _context.SaveChangesAsync();
            }

            // Lấy menu
            var menu = await _context.SanPhams
                .AsNoTracking()
                .Where(sp => sp.TrangThai == "Đang bán")
                .OrderBy(sp => sp.LoaiSP)
                .Select(sp => new
                {
                    sp.MaSP,
                    sp.TenSP,
                    sp.DonGia,
                    sp.LoaiSP,
                    sp.HinhAnh
                })
                .ToListAsync();

            var result = new
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
            };

            return (true, "Thành công", result);
        }
    }
}