using HTGMTMQ_QR.BackendServer.Data;
using HTGMTMQ_QR.BackendServer.Data.Entities;
using HTGMTMQ_QR.BackendServer.Service;
using Microsoft.AspNetCore.Mvc;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [ApiController]
    [Route("api/chatbot")]
    public class ChatbotController : ControllerBase
    {
        private readonly RecommendationService _recommendationService;
        private readonly ChatbotService _chatbotService;

        public ChatbotController(RecommendationService recommendationService, ChatbotService chatbotService)
        {
            _recommendationService = recommendationService;
            _chatbotService = chatbotService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string message)
        {
            var intent = await _chatbotService.Analyze(message);
            Console.WriteLine($"Type: {intent.type}, Price: {intent.price}");
            var foods = await _recommendationService.Recommend(intent, message);

            var firstFood = foods.FirstOrDefault();

            var messageText = firstFood != null
                ? $"Bạn có thể thử {firstFood.TenSP} nha 😋"
                : "Mình chưa tìm được món phù hợp 😢";

            return Ok(new
            {
                message = messageText,
                data = foods
            });
        }
    }
}

       