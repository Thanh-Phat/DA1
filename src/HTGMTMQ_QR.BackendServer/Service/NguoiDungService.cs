using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.NguoiDung;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class NguoiDungService
    {
        private readonly ApplicationDbcontext _context;

        public NguoiDungService(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GET ALL
        public async Task<Pagination<NguoiDungViewModels>> GetAllNguoiDung(string? filter, int pageIndex, int pageSize)
        {
            var query = _context.NguoiDungs.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x =>
                    x.TenDangNhap.Contains(filter) ||
                    x.HoTen.Contains(filter) ||
                    x.VaiTro.Contains(filter));
            }

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.MaND)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new NguoiDungViewModels
                {
                    MaND = u.MaND,
                    TenDangNhap = u.TenDangNhap,
                    HoTen = u.HoTen,
                    VaiTro = u.VaiTro,
                    TrangThai = u.TrangThai
                })
                .ToListAsync();

            return new Pagination<NguoiDungViewModels>
            {
                Items = items,
                TotalRecords = totalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        // GET BY ID
        public async Task<NguoiDungViewModels?> GetNguoiDungbyId(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null) return null;

            return new NguoiDungViewModels
            {
                MaND = user.MaND,
                TenDangNhap = user.TenDangNhap,
                HoTen = user.HoTen,
                VaiTro = user.VaiTro,
                TrangThai = user.TrangThai
            };
        }

        // CREATE
        public async Task<(bool, string)> PostNguoiDung(NguoiDungCreateVm model)
        {
            if (await _context.NguoiDungs.AnyAsync(x => x.TenDangNhap == model.TenDangNhap))
            {
                return (false, "Tên đăng nhập đã tồn tại.");
            }

            var nd = new NguoiDung
            {
                TenDangNhap = model.TenDangNhap,
                MatKhau = PasswordHelper.HashPassword(model.MatKhau),
                HoTen = model.HoTen,
                VaiTro = model.VaiTro,
                TrangThai = true,
                NgayCapNhatMK = DateTime.Now
            };

            _context.NguoiDungs.Add(nd);
            await _context.SaveChangesAsync();

            return (true, "Thêm người dùng thành công.");
        }

        // UPDATE
        public async Task<(bool, string)> PutNguoiDung(int id, NguoiDungUpdataVm model)
        {
            if (id != model.MaND)
                return (false, "ID không khớp.");

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return (false, "Không tìm thấy người dùng.");

            user.HoTen = model.HoTen;
            user.VaiTro = model.VaiTro;
            user.TrangThai = model.TrangThai;
            user.NgayCapNhatMK = DateTime.Now;
            user.LastModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, "Cập nhật thành công.");
        }

        // DELETE
        public async Task<(bool, string)> DeleteNguoiDung(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return (false, "Không tìm thấy người dùng.");

            _context.NguoiDungs.Remove(user);
            await _context.SaveChangesAsync();

            return (true, "Xóa thành công.");
        }

        // ĐỔI MẬT KHẨU
        public async Task<(bool, string)> DoiMatKhauNguoiDung(int id, DoiMatKhauVm model)
        {
            if (id != model.MaND)
                return (false, "ID không khớp.");

            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
                return (false, "Không tìm thấy người dùng.");

            if (!PasswordHelper.VerifyPassword(model.MatKhauCu, user.MatKhau))
                return (false, "Mật khẩu cũ không đúng.");

            user.MatKhau = PasswordHelper.HashPassword(model.MatKhauMoi);
            user.NgayCapNhatMK = DateTime.Now;

            await _context.SaveChangesAsync();

            return (true, "Đổi mật khẩu thành công.");
        }
    }
}