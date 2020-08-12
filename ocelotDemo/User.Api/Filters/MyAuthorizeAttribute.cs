using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace User.Api.Filters
{
    /// <summary>
    /// 重写实现处理授权失败时返回json,避免跳转登录页
    /// </summary>
    public class ApiAuthorize : AuthorizeAttribute
    {
        //protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        //{
        //    base.HandleUnauthorizedRequest(filterContext);

        //    var response = filterContext.Response = filterContext.Response ?? new HttpResponseMessage();
        //    response.StatusCode = HttpStatusCode.Forbidden;
        //    var content = new 
        //    {
        //        success = false,
        //        errs = new[] { "服务端拒绝访问：你没有权限，或者掉线了" }
        //    };
        //    response.Content = new StringContent(Json.Encode(content), Encoding.UTF8, "application/json");
        //}
    }
}
