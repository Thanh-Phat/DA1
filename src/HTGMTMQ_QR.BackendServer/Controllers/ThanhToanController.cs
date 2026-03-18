using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.ThanhToan;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ThanhToanController : ControllerBase
    {
        private readonly ThanhToanService _thanhToanService;
        public ThanhToanController(ThanhToanService thanhToanService)
        {
            _thanhToanService = thanhToanService;
        }

        // GETALL: danh sách thanh toán
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllThanhToan(string? filter = null, int pageIndex = 1, int pageSize = 20)
        {
            var result = await _thanhToanService.GetAllThanhToan(filter, pageIndex, pageSize);
            return Ok(result);
        }

        // GET theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ThanhToanViewModels>> GetThanhToanById(int id)
        {
            var tt = await _thanhToanService.GetThanhToanById(id);
            if (tt == null)
                return NotFound("Không tìm thấy thông tin thanh toán.");
            return Ok(tt);

        }

        // GET: Lấy thông tin hóa đơn + chi tiết món trước khi thanh toán
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("hoadon/{maHD}")]
        public async Task<IActionResult> GetHoaDonChiTiet(int maHD)
        {
            var result = await _thanhToanService.GetHoaDonChiTiet(maHD);
            if (result == null)
                return NotFound(new { message = "Không tìm thấy hóa đơn hoặc chi tiết hóa đơn." });
            return Ok(result);
        }

        //POST: tạo thanh toán
        [Authorize(Roles = "ThuNgan")]
        [HttpPost("{id}/thanh-toan")]

        public async Task<IActionResult> PostThanhToan(ThanhToanCreateVm model)
        {
            var (success, message,data) = await _thanhToanService.PostThanhToan(model);
            if (!success)
                return BadRequest(new { message });
            return Ok(new { message });

        }
        //
        //Put: cập nhật thanh toán
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}/cap-nhat-thanh-toan")]
        public async Task<ActionResult<ThanhToanViewModels>> PutThanhToan(int id, ThanhToanUpdateVm model)
        {
            var result = await _thanhToanService.PutThanhToan(id, model);
            if (result == null)
                return NotFound("Không tìm thấy thông tin thanh toán hoặc cập nhật thất bại.");
            return Ok(new { message = "Cập nhật thanh toán thành công.", data = result });
        }

        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}/Thanh-Toan")]
        public async Task<IActionResult> DeleteThanhToan(int id)
        {
            var result = await _thanhToanService.DeleteThanhToan(id);
            if (!result)
                return NotFound(new { message = "Không tìm thấy thông tin thanh toán hoặc xóa thất bại." });
            return Ok(new { message = "Xóa thanh toán thành công." });

        }
    }
}
