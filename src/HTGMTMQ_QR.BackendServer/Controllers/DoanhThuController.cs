using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.ViewModels.Systems.DoanhThu;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize(Roles = "QuanLy")]
    [Route("api/[controller]")]
    [ApiController]
    public class DoanhThuController : ControllerBase
    {

        private readonly ApplicationDbcontext _context;
        public DoanhThuController(ApplicationDbcontext context)
        {
            _context = context;
        }

        // GET: api/revenue/ngay?date=2025-11-23
        [HttpGet("ngay")]
        public async Task<IActionResult> GetDoanhThuTheoNgay(DateTime date)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT.Date == date.Date);

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return Ok(new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            });
        }

        // GET: api/revenue/khoang?from=2025-11-01&to=2025-11-30
        [HttpGet("khoang")]
        public async Task<IActionResult> GetDoanhThuTheoKhoang(DateTime from, DateTime to)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT.Date >= from.Date && t.NgayTT.Date <= to.Date);

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return Ok(new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            });
        }

        // GET: api/revenue/thang?month=11&year=2025
        [HttpGet("thang")]
        public async Task<IActionResult> GetDoanhThuTheoThang(int month, int year)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT.Month == month && t.NgayTT.Year == year);

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return Ok(new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            });
        }

        // GET: api/revenue/nam?year=2025
        [HttpGet("nam")]
        public async Task<IActionResult> GetDoanhThuTheoNam(int year)
        {
            var query = _context.ThanhToans
                .Where(t => t.NgayTT.Year == year);

            var data = await query
                .Select(t => new ChiTietHoaDonItem
                {
                    MaHD = t.MaHD,
                    NgayTT = t.NgayTT,
                    SoTien = t.SoTien,
                    HinhThuc = t.HinhThuc
                }).ToListAsync();

            return Ok(new DoanhThuViewModels
            {
                TongDoanhThu = data.Sum(x => x.SoTien),
                SoHoaDon = data.Count,
                ChiTiet = data
            });
        }
    }
}
