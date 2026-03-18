using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Helpers;
using HTGMTMQ_QR.BackendServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly AuthService _authService;
        public LoginController(AuthService authService)
        {
            _authService = authService;
        }

        public class LoginRequest
        {
            public string TenDangNhap { get; set; }
            public string MatKhau { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var (success, message, data) = await _authService.Login(model.TenDangNhap, model.MatKhau);

            if (!success)
                return Unauthorized(new { message });

            return Ok(new { message, data });
        }
    }
}
