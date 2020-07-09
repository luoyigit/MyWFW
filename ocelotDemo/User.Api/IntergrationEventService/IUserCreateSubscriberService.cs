using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Api.IntergrationEventService
{
    public interface IUserCreateSubscriberService
    {
        Task ChangeUserinfoAsync(UserInfoEventModel eventModel);
    }
}
