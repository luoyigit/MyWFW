using EventBus.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiOne.IntegrationEvents.Events
{
    public class TestEvent: IntegrationEvent
    {
        public string Name { get; set; }

        public TestEvent(string name)
        {
            Name = name;
        }
    }
}
