using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Contact.Api.Controllers
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
