using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.HoaDon;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HTGMTMQ_QR.BackendServer.Service;
using HoaDonService = HTGMTMQ_QR.BackendServer.Service.HoaDonService;
namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonController : ControllerBase
    {
        private readonly HoaDonService _hoaDonService;
        public HoaDonController(HoaDonService hoaDonService)
        {
            _hoaDonService = hoaDonService;
        }

        //Lấy toàn bộ danh sách Chitiethoadon 
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet]
        public async Task<IActionResult> GetAllHoaDon(string? filter, int pageIndex = 1, int pageSize = 10)
        {
            var result = await _hoaDonService.GetAllHoaDon(filter, pageIndex, pageSize);
            return Ok(result);
        }

        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("{id}")]
        public async Task<ActionResult<HoaDonViewModels>> GetHDById(int id)
        {
            var hd = await _hoaDonService.GetHDById(id);
            if (hd == null)
                return NotFound("Không tìm thấy hóa đơn.");
            return Ok(hd);
        }
        // GET: api/hoadon/TheoNgay?date=2025-01-01
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("TheoNgay")]
        public async Task<IActionResult> GetHDTheoNgay(DateTime date)
        {
            var result = await _hoaDonService.GetHDTheoNgay(date);
            return Ok(result);
        }

        // GET: /api/hoadon/dangmo/{maban}
        [Authorize(Roles = "ThuNgan,Bep,QuanLy")]
        [HttpGet("dangmo/{maban}")]
        public async Task<IActionResult> GetHoaDonDangMoTheoBan(int maban)
        {
           var result = await _hoaDonService.GetHoaDonDangMoTheoBan(maban);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn đang mở cho bàn này." });
            return Ok(result);  
        }

        //Post = Thêm hóa đơn
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpPost]
        public async Task<ActionResult<HoaDonViewModels>> PostHD(HoaDonCreateVm model)
        {
            var result = await _hoaDonService.PostHD(model);
            if (result == null)
                return BadRequest(new { message = "Không thể tạo hóa đơn." });
            return Ok(new { message = "Tạo hóa đơn thành công.", data = result });
        }

        // PUT: api/hoadon/{id}
        // Cập nhật thông tin hóa đơn
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}")]
        public async Task<ActionResult<HoaDonViewModels>> PutHD(int id, HoaDonUpdateVm model)
        {
            var result = await _hoaDonService.PutHD(id, model);
            if (result == null)
                return BadRequest(new { message = "Không thể cập nhật hóa đơn." });
            return Ok(new { message = "Cập nhật hóa đơn thành công.", data = result });
        }


        //Delete: api/hoadon/{id}
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHD(int id)
        {
            var result = await _hoaDonService.DeleteHD(id);
            if (!result)
                return BadRequest(new { message = "Không thể xóa hóa đơn." });
            return Ok(new { message = "Xóa hóa đơn thành công." });
        }
    }
}
