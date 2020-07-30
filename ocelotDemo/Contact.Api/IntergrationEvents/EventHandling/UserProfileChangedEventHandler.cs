﻿using System.Threading;
using System.Threading.Tasks;
using Contact.Api.Data;
using Contact.Api.Dtos;
using Contact.API.IntergrationEvents.Events;
using DotNetCore.CAP;
using Microsoft.Extensions.Logging;

namespace Contact.API.IntergrationEvents.EventHandling
{
    public class UserProfileChangedEventHandler : ICapSubscribe
    {
        private readonly IContactBookRepository _contactRepository;
        private readonly ILogger<UserProfileChangedEventHandler> _logger;

        public UserProfileChangedEventHandler(IContactBookRepository contactRepository,
            ILogger<UserProfileChangedEventHandler> logger)
        {
            _contactRepository = contactRepository;
            _logger = logger;
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

            _logger.LogTrace($"finbook_userapi_userprofilechanged 接收成功!");
        }
    }
}
