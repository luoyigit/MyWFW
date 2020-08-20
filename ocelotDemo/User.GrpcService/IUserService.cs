using MagicOnion;
using ST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using User.GrpcService.Models;
using User.GrpcService.Models.Request;
using User.GrpcService.Models.Response;

namespace User.GrpcService
{
    public interface IUserService : IService<IUserService>
    {
        UnaryResult<BaseResponse<UserInfo>> GetBaseUserInfoAsync(GetUserInfoByIdRequest model); 
    }
}
