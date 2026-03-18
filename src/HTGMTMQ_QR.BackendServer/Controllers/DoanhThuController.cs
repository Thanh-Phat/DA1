using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Service;
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

        private readonly DoanhThuService _doanhThuService;
        public DoanhThuController(DoanhThuService doanhThuService)
        {
            _doanhThuService = doanhThuService;
        }

        // GET: api/revenue/ngay?date=2025-11-23
        [HttpGet("ngay")]
        public async Task<IActionResult> GetDoanhThuTheoNgay(DateTime date)
        {
            var result = await _doanhThuService.GetDoanhThuTheoNgay(date);
            return Ok(result);
        }

        // GET: api/revenue/khoang?from=2025-11-01&to=2025-11-30
        [HttpGet("khoang")]
        public async Task<IActionResult> GetDoanhThuTheoKhoang(DateTime from, DateTime to)
        {
           var result = await _doanhThuService.GetDoanhThuTheoKhoang(from, to);
            return Ok(result);
        }

        // GET: api/revenue/thang?month=11&year=2025
        [HttpGet("thang")]
        public async Task<IActionResult> GetDoanhThuTheoThang(int month, int year)
        {
            var result = await _doanhThuService.GetDoanhThuTheoThang(month, year);
            return Ok(result);
        }

        // GET: api/revenue/nam?year=2025
        [HttpGet("nam")]
        public async Task<IActionResult> GetDoanhThuTheoNam(int year)
        {
            var result = await _doanhThuService.GetDoanhThuTheoNam(year);   
            return Ok(result);
        }
    }
}
