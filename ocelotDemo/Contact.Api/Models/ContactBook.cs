using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace Contact.Api.Models
{
    /// <summary>
    /// 通讯录
    /// </summary>
    [BsonIgnoreExtraElements]
    public class ContactBook
    {
        public ContactBook()
        {
            Contacts = new List<Contact>();
        }
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// 好友列表
        /// </summary>
        public List<Contact> Contacts { get; set; }
    }
}