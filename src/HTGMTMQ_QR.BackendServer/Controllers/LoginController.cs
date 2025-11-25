using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Helpers;
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
        private readonly ApplicationDbcontext _context;
        private readonly IConfiguration _config;

        public LoginController(ApplicationDbcontext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public class LoginRequest
        {
            public string TenDangNhap { get; set; }
            public string MatKhau { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest model)
        {
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.TenDangNhap == model.TenDangNhap);
            if (user == null)
            {
                return Unauthorized("Sai tài khoản.");
            }
            if (!PasswordHelper.VerifyPassword(model.MatKhau, user.MatKhau))
            {
                return Unauthorized("Sai mật khẩu.");
            }
            if (user.TrangThai == false)
            {
                return Unauthorized("Tài khoản đang bị khóa");
            }


            // tạo JWT Token

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim("id", user.MaND.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(5),
                signingCredentials: creds
             );
            return Ok(new
            { 
                message = "Đăng nhập thành công",
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new
                {
                    user.MaND,
                    user.TenDangNhap,
                    user.HoTen,
                    user.VaiTro,
                } 
            });
        }


    }
}
