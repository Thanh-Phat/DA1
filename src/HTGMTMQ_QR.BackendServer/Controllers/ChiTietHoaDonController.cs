using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using HTGMTMQ_QR.ViewModels.Systems.Chitiethoadon;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChiTietHoaDonController : ControllerBase
    {
        private readonly ChiTietHoaDonService _chiTietHoaDonService;

        public ChiTietHoaDonController(ChiTietHoaDonService chiTietHoaDonService)
        {
            _chiTietHoaDonService = chiTietHoaDonService;
        }
        // URL GET: http://localhost:5001/api/chitiethoadon/?filter={searchKeyword}&pageIndex=1&pageSize=20
        // GET ALL CTHD theo MaHD
        [Authorize(Roles = "QuanLy")]
        [HttpGet]
        public async Task<IActionResult> GetAllChiTietHoaDon(string? filter = null, int pageIndex = 1, int pageSize = 20)
        {
            var result = await _chiTietHoaDonService.GetAllChiTietHoaDon(filter, pageIndex, pageSize);
            return Ok(result);


        }
        // url: http://localhost:5001/api/chitiethoadon/{id}/xem-chi-tiet
        //xem chi tiết
        [Authorize(Roles = "ThuNgan,QuanLy")]
        [HttpGet("{id}/xem-chi-tiet-mon")]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> GetCTHDById(int id)
        {
            var cthd = await _chiTietHoaDonService.GetCTHDById(id);
            if (cthd == null)
                return NotFound("Không tìm thấy chi tiết hóa đơn.");
            return Ok(cthd);
        }

        //url: http://localhost:5001/api/chitiethoadon/them-mon
        // POST: Thêm món vào hóa đơn
        // KHÁCH GỌI MÓN (không cần đăng nhập)
        [AllowAnonymous]
        [HttpPost("them-mon")]
        public async Task<ActionResult<ChiTietHoaDonViewModels>> PostCTHD(ChiTietHoaDonCreateVm model)
        {
            var( success, message) = await _chiTietHoaDonService.PostCTHD(model);
            if (success == null)
                return BadRequest(new { message = "Thêm món thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Thêm món thành công.", data = success });
        }
        // url: http://localhost:5001/api/chitiethoadon/{id}/capnhat-trangthai
        // PUT: Cập nhật trạng thái món
        // BẾP cập nhật trạng thái món
        [Authorize(Roles = "Bep")]
        [HttpPut("{id}/capnhat-trangthai")]
        public async Task<IActionResult> PutCTHD(int id, ChiTietHoaDonUpdataVm model)
        {
            var (success, message) = await _chiTietHoaDonService.PutCTHD(id, model);
            if (!success)
                return BadRequest(new { message = "Cập nhật trạng thái món thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Cập nhật trạng thái món thành công." });

        }


        //url:http://localhost:5001/api/chitiethoadon/{id}/Xoa-mon
        // DELETE: Xóa món
        [Authorize(Roles = "QuanLy")]
        [HttpDelete(("{id}/Xoa-mon"))]
        public async Task<IActionResult> DeleteCTHD(int id)
        {
            var (success, message) = await _chiTietHoaDonService.DeleteCTHD(id);
            if (!success)
                return BadRequest(new { message = "Xóa món thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Xóa món thành công." });
        }
    }
}
