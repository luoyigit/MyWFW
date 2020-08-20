using ST.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace User.GrpcService.Models.Request
{
    public class GetUserInfoByIdRequest:BaseRequest
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [Required]
        public int UserId { get; set; }


        public GetUserInfoByIdRequest(int userId)
        {
            UserId = userId;
        }
    }
}
