using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ApiTwo.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            return Content("two");
        }
    }
}
