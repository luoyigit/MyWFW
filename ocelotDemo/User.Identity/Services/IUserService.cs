using System.Threading.Tasks;
using User.Identity.Models;

namespace User.Identity.Services
{
    /// <summary>
    /// 单一职责
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 检查手机号是否已经注册，如果没有注册则创建用户
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        Task<BaseUserInfo> GetOrCreateAsync(string phone);
    }
}
