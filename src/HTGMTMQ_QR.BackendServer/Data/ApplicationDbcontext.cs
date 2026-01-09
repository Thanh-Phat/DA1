using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Data.Entities.Interfaces;
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
          public DbSet<CaLamViec> CaLamViecs { get; set; }
        // 🕒 Tự động cập nhật thời gian sửa đổi
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IDateTracking &&
                            (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IDateTracking)entry.Entity;
                if (entry.State == EntityState.Added)
                {
                    entity.CreateDate = DateTime.Now;
                }
                entity.LastModifiedDate = DateTime.Now;
            }

            return base.SaveChangesAsync(cancellationToken);
        }

    }
}
