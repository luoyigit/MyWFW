using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Infrastructure
{
    public class UserOperationException : Exception
    {
        public UserOperationException() { }

        public UserOperationException(string message) : base(message) { }

        public UserOperationException(string message, Exception exception) : base(message, exception) { }

    }
}
