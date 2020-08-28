using ApiTwo.IntegrationEvents.Events;
using EventBus.Abstractions;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTwo.IntegrationEvents.EventHandling
{
    public class TestEventHandler : IIntegrationEventHandler<TestEvent>
    {
        private readonly ILogger<TestEventHandler> _logger;

        public TestEventHandler(ILogger<TestEventHandler> logger)
        {
            _logger = logger;
        }
        public async Task Handle(TestEvent @event)
        {
            _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})", @event.Id, Program.AppName, @event);
            var tt = @event.Name;
            await Task.CompletedTask;
            
        }
    }
}
