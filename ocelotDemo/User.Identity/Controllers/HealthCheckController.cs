using Microsoft.AspNetCore.Mvc;

namespace User.Identity.Controllers
{
    [Route("[controller]")]
    public class HealthCheckController : Controller
    {
        [HttpGet("")]
        [HttpHead("")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}