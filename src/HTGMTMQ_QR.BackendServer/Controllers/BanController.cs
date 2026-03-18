using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.Ban;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Threading.Tasks;
using QRCode = HTGMTMQ_QR.BackendServer.Data.Entities.QRCode;


namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BanController : ControllerBase
    {
        private readonly BanService _banService;
        public BanController(BanService banService)
        {
            _banService = banService;
        }

        // URL GET: http://localhost:5001/api/ban/?filter={searchKeyword}&pageIndex=1&pageSize=10
        // Lấy danh sách (có filter + paging)
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllBan(string? filter = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = await _banService.GetAllBan(filter, pageIndex, pageSize);
            return Ok(result);
        }

        //URL Get: http://locahost:5001/api/ban/{id}
        //Lấy người dùng theo ID
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}")]
        public async Task<ActionResult<BanViewModels>> GetBanbyId(int id)
        {
            var ban = await _banService.GetBanbyId(id);
            return Ok(ban);
        }

        //URL Get: http://locahost:5001/api/ban/{id}/qr
        //Lấy ID QR theo bàn
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQRTheoBan(int id)
        {
            var qr = await _banService.GetQRTheoBan(id);
            if (qr == null)
                return NotFound(new { message = "Không tìm thấy QR cho bàn này." });
            return Ok(qr);
        }


        //Url: http://locahost:7066/api/ban/{id}/them-ban
        //Thêm bàn 
        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public async Task<ActionResult<BanViewModels>> PostBan(BanCreateVm model)
        {
            var result = await _banService.PostBan(model);
            if (!result)
                return BadRequest(new { message = "Thêm bàn thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Thêm bàn thành công." });
        }
        //Url: http://locahost:7066/api/ban/{id}/tao-qr
        //Tạo mã QR cho bàn
        [Authorize(Roles = "QuanLy")]
        [HttpPost("{id}/tao-qr")]
        public async Task<IActionResult> PostQRCode(int id)
        {
            var host = $"{Request.Scheme}://{Request.Host}";    
            var result = await _banService.PostQRCode(id, host);
            if (result == null)
                return BadRequest(new { message = "Tạo QR thất bại. Vui lòng kiểm tra lại." });
            return Ok(result);
        }

        //Url: http://locahost:7066/api/ban/{id}/cap-nhat-ban
        //Cập nhật thông tin bàn
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBan(int id,BanUpdateVm model)
        {
           var result = await _banService.PutBan(id, model);
            if (!result)
                return BadRequest(new { message = "Cập nhật bàn thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Cập nhật bàn thành công." });
        }

        //Url: http://locahost:7066/api/ban/{id}/xoa-ban
        //Xóa bàn
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBan(int id)
        {
            var result = await _banService.DeleteBan(id);
            if (!result)
                return BadRequest(new { message = "Xóa bàn thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Xóa bàn thành công." });
        }
    }
}

