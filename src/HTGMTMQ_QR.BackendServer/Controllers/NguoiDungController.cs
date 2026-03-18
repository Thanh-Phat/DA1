using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.Common;
using HTGMTMQ_QR.ViewModels.Systems.NguoiDung;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Authorize(Roles = "QuanLy")]
    [Route("api/[controller]")]
    [ApiController]
    public class NguoiDungController : ControllerBase
    {
        private readonly NguoiDungService _NguoiDungservice;

        public NguoiDungController(NguoiDungService NguoiDungservice)
        {
            _NguoiDungservice = NguoiDungservice;
        }
        // URL GET: http://localhost:5001/api/nguoidung/?filter={searchKeyword}&pageIndex=1&pageSize=10
        // Lấy danh sách (có filter + paging)
        [HttpGet]
        public async Task<IActionResult> GetAllNguoiDung(string? filter= null, int pageIndex= 1, int pageSize = 10)
        {
           var result = await _NguoiDungservice.GetAllNguoiDung(filter, pageIndex, pageSize);
            return Ok(result);
        }

        //URL Get: http://locahost:5001/api/nguoidung/{id}
        //Lấy người dùng theo ID
        [HttpGet("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> GetNguoiDungbyId(int id)
        { 
            var result = await _NguoiDungservice.GetNguoiDungbyId(id);
            if (result == null)
                return NotFound("Không tìm thấy người dùng.");
            return Ok(result);
        }
        //URL POST: http://locahost:5001/api/nguoidung
        //Thêm người dùng
        [HttpPost]
        public async Task<ActionResult<NguoiDungViewModels>> PostNguoiDung(NguoiDungCreateVm model)
        {
            var (success, message) = await _NguoiDungservice.PostNguoiDung(model);
            if (success == null)
                return BadRequest("Không thể tạo người dùng mới.");
            return Ok(new{message});
        }

        //URL Put: http://locahost:5001/api/nguoidung/{id}
        // Cập nhật thông tin người dùng
        [HttpPut("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> PutNguoiDung(int id, NguoiDungUpdataVm model)
        { 
            if (id != model.MaND)
            {
                return BadRequest("ID không khớp.");
            };
            var (success, message) = await _NguoiDungservice.PutNguoiDung(id, model);
            if (success == null)
                return NotFound("Không tìm thấy người dùng hoặc cập nhật thất bại.");
            return Ok(new {message});
        }
        //URL Delete http://locahost:5001/api/nguoidung/{id}
        //Xóa người dùng
        [HttpDelete("{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> DeleteNguoiDung(int id)
        {
            var (success, message) = await _NguoiDungservice.DeleteNguoiDung(id);
            if (!success)
                return NotFound("Không tìm thấy người dùng hoặc xóa thất bại.");
            return Ok(new { message = "Xóa người dùng thành công." });
        }
        //URL Put http://locahost:5001/api/nguoidung/doimatkhau/{id}
        //Đổi mật khẩu
        [HttpPut("doimatkhau/{id}")]
        public async Task<ActionResult<NguoiDungViewModels>> DoiMatKhauNguoiDung(int id, DoiMatKhauVm model)
        {
            if (id != model.MaND)
            {
                return BadRequest("ID không khớp.");
            };
            var (success, message) = await _NguoiDungservice.DoiMatKhauNguoiDung(id, model);
            if (!success)
                return NotFound(new { message });
            return Ok(new { message });
        }


    }
}
