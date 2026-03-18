using HTGMTMQ_QR.BackendServer.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTGMTMQ_QR.BackendServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QrController : ControllerBase
    {
        private readonly QrService _service;

        public QrController(QrService service)
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpGet("{maban}")]
        public async Task<IActionResult> AccessByQr(int maban)
        {
            var (success, message, data) = await _service.AccessByQr(maban);

            if (!success)
                return NotFound(new { message });

            return Ok(data);
        }
    }
}