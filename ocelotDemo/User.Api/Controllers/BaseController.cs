using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using User.Api.Models.Dtos;

namespace User.Api.Controllers
{
    public class BaseController: Controller
    {
        private readonly ILogger<BaseController> _logger;

        public BaseController(ILogger<BaseController> logger)
        {
            _logger = logger;
        }

        protected UserIdentity UserIdentity
        {
            get
            {
                var userIdentity = new UserIdentity
                {
                    UserId = Convert.ToInt16(User.Claims.FirstOrDefault(b => b.Type == "sub").Value),
                    Title = User.Claims.FirstOrDefault(b => b.Type == "title").Value,
                    Company = User.Claims.FirstOrDefault(b => b.Type == "company").Value,
                    Avatar = User.Claims.FirstOrDefault(b => b.Type == "avatar").Value,
                    Name = User.Claims.FirstOrDefault(b => b.Type == "name").Value
                };
                return userIdentity;
            }
        }
    }
}
