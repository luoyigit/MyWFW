using MessagePack;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ST.Infrastructure
{
    [MessagePackObject(true)]
    public class BaseResponse<T>
    {
        public BaseResponse() { }

        public BaseResponse(bool result, string msg = "", string errCode = "")
        {
            Success = result;
            Msg = msg;
            ErrorCode = errCode;
        }
        /// <summary>
        /// 数据集
        /// </summary>
        public T Data { get; set; }

        [DefaultValue(true)]
        public bool Success { get; set; }

        public string ErrorCode { get; set; }

        public string Msg { get; set; }

        public void FailBack(string msg, string errCode = "")
        {
            Success = false;
            Msg = msg;
            ErrorCode = errCode;
        }

        public void SuccessBack(string msg)
        {
            Success = true;
            Msg = msg;
        }
    }
}
