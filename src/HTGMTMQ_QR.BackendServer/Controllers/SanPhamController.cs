using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.SanPham;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SanPhamController : ControllerBase
    {
        private readonly SanPhamService _sanPhamService;

        public SanPhamController(SanPhamService sanPhamService)
        {
            _sanPhamService = sanPhamService;
        }

        // GET ALL
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet]
        public async Task<IActionResult> GetAllSanPham(string? filter = null, int pageIndex = 1, int pageSize = 10)
        {
            var result = await _sanPhamService.GetAllSanPham(filter, pageIndex, pageSize);
            return Ok(result);
        }

        // GET BY ID
        [Authorize(Roles = "QuanLy,ThuNgan")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSanPhamById(int id)
        {
            var result = await _sanPhamService.GetSanPhamById(id);

            if (result == null)
                return NotFound("Không tìm thấy sản phẩm.");

            return Ok(result);
        }

        // GET MENU
        [AllowAnonymous]
        [HttpGet("menu")]
        public async Task<IActionResult> GetMenu()
        {
            var result = await _sanPhamService.GetMenu();
            return Ok(result);
        }

        // POST
        [Authorize(Roles = "QuanLy")]
        [HttpPost]
        public async Task<IActionResult> PostSanPham(SanPhamCreateVm model)
        {
            var (success, message) = await _sanPhamService.PostSanPham(model);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // PUT
        [Authorize(Roles = "QuanLy")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSanPham(int id, SanPhamUpdateVm model)
        {
            var (success, message) = await _sanPhamService.PutSanPham(id, model);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // DELETE
        [Authorize(Roles = "QuanLy")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, message) = await _sanPhamService.Delete(id);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}