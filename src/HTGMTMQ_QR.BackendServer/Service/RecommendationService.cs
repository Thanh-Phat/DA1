using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class RecommendationService
    {
        private readonly ApplicationDbcontext _context;
        public RecommendationService(ApplicationDbcontext context)
        {
            _context = context;
        }
        public async Task<List<SanPham>> Recommend(FoodIntent intent, string message)
        {
            var query = _context.SanPhams
                .Where(x => x.TrangThai == "Đang bán")
                .AsQueryable();

            // 🔥 fallback nếu AI ngu
            if (string.IsNullOrEmpty(intent.type))
            {
                var msg = message.ToLower();

                if (msg.Contains("lẩu")) intent.type = "lau";
                else if (msg.Contains("bò")) intent.type = "mon bo";
                else if (msg.Contains("heo")) intent.type = "mon heo";
                else if (msg.Contains("cá")) intent.type = "mon ca";
                else if (msg.Contains("uống")) intent.type = "thuc uong";
            }

            // 🔥 filter type
            if (!string.IsNullOrEmpty(intent.type))
            {
                query = query.Where(x => x.LoaiSP.ToLower().Contains(intent.type));
            }

            // 🔥 filter giá
            if (intent.price == "re")
                query = query.Where(x => x.DonGia < 50000);
            else if (intent.price == "trungbinh")
                query = query.Where(x => x.DonGia >= 50000 && x.DonGia <= 150000);
            else if (intent.price == "cao")
                query = query.Where(x => x.DonGia > 150000);

            // 🔥 đọc giá từ text (vd: 50k)
            var match = System.Text.RegularExpressions.Regex.Match(message, @"\d+");

            if (match.Success)
            {
                var priceLimit = int.Parse(match.Value);

                if (message.Contains("k"))
                    priceLimit *= 1000;

                query = query.Where(x => x.DonGia <= priceLimit);
            }

            // 🔥 random cho giống AI
            return await query
                .OrderBy(x => Guid.NewGuid())
                .Take(5)
                .ToListAsync();
        }
        public string MapType(string type)
        {
            if (string.IsNullOrEmpty(type)) return null;

            return type switch
            {
                "mon khai vi" => "Món khai vị",
                "mon dac trung" => "Món đặc trưng",
                "lau" => "Lẩu",
                "mon bo" => "Món bò",
                "mon ca" => "Món cá",
                "mon heo" => "Món heo",
                "mon goi kem" => "Món gọi kèm",
                "mon trang mieng" => "Món tráng miệng",
                "thuc uong" => "Thức uống",
                _ => null
            };
        }
    }
}