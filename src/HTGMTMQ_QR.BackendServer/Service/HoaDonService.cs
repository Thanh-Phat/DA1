using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.HoaDon;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class HoaDonService
    {
        private readonly ApplicationDbcontext _context;

        public HoaDonService(ApplicationDbcontext context)
        {
            _context = context;
        }

        public async Task<Pagination<HoaDonViewModels>> GetAllHoaDon(string? filter , int pageIndex , int pageSize)
        {
            var query = _context.HoaDons.AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(hd => hd.TrangThai.Contains(filter));
            }

            var TotalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(hd => hd.MaHD)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(hd => new HoaDonViewModels
                {
                    MaHD = hd.MaHD,
                    MaBan = hd.MaBan,
                    MaND = hd.MaND ?? 0,
                    NgayTao = hd.NgayTao,
                    TongTien = hd.TongTien,
                    TrangThai = hd.TrangThai,
                })
                .ToListAsync();

            return new Pagination<HoaDonViewModels>
            {
                Items = items,
                TotalRecords = TotalRecords,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }


        public async Task<HoaDonViewModels?> GetHDById(int id)
        {
            var hd = await _context.HoaDons.FindAsync(id);

            if (hd == null)
                return null;

            return new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND ?? 0,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai,
            };
        }

        public async Task<List<HoaDonViewModels>> GetHDTheoNgay(DateTime date)
        {
            return await _context.HoaDons
            .Where(hd => hd.NgayTao.Date == date.Date)
            .Select(hd => new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND ?? 0,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai
            })
            .ToListAsync();
        }

        public async Task<HoaDonViewModels?> GetHoaDonDangMoTheoBan(int maban)
        {
            var hd = await _context.HoaDons
                .Where(h => h.MaBan == maban && h.TrangThai == "Chưa thanh toán")
                .OrderByDescending(h => h.NgayTao)
                .FirstOrDefaultAsync();

            if (hd == null)
                return null;


            return new HoaDonViewModels
            {
                MaHD = hd.MaHD,
                MaBan = hd.MaBan,
                MaND = hd.MaND ?? 0,
                NgayTao = hd.NgayTao,
                TongTien = hd.TongTien,
                TrangThai = hd.TrangThai
            };

        }

        public async Task<bool> PostHD(HoaDonCreateVm model)
        {
            var hd = new HoaDon
            {
                MaBan = model.MaBan,
                MaND = model.MaND,
                NgayTao = DateTime.Now,
                TongTien = 0,
                TrangThai = "Chưa thanh toán"
            };

            _context.HoaDons.Add(hd);
            // Cập nhật trạng thái bàn

            var ban = await _context.Bans.FindAsync(model.MaBan);
            if (ban != null)
            {
                ban.TrangThai = "Đang phục vụ";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> PutHD(int id, HoaDonUpdateVm model)
        {
            var hd = await _context.HoaDons.FindAsync(id);
            if (hd == null)
                return false;

            // Nếu đổi bàn → cập nhật trạng thái 2 bàn
            if (hd.MaBan != model.MaBan)
            {
                // Bàn cũ → Trống
                var banCu = await _context.Bans.FindAsync(hd.MaBan);
                if (banCu != null)
                    banCu.TrangThai = "Trống";

                // Bàn mới → Đang phục vụ
                var banMoi = await _context.Bans.FindAsync(model.MaBan);
                if (banMoi != null)
                    banMoi.TrangThai = "Đang phục vụ";
            }

            // Cập nhật dữ liệu hóa đơn
            hd.MaBan = model.MaBan;
            hd.MaND = model.MaND;
            hd.TrangThai = model.TrangThai;
            hd.LastModifiedDate = DateTime.Now;

            // Không cho sửa tổng tiền — auto tính
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteHD(int id)
        {
            var hoadon = await _context.HoaDons.FindAsync(id);
            if (hoadon == null)
                return false;
            // Không cho xoá hóa đơn đã thanh toán
            if (hoadon.TrangThai == "Đã thanh toán")
                return false;
            //Xóa chi tiết hóa đơn liên quan
            var cthds = _context.ChiTietHoaDons.Where(c => c.MaHD == id);
            _context.ChiTietHoaDons.RemoveRange(cthds);

            //Đặt trạng thái bàn về Trống
            var ban = await _context.Bans.FindAsync(hoadon.MaBan);
            if (ban != null)
                ban.TrangThai = "Trống";
            //Xóa hóa đơn
            _context.HoaDons.Remove(hoadon);
            await _context.SaveChangesAsync();

            return true;
        }
        public static async Task CapNhatTongTien(ApplicationDbcontext context, int maHD)
        {
            // Tính tổng tiền từ chi tiết hóa đơn
            var tongTien = await context.ChiTietHoaDons
                .Where(x => x.MaHD == maHD)
                .SumAsync(x => (decimal?)x.ThanhTien) ?? 0;

            // Lấy hóa đơn
            var hoaDon = await context.HoaDons.FindAsync(maHD);

            if (hoaDon != null)
            {
                hoaDon.TongTien = tongTien;
                hoaDon.LastModifiedDate = DateTime.Now;

                await context.SaveChangesAsync();
            }
        }
    }
}
