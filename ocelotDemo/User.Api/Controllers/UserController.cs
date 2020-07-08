using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace User.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        [HttpGet("GetUserList")]
        public IActionResult GetUserList()
        {
            return Ok("userlist");
        }

        [HttpPost("Add")]
        public IActionResult AddUser()
        {
            return Ok("add");
        }
    }
}
