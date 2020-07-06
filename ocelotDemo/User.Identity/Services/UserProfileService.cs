using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;

namespace User.Identity.Services
{
    public class UserProfileService:IProfileService
    {
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(ILogger<UserProfileService> logger)
        {
            _logger = logger;
        }
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userId = context.Subject.GetSubjectId();
            if (userId != null)
            {
                //赋值给IssuedClaims后便能在token中正常获取到所需Claim
                context.IssuedClaims.AddRange(context.Subject.Claims);
            }
            context.LogIssuedClaims(_logger);
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            _logger.LogDebug("IsActive called from: {caller}", context.Caller);
            var userId = context.Subject.GetSubjectId();
            if (!string.IsNullOrEmpty(userId))
            {
                context.IsActive = true;
            }
            return  Task.CompletedTask;
        }
    }
}