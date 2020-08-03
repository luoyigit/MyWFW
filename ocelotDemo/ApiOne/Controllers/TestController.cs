using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace ApiOne.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public IConfiguration Configuration;
        public TestController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            var tag = Configuration["LocalService:HostTag"];
            return Ok(tag);
        }
    }
}
