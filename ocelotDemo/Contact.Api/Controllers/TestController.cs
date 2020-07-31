using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Contact.Api.Controllers
{
    [Route("[controller]")]
    public class TestController : Controller
    {
        public IConfiguration Configuration;
        public TestController(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            var tag = Configuration["LocalService:HostTag"];
            return Ok(tag);
        }
    }
}
