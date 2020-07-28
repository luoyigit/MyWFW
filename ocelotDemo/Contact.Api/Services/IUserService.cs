using System.Threading.Tasks;
using Contact.Api.Dtos;

namespace Contact.Api.Services
{
    public interface IUserService
    {
        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns></returns>
        Task<BaseUserInfo> GetBaseUserInfoAsync(int userId);
    }
}