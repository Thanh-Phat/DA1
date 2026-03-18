using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.SanPham;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class SanPhamService
    {
        private readonly ApplicationDbcontext _context;

        public SanPhamService(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GET ALL
        public async Task<Pagination<SanPhamViewModels>> GetAllSanPham(string? filter, int pageIndex, int pageSize)
        {
            var query = _context.SanPhams
                .AsQueryable()
                .AsNoTracking();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(sp => sp.TenSP.Contains(filter) || sp.LoaiSP.Contains(filter));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(sp => sp.MaSP)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(sp => new SanPhamViewModels
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    DonGia = sp.DonGia,
                    LoaiSP = sp.LoaiSP,
                    TrangThai = sp.TrangThai,
                    HinhAnh = sp.HinhAnh ?? ""
                })
                .ToListAsync();

            return new Pagination<SanPhamViewModels>
            {
                Items = items,
                TotalRecords = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        // GET BY ID
        public async Task<SanPhamViewModels?> GetSanPhamById(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null) return null;

            return new SanPhamViewModels
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                DonGia = sp.DonGia,
                LoaiSP = sp.LoaiSP,
                TrangThai = sp.TrangThai,
                HinhAnh = sp.HinhAnh ?? "",
                MoTa = sp.MoTa ?? "",
                ThanhPhan = sp.ThanhPhan ?? ""
            };
        }

        // MENU
        public async Task<List<SanPhamViewModels>> GetMenu()
        {
            return await _context.SanPhams
                .AsNoTracking()
                .Where(x => x.TrangThai == "Đang bán")
                .OrderBy(x => x.LoaiSP)
                .Select(x => new SanPhamViewModels
                {
                    MaSP = x.MaSP,
                    TenSP = x.TenSP,
                    DonGia = x.DonGia,
                    LoaiSP = x.LoaiSP,
                    TrangThai = x.TrangThai,
                    HinhAnh = x.HinhAnh ?? "",
                    MoTa = x.MoTa ?? "",
                    ThanhPhan = x.ThanhPhan ?? ""
                })
                .ToListAsync();
        }

        // POST
        public async Task<(bool, string)> PostSanPham(SanPhamCreateVm model)
        {
            if (await _context.SanPhams.AnyAsync(sp => sp.TenSP == model.TenSP))
                return (false, "Sản phẩm đã tồn tại.");

            var sp = new SanPham
            {
                TenSP = model.TenSP,
                DonGia = model.DonGia,
                LoaiSP = model.LoaiSP,
                ThanhPhan = model.ThanhPhan ?? "",
                MoTa = model.MoTa ?? "",
                HinhAnh = model.HinhAnh ?? "",
                TrangThai = "Đang bán"
            };

            _context.SanPhams.Add(sp);
            await _context.SaveChangesAsync();

            return (true, "Thêm sản phẩm thành công.");
        }

        // PUT
        public async Task<(bool, string)> PutSanPham(int id, SanPhamUpdateVm model)
        {
            if (id != model.MaSP)
                return (false, "ID không khớp.");

            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
                return (false, "Không tìm thấy sản phẩm.");

            sp.TenSP = model.TenSP;
            sp.DonGia = model.DonGia;
            sp.LoaiSP = model.LoaiSP;
            sp.HinhAnh = model.HinhAnh ?? "";
            sp.MoTa = model.MoTa ?? "";
            sp.ThanhPhan = model.ThanhPhan ?? "";
            sp.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();

            return (true, "Cập nhật sản phẩm thành công.");
        }

        // DELETE
        public async Task<(bool, string)> Delete(int id)
        {
            var sp = await _context.SanPhams.FindAsync(id);
            if (sp == null)
                return (false, "Không tìm thấy sản phẩm.");

            bool isUsed = await _context.ChiTietHoaDons.AnyAsync(c => c.MaSP == id);
            if (isUsed)
                return (false, "Không thể xóa món đã có trong hóa đơn.");

            _context.SanPhams.Remove(sp);
            await _context.SaveChangesAsync();

            return (true, "Xóa sản phẩm thành công.");
        }
    }
}