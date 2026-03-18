using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.ViewModels.Systems.DoanhThu;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class DoanhThuService
    {
        private readonly ApplicationDbcontext _context;

        public DoanhThuService(ApplicationDbcontext context)
        {
            _context = context;
        }

        public async Task<DoanhThuViewModels> GetDoanhThuTheoNgay(DateTime date)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT != null &&
                    t.NgayTT >= date.Date &&
                    t.NgayTT < date.Date.AddDays(1));

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            };
        }

        public async Task<DoanhThuViewModels> GetDoanhThuTheoKhoang(DateTime from, DateTime to)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT != null && t.NgayTT >= from.Date && t.NgayTT < to.Date.AddDays(1));

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            };
        }

        public async Task<DoanhThuViewModels> GetDoanhThuTheoThang(int month, int year)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT != null &&
                            t.NgayTT.Value.Month == month &&
                            t.NgayTT.Value.Year == year);
            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            };
        }

        public async Task<DoanhThuViewModels> GetDoanhThuTheoNam(int year)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT != null &&
                            t.NgayTT.Value.Year == year);

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            };
        }
    }
}