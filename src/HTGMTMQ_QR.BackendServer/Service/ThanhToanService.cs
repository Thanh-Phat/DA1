using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.ThanhToan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class ThanhToanService
    {
        private readonly ApplicationDbcontext _context;
        public ThanhToanService(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GETALL: danh sách thanh toán
        public async Task<Pagination<ThanhToanViewModels>> GetAllThanhToan(string? filter = null, int pageIndex = 1, int pageSize = 20)
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
                    NgayTT = (DateTime)tt.NgayTT
                })
                .ToListAsync();

            return new Pagination<ThanhToanViewModels>
            {
                Items = items,
                TotalRecords = total,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
        }

        // GET theo ID
        public async Task<ThanhToanViewModels?> GetThanhToanById(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);

            if (tt == null)
                return null;

            return new ThanhToanViewModels
            {
                MaTT = tt.MaTT,
                MaHD = tt.MaHD,
                HinhThuc = tt.HinhThuc,
                SoTien = tt.SoTien,
                NgayTT = (DateTime)tt.NgayTT
            };
        }

        // GET: Lấy thông tin hóa đơn + chi tiết món trước khi thanh toán
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("hoadon/{maHD}")]
        public async Task<object> GetHoaDonChiTiet(int maHD)
        {
            var hoaDon = await _context.HoaDons
                .Include(hd => hd.ChiTietHoaDons)
                .ThenInclude(ct => ct.SanPham)
                .FirstOrDefaultAsync(hd => hd.MaHD == maHD);

            if (hoaDon == null)
                return null;

            return new
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
            };
        }


        //POST: tạo thanh toán

        public async Task<(bool success, string message, object? data)> PostThanhToan(ThanhToanCreateVm model)
        {
            var hd = await _context.HoaDons.FindAsync(model.MaHD);

            if (hd == null)
            {
                return (false, "Không tìm thấy hóa đơn.", null);
            }
            if (hd.TrangThai == "Đã thanh toán")
            {
                return (false, "Hóa đơn đã thanh toán trước đó.", null);
            }

            if (model.SoTien < hd.TongTien)
            {
                return (false, "Số tiền thanh toán không hợp lệ.", null);
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

            await _context.SaveChangesAsync();
            return (true, "Thanh toán thành công", new
            {
                tongTien = hd.TongTien,
                soTienKhachDua = model.SoTien,
                tienThua = tienthua
            });
        }
        //
        //Put: cập nhật thanh toán
        public async Task<bool> PutThanhToan(int id, ThanhToanUpdateVm model)
        {
            //if (id != model.MaTT)
            //    return false;

            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt == null)
            {
                return false;
            }

            tt.HinhThuc = model.HinhThuc;
            tt.SoTien = model.SoTien;
            tt.NgayTT = DateTime.Now;

            _context.Entry(tt).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteThanhToan(int id)
        {
            var tt = await _context.ThanhToans.FindAsync(id);
            if (tt == null)
                return false;
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

            return true;
        }
    }
}
