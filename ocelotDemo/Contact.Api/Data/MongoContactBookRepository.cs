using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contact.Api.Dtos;
using Contact.Api.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Contact.Api.Data
{
    public class MongoContactBookRepository:IContactBookRepository
    {
        private readonly ContactContext _context;

        private readonly ILogger<MongoContactBookRepository> _logger;

        public MongoContactBookRepository(ContactContext contactContext, ILogger<MongoContactBookRepository> logger)
        {
            _context = contactContext;
            _logger = logger;
        }
        
        public async Task<bool> UpdateUserInfoAsync(BaseUserInfo baseUserInfo, CancellationToken cancellationToken)
        {
            var contactBook =
                (await _context.ContactBooks.FindAsync(o => o.UserId == baseUserInfo.UserId, null, cancellationToken))
                .FirstOrDefault(cancellationToken);
            if (contactBook == null) return true;
            
            //该用户的所有好友
            var contactIds = contactBook.Contacts.Select(u => u.UserId);

            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.In(c => c.UserId, contactIds),
                            Builders<ContactBook>.Filter.ElemMatch(e => e.Contacts,contact => contact.UserId == baseUserInfo.UserId));
            
            var update = Builders<ContactBook>.Update
                .Set("Contacts.$.Name", baseUserInfo.Name)
                .Set("Contacts.$.Avatar", baseUserInfo.Avatar)
                .Set("Contacts.$.Title", baseUserInfo.Title)
                .Set("Contacts.$.Company", baseUserInfo.Company);
            //更新所有好友通讯录里的信息
            var updateResult = await _context.ContactBooks.UpdateManyAsync(filter, update, null, cancellationToken);
            return updateResult.MatchedCount == updateResult.ModifiedCount;
        }

        public async Task<List<Models.Contact>> GetContactsAsync(int userId, CancellationToken cancellationToken)
        {
            var contactBook = (await _context.ContactBooks.FindAsync(o => o.UserId == userId, null, cancellationToken)).FirstOrDefault(cancellationToken);
            if(contactBook !=null)
            {
                return contactBook.Contacts;
            }
            return new List<Models.Contact>();
        }

        public async Task<bool> TagContactAsync(int userId, int contactId, List<string> tags, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"用户{ userId} 更新自己的好友 {contactId} 标签信息 ");
            //查询userId通讯录,并匹配到 contactId好友
            var filter = Builders<ContactBook>.Filter.And(
                Builders<ContactBook>.Filter.Eq(b =>b.UserId,userId),
                            Builders<ContactBook>.Filter.ElemMatch(c =>c.Contacts,contact =>contact.UserId == contactId));
            
            //设置更新 contactId 的 Tags 属性
            var update = Builders<ContactBook>.Update.Set("Contacts.$.Tags", tags);
            var result = await _context.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }

        public async Task<bool> AddContactAsync(int userId, BaseUserInfo baseUserInfo, CancellationToken cancellationToken)
        {
            //检查该用户是否已经创建通讯录
            if((await _context.ContactBooks.CountDocumentsAsync(b=>b.UserId==userId,null,cancellationToken))==0)
            {
                _logger.LogInformation($"为用户:{userId} 创建通讯录! ");
                //创建通讯录
                await _context.ContactBooks.InsertOneAsync(new ContactBook{ UserId = userId},null,cancellationToken);
            }
            //检索条件
            var filter = Builders<ContactBook>.Filter.Eq(b => b.UserId, userId);
            var update = Builders<ContactBook>.Update.AddToSet(book => book.Contacts, new Contact.Api.Models.Contact()
            {
                UserId = baseUserInfo.UserId,
                Name =baseUserInfo.Name,
                Title =baseUserInfo.Title,
                Company = baseUserInfo.Company,
                Avatar = baseUserInfo.Avatar
            });
            //查询并添加
            var result = await _context.ContactBooks.UpdateOneAsync(filter, update, null, cancellationToken);

            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
        }
    }
}