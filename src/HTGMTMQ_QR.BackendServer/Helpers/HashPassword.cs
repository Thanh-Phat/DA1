using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace HTGMTMQ_QR.BackendServer.Helpers
{
    public static class PasswordHelper
    {
        // ✅ Hàm mã hóa mật khẩu
        public static string HashPassword(string password)
        {
            // Tạo salt ngẫu nhiên
            byte[] salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Mã hóa bằng PBKDF2 (an toàn, chuẩn .NET)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            // Trả về chuỗi dạng "salt:hash"
            return $"{Convert.ToBase64String(salt)}:{hashed}";
        }

        // ✅ (Tuỳ chọn) Hàm kiểm tra mật khẩu khi đăng nhập
        public static bool VerifyPassword(string password, string storedHash)
        {
            var parts = storedHash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hashToCompare = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 32));

            return hashToCompare == parts[1];
        }
    }
}
