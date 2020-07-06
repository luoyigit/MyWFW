using System.Threading.Tasks;

namespace User.Identity.Services
{
    /// <summary>
    /// 单一职责
    /// </summary>
    public interface IAuthCodeService
    {
        /// <summary>
        /// 根据手机号验证验证码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="authCode">验证码</param>
        /// <returns></returns>
        Task<bool> Validate(string phone, string authCode);
    }
}