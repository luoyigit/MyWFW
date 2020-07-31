using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Contact.Api.Dtos;
using Contact.Api.Models;

namespace Contact.Api.Data
{
    /// <summary>
    /// 好友通讯录仓储
    /// </summary>
    public interface IContactBookRepository
    {
        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="baseUserInfo">要更新的用户信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> UpdateUserInfoAsync(BaseUserInfo baseUserInfo, CancellationToken cancellationToken);

        Task<bool> UpdateContactInfo(UserIdentity userInfo, CancellationToken cancellationToken);

        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>用户好友列表</returns>
        Task<List<Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken);


        /// <summary>
        /// 给好友打标签
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="contactId">好友ID</param>
        /// <param name="tags">标签列表</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken);


        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="baseUserInfo">待添加的用户信息</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddContactAsync(int userId, BaseUserInfo baseUserInfo, CancellationToken cancellationToken);

        /// <summary>
        /// 添加测试数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> AddTestDataAsync(Test model, CancellationToken cancellationToken);
    }
}