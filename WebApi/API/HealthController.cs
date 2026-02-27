using Microsoft.AspNetCore.Mvc;

namespace MyWarehouse.WebApi.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Status = "Healthy",
                Service = "WebApi",
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
