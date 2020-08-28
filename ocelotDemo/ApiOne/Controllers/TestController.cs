using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiOne.IntegrationEvents;
using ApiOne.IntegrationEvents.Events;
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
        private readonly ICatalogIntegrationEventService _catalogIntegrationEventService;
        public TestController(IConfiguration configuration, IGRpcConnection gRpcConnection, ICatalogIntegrationEventService catalogIntegrationEventService)
        {
            Configuration = configuration;
            _gRpcConnection = gRpcConnection;
            _catalogIntegrationEventService = catalogIntegrationEventService;
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


        public async Task<IActionResult> Publish()
        {
            var testEvent = new TestEvent("aaa");

            //通过本地事务实现原始目录数据库操作与集成事件日志之间的原子性
            // Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
            await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(testEvent);

            // Publish through the Event Bus and mark the saved event as published
            await _catalogIntegrationEventService.PublishThroughEventBusAsync(testEvent);
            return Ok("事件发送成功");
        }
    }
}
