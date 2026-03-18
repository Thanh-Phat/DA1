using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
namespace HTGMTMQ_QR.BackendServer.Data.SeedData
{
    public class SeedAdmin
    {
        public static void Seed(ApplicationDbcontext context)
        {
            {

                if (!context.NguoiDungs.Any(x => x.TenDangNhap == "admin"))
                {
                    var admin = new NguoiDung

                    {
                        TenDangNhap = "admin",
                        MatKhau =PasswordHelper.HashPassword("admin123"), // Lưu ý: Trong thực tế, bạn nên hash mật khẩu
                        HoTen = "Quản Lý",
                        VaiTro = "QuanLy",
                        TrangThai = true,
                        NgayCapNhatMK = DateTime.Now
                    };

                    context.NguoiDungs.Add(admin);
                    context.SaveChanges();
                }
            }
        }
    }
}
