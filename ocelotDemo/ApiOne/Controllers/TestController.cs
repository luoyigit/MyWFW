using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiOne.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return Content("one");
        }
    }
}
