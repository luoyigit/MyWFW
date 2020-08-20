using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ST.Infrastructure.DI
{
    public class EngineContext
    {
        public static IEngine _engine;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void Init(IEngine engine)
        {
            if(_engine == null)
            {
                _engine = engine;
            }
        }


        public static IEngine Engine
        {
            get
            {
                return _engine;
            }
        }
    }
}
