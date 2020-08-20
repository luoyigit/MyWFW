using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Infrastructure.DI
{
    public class Engine : IEngine
    {
        private IServiceProvider _serviceProvider;

        public Engine(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T Resolve<T>() where T : class
        {
           return (T)_serviceProvider.GetService(typeof(T));
        }
    }
}
