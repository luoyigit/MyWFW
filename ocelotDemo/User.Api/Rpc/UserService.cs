using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using ST.Infrastructure;
using ST.Infrastructure.DI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Api.Data;
using User.GrpcService;
using User.GrpcService.Models.Request;
using User.GrpcService.Models.Response;

namespace User.Api.Rpc
{
    public class UserService: ServiceBase<IUserService>, IUserService
    {
        private readonly UserContext _userContext;
        public UserService()
        {
            //var serviceProvidersFeature = HttpContext.Features.Get<IServiceProvidersFeature>();
            //var services = serviceProvidersFeature.RequestServices;
            //var service = (IServiceProvider)services.GetService(typeof(IServiceProvider));
            _userContext = EngineContext.Engine.Resolve<UserContext>();
        }
        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async UnaryResult<BaseResponse<UserInfo>> GetBaseUserInfoAsync(GetUserInfoByIdRequest model)
        {
            var entity = await _userContext.Users.SingleOrDefaultAsync(b => b.Id == model.UserId);
            if (entity == null)
            {
                throw new UserOperationException("用户不存在");
            }
            else
            {
                var result = new UserInfo();
                result.Name = entity.Name;
                result.UserId = entity.Id;
                result.Title = entity.Title;
                result.Company = entity.Company;
                result.Avatar = entity.Avatar;

                return result.ToResponse();
            }
            //await Task.Yield();
            //var result = new UserInfo();
            //result.Name = "luoyi";
            //result.UserId = 3;
            //return result.ToResponse();
        }
    }
}
