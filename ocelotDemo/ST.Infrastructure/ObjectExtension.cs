using System;
using System.Collections.Generic;
using System.Text;

namespace ST.Infrastructure
{
    public static class ObjectExtension
    {
        public static BaseResponse<T> ToResponse<T>(this T obj)
        {
            if (obj == null)
                throw new NullReferenceException("Cannot be an empty object");
            else
            {
                var result = new BaseResponse<T>();
                result.Data = (T)obj;
                return result;
            }
        }
    }
}
