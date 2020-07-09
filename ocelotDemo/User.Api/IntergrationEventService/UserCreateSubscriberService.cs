using System;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace User.Api.IntergrationEventService
{
    public class UserCreateSubscriberService : IUserCreateSubscriberService, ICapSubscribe
    {
        private readonly ILogger<UserCreateSubscriberService> _logger;

        public UserCreateSubscriberService(ILogger<UserCreateSubscriberService> logger)
        {
            _logger = logger;
        }

        [CapSubscribe("userApi.userCreated")]
        public async Task ChangeUserinfoAsync(UserInfoEventModel eventModel)
        {
            _logger.LogInformation($"时间:{DateTime.Now }  收到用户 {eventModel.Name} 修改信息事件");
        }
    }
}