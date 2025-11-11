using HTGMTMQ_QR.BackendServer.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Data
{
    public class ApplicationDbcontext : DbContext
    {
        public ApplicationDbcontext(DbContextOptions options) : base(options)
        {
        }
            public DbSet<NguoiDung> NguoiDungs { get; set; }
            public DbSet<HoaDon> HoaDons { get; set; }
            public DbSet<Ban> Bans { get; set; }
            public DbSet<QRCode> QRCodes { get; set; }
            public DbSet<SanPham> SanPhams { get; set; }
            public DbSet<ChiTietHoaDon> ChiTietHoaDons { get; set; }
            public DbSet<ThanhToan> ThanhToans { get; set; }
    }
}
