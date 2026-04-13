using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using System.Text;
using System.Text.Json;

namespace HTGMTMQ_QR.BackendServer.Service
{
    public class ChatbotService
    {
        private readonly HttpClient _httpClient;

        public ChatbotService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<FoodIntent> Analyze(string userMessage)
        {
            // 🔐 lấy API key từ .env
            var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                Console.WriteLine("GEMINI_API_KEY chưa được thiết lập!");
                return new FoodIntent();
            }

            var prompt = $@"
                        Bạn là chatbot tư vấn món ăn.

                        Trích xuất yêu cầu thành JSON:

                        {{
                          ""taste"": ""cay/ngot/man/null"",
                          ""price"": ""re/trungbinh/cao/null"",
                          ""type"": ""mon khai vi/mon dac trung/lau/mon bo/mon ca/mon heo/mon goi kem/mon trang mieng/thuc uong/null""
                        }}

                        Chỉ trả JSON.

                        User: {userMessage}
                        ";

            var body = new
            {
                contents = new[]
                {
                                    new
                                    {
                                        parts = new[]
                                        {
                                            new { text = prompt }
                                        }
                                    }
                                }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var res = await _httpClient.PostAsync(
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={apiKey}",
                content
            );
            if (!res.IsSuccessStatusCode)
            {
                Console.WriteLine("Lỗi API Gemini: " + res.StatusCode);
                return new FoodIntent();
            }


            var json = await res.Content.ReadAsStringAsync();

            Console.WriteLine("Gemini response: " + json);

            return ParseGemini(json);
        }

        private FoodIntent ParseGemini(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);

                // check candidates tồn tại
                if (!doc.RootElement.TryGetProperty("candidates", out var candidates)
                    || candidates.GetArrayLength() == 0)
                {
                    return new FoodIntent();
                }

                var candidate = candidates[0];

                // check content
                if (!candidate.TryGetProperty("content", out var content))
                    return new FoodIntent();

                if (!content.TryGetProperty("parts", out var parts)
                    || parts.GetArrayLength() == 0)
                {
                    return new FoodIntent();
                }

                var part = parts[0];

                if (!part.TryGetProperty("text", out var textElement))
                    return new FoodIntent();

                var text = textElement.GetString();

                if (string.IsNullOrEmpty(text))
                    return new FoodIntent();

                // emove markdown
                text = text.Replace("```json", "")
                           .Replace("```", "")
                           .Trim();

                // parse JSON an toàn
                var result = JsonSerializer.Deserialize<FoodIntent>(text, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new FoodIntent();

                // normalize
                result.taste = result.taste?.ToLower();
                result.price = result.price?.ToLower();
                result.type = result.type?.ToLower();

                return result;
            }
            catch
            {
                return new FoodIntent();
            }
        }
    }
}