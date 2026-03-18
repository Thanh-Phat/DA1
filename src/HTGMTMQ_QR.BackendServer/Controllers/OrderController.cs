using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Service;
using HTGMTMQ_QR.ViewModels.Systems.SanPham;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderService _orderService;

        public OrderController(OrderService orderService)
        {
            orderService = _orderService;
        }

        [AllowAnonymous]
        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem(AddItemVm model)
        {
            var result = await _orderService.AddItem(model);
            if (!result)
                return BadRequest(new { message = "Thêm món thất bại. Vui lòng kiểm tra lại." });
            return Ok(new { message = "Thêm món thành công." });
        }

        // GET: api/order/{mahd}
        [AllowAnonymous]
        [HttpGet("{mahd}")]
        public async Task<IActionResult> GetOrder(int mahd)
        {
            var items = await _orderService.GetOrder(mahd);
            return Ok(items);
        }
    }
}
