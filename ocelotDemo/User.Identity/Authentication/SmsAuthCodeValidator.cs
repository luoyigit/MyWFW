using IdentityServer4.Models;
using IdentityServer4.Validation;
using System.Security.Claims;
using System.Threading.Tasks;
using User.Identity.Models;
using User.Identity.Services;

namespace User.Identity.Authentication
{
    public class SmsAuthCodeValidator : IExtensionGrantValidator
    {
        private readonly IAuthCodeService _authCodeService;
        private readonly IUserService _userService;
        public string GrantType => "sms_auth_code";

        public SmsAuthCodeValidator(IAuthCodeService authCodeService, IUserService userService)
        {
            _authCodeService = authCodeService;
            _userService = userService;
        }

        //connect/token 访问进来
        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];

            var errorValidationResult = new GrantValidationResult(TokenRequestErrors.InvalidGrant);
            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(code))
            {
                context.Result = errorValidationResult;
                return;
            }
            //检查手机号和验证码是否匹配
            if (!await _authCodeService.Validate(phone, code))
            {
                context.Result = errorValidationResult;
                return;
            }
            var userInfo = await _userService.GetOrCreateAsync(phone);
            if (userInfo == null)
            {
                //如果用户ID小于等于0 ，验证失败
                context.Result = errorValidationResult;
                return;
            }

            #region 测试代码
            //var userInfo = new BaseUserInfo();
            //userInfo.Name = "luoyi";
            //userInfo.Title = "denglu";
            //userInfo.Company = "hw";
            //userInfo.Avatar = "2222";
            //userInfo.UserId = 3;
            #endregion

            //构建UserClaims
            var claims = new Claim[]
            {
                new Claim("name",userInfo.Name??string.Empty),
                new Claim("title",userInfo.Title??string.Empty),
                new Claim("company",userInfo.Company??string.Empty),
                new Claim("avatar",userInfo.Avatar??string.Empty),
                new Claim("sub",userInfo.UserId.ToString())
            };
            context.Result = new GrantValidationResult(userInfo.UserId.ToString(), GrantType, claims);
        }
    }
}
