using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCore.CAP;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ST.Infrastructure;
using User.Api.Data;
using User.Api.Models;
using User.Api.Models.Dtos;

namespace User.Api.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/users")]
    public class UserController : BaseController
    {
        private readonly UserContext _userContext;
        private readonly ILogger<UserController> _logger;
        private readonly ICapPublisher _publisher;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userContext"></param>
        /// <param name="logger"></param>
        public UserController(UserContext userContext, ILogger<UserController> logger, ICapPublisher publisher) : base(logger)
        {
            _userContext = userContext;
            _logger = logger;
            _publisher = publisher;
        }

        /// <summary>
        /// 登录用户获取个人信息
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var user = await _userContext.Users
                .AsNoTracking()
                .Include(o => o.Properties)
                .SingleOrDefaultAsync(o => o.Id == UserIdentity.UserId);
            if (user == null)
                throw new UserOperationException($"错误的用户编号：{UserIdentity.UserId}");

            return Ok(user);
        }

        /// <summary>
        /// 用户更新个人信息（Patch做部分更新）
        /// </summary>
        /// <returns></returns>
        [Route("")]
        [HttpPatch]
        public async Task<IActionResult> Patch([FromBody] JsonPatchDocument<AppUser> patch)
        {
            var user = await _userContext.Users
                .Include(o => o.Properties)
                .SingleOrDefaultAsync(o => o.Id == UserIdentity.UserId);
            //修改更新到实体对象
            patch.ApplyTo(user);
            //如果有修改Properties, 不追踪 AppUser 实体的 Properties 属性 单独通过以下的方法进行处理
            if (user.Properties != null)
            {

                foreach (var item in user.Properties)
                {
                    _userContext.Entry(item).State = EntityState.Detached;
                }

                //Properties 属性 单独通过以下的方法进行处理
                //获取原来用户所有的Properties, 必须使用 AsNoTracking()，否则会自动附加到用户属性上
                var originProperties = await _userContext.UserProperties.AsNoTracking().Where(b => b.AppUserId == UserIdentity.UserId).ToListAsync();

                foreach (var item in originProperties.Where(item => !user.Properties.Exists(b => b.Key == item.Key && b.Value == item.Value)))
                {
                    //如果不存在做删除操作
                    _userContext.Remove(item);
                }
                foreach (var item in user.Properties.Where(item => !originProperties.Exists(b => b.Key == item.Key && b.Value == item.Value)))
                {
                    //如果不存在做新增操作
                    _userContext.Add(item);
                }
            }
            //更新用户信息
            _userContext.Users.Update(user);
            _userContext.SaveChanges();

            return Json(user);
        }
        /// <summary>
        /// 检查或则创建用户 并返回用户基本信息
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <returns>用户ID</returns>
        [Route("check-or-create")]
        [HttpPost]
        public async Task<IActionResult> CheckOrCreateUser(string phone)
        {
            var user = await _userContext.Users.SingleOrDefaultAsync(b => b.Phone == phone);
            if (user == null)
            {
                using (var transaction = _userContext.Database.BeginTransaction())
                {
                    //用户不存在，直接创建用户
                    user = new AppUser { Phone = phone };
                    await _userContext.Users.AddAsync(user);
                    await _userContext.SaveChangesAsync();
                    _publisher.Publish("userApi.userCreated", new UserIdentity
                    {
                        UserId = user.Id,
                        Title = user.Title,
                        Company = user.Company,
                        Name = user.Name,
                        Avatar = user.Avatar
                    });
                    await transaction.CommitAsync();
                }
            }
            return Ok(new UserIdentity
            {
                UserId = user.Id,
                Name = user.Name,
                Title = user.Title,
                Company = user.Company,
                Avatar = user.Avatar
            });
        }
        /// <summary>
        /// 更新用户标签数据
        /// </summary>
        /// <param name="tags">用户标签数据</param>
        /// <returns></returns>       
        [HttpPut]
        [Route("tags")]
        public async Task<IActionResult> UpdateTags([FromBody] List<string> tags)
        {
            var originTags = await _userContext.UserTags.Where(b => b.AppUserId == UserIdentity.UserId).ToListAsync();
            var newTags = tags.Except(originTags.Select(b => b.Tag));
            await _userContext.UserTags.AddRangeAsync(newTags.Select(b => new UserTag
            {
                CreateTime = DateTime.Now,
                AppUserId = UserIdentity.UserId,
                Tag = b
            }));
            await _userContext.SaveChangesAsync();
            return Ok();
        }
        /// <summary>
        /// 获取用户标签数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetUserTagsAsync()
        {
            return Ok(await _userContext.UserTags.Where(b => b.AppUserId == UserIdentity.UserId).ToListAsync());
        }
        /// <summary>
        /// 通过手机号查询信息
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <returns>人员信息</returns>
        /// 
        [HttpPost]
        [Route("search/{phone}")]
        public async Task<IActionResult> Search(string phone)
        {
            return Ok(await _userContext.Users.Include(b => b.Properties).SingleOrDefaultAsync(b => b.Phone == phone));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-userinfo/{id}")]
        public async Task<IActionResult> GetUserBaseInfoAsync(int id)
        {
            var entity = await _userContext.Users.SingleOrDefaultAsync(b => b.Id == id);

            if (entity == null)
            {
                _logger.LogInformation($"查询用户编号 {id},信息不存在");
                throw new UserOperationException("用户不存在");
            }
            return Ok(new
            {
                UserId = entity.Id,
                entity.Name,
                entity.Title,
                entity.Company,
                entity.Avatar
            });
        }
    }
}
