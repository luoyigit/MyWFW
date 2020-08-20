using MagicOnion;
using MagicOnion.Server;
using ST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.GrpcService;
using User.GrpcService.Models;
using User.GrpcService.Models.Request;
using User.GrpcService.Models.Response;

namespace User.Grpc.Services
{
    public class UserService : ServiceBase<IUserService>, IUserService
    {
        public async UnaryResult<BaseResponse<UserInfo>> GetBaseUserInfoAsync(GetUserInfoByIdRequest model)
        {
            await Task.Yield();
            var result = new UserInfo();
            result.Name = "luoyi";
            result.UserId = 3;
            return result.ToResponse(); 
        }
    }
}
