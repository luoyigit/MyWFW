using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Filters
{
    public class RolesAuthorizationRequirement : AuthorizationHandler<RolesAuthorizationRequirement>, IAuthorizationRequirement
    {
        public IEnumerable<string> AllowedRoles { get; }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RolesAuthorizationRequirement requirement)
        {
           //if(requirement.AllowedRoles.Any(r => context.User.IsInRole(r)))
           // {
           //     context.Succeed(requirement);
           // }
            return Task.CompletedTask;
        }
    }
}
