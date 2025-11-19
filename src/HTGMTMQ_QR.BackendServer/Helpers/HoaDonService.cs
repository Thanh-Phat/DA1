using HTGMTMQ_QR.BackendServer.Data;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Helpers
{
    public class HoaDonService
    {
        public static async Task CapNhatTongTien(ApplicationDbcontext _context, int maHD)
        {
            var tong = await _context.ChiTietHoaDons
              .Where(x => x.MaHD == maHD)
              .SumAsync(x => (decimal?)x.ThanhTien) ?? 0;

            var hd = await _context.HoaDons.FindAsync(maHD);
            if (hd != null)
            {
                hd.TongTien = tong;
                hd.LastModifiedDate = DateTime.Now;
            }
        }
    }
}