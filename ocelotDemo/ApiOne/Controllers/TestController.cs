using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ST.Common.MagicOnion;
using User.GrpcService;

namespace ApiOne.Controllers
{
    //[Authorize]
    [Route("[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        public IConfiguration Configuration;
        private IGRpcConnection _gRpcConnection;
        private string _userRpcServiceName = "UserRpc";
        public TestController(IConfiguration configuration, IGRpcConnection gRpcConnection)
        {
            Configuration = configuration;
            _gRpcConnection = gRpcConnection;
        }
        [HttpGet("")]
        public IActionResult Index()
        {
            var tag = Configuration["LocalService:HostTag"];
            return Ok(tag);
        }

        [HttpGet("Index2")]
        public async Task<IActionResult> Index2(int userId)
        {
            var _userService =await _gRpcConnection.GetRemoteService<IUserService>(_userRpcServiceName);
            var rpcData = await _userService.GetBaseUserInfoAsync(new User.GrpcService.Models.Request.GetUserInfoByIdRequest(userId));
            return Json(rpcData);
        }
    }
}
