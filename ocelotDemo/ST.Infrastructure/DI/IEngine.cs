using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Infrastructure.DI
{
    public interface IEngine
    {
        T Resolve<T>() where T : class;
    }
}
