using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace MormorsBageri.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly IConfiguration _config;

        public HealthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("email")]
        public IActionResult CheckEmailConfig()
        {
            var fromEmail = Environment.GetEnvironmentVariable("EMAIL_FROM") ?? _config["Email:FromEmail"];

            if (string.IsNullOrEmpty(fromEmail))
            {
                return StatusCode(503, new { status = "Email configuration missing" });
            }
            return Ok(new { status = "Email configuration OK" });
        }
    }
}