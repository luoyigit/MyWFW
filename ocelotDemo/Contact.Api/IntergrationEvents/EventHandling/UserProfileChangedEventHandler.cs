using System;
using System.Threading;
using System.Threading.Tasks;
using Contact.Api.Data;
using Contact.Api.Dtos;
using Contact.Api.Models;
using Contact.API.IntergrationEvents.Events;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Contact.API.IntergrationEvents.EventHandling
{
    public class UserProfileChangedEventHandler : ICapSubscribe
    {
        private readonly IContactBookRepository _contactRepository;
        private readonly ILogger<UserProfileChangedEventHandler> _logger;
        private readonly AppSetting _setting;

        public UserProfileChangedEventHandler(IContactBookRepository contactRepository,
            ILogger<UserProfileChangedEventHandler> logger,
            IOptionsSnapshot<AppSetting> setting)
        {
            _contactRepository = contactRepository;
            _logger = logger;
            _setting = setting.Value;
        }

        [CapSubscribe("finbook_userapi_userprofilechanged")]
        public void UpdateContactInfo(UserProfileChangedEvent @event)
        {
            var token = new CancellationToken();
            _contactRepository.UpdateContactInfo(new UserIdentity
            {
                UserId = @event.UserId,
                Name = @event.Name,
                Avatar = @event.Avatar,
                Title = @event.Title,
                Company = @event.Company,
            }, token);

            _contactRepository.AddTestDataAsync(new Test() { Title = $"api:{_setting.Flag},title:{@event.Title}", CreateTime = DateTime.Now }, token);
            _logger.LogInformation($"finbook_userapi_userprofilechanged 接收成功!");
        }
    }
}
