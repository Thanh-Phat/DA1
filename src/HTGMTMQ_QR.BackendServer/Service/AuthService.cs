using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class AuthService
    {
        private readonly ApplicationDbcontext _context;
        private readonly IConfiguration _config;

        public AuthService(ApplicationDbcontext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<(bool success, string message, object? data)> Login(string username, string password)
        {
            // validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return (false, "Thiếu tài khoản hoặc mật khẩu.", null);

            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                return (false, "JWT chưa được cấu hình.", null);

            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(x => x.TenDangNhap == username);

            if (user == null || !PasswordHelper.VerifyPassword(password, user.MatKhau))
                return (false, "Sai tài khoản hoặc mật khẩu.", null);

            if (user.TrangThai == false)
                return (false, "Tài khoản đang bị khóa.", null);

            // tạo token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.TenDangNhap),
                new Claim(ClaimTypes.Role, user.VaiTro),
                new Claim(ClaimTypes.NameIdentifier, user.MaND.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireHours = int.Parse(_config["Jwt:ExpireHours"] ?? "5");

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(expireHours),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (true, "Đăng nhập thành công", new
            {
                token = tokenString,
                user = new
                {
                    user.MaND,
                    user.TenDangNhap,
                    user.HoTen,
                    user.VaiTro
                }
            });
        }
    }
}
