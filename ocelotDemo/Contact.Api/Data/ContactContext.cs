using System.Collections.Generic;
using Contact.Api.Dtos;
using Contact.Api.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Contact.Api.Data
{
    public class ContactContext
    {
        private IMongoDatabase _database;
        private readonly AppSetting _setting;

        public ContactContext(IOptionsSnapshot<AppSetting> setting)
        {
            _setting = setting.Value;
            var client = new MongoClient(_setting.MongoConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(_setting.ContactDataBaseName);
            }
        }
        
        /// <summary>
        /// 确保表空间已经创建
        /// </summary>
        /// <param name="collectionName"></param>
        /// <returns></returns>
        private  void CheckAndCreateCollection(string collectionName)
        {
            var collectionList = _database.ListCollections().ToList();
            var collectionNames = new List<string>();
            //获取所有表名称
            collectionList.ForEach(b => collectionNames.Add(b["name"].AsString));
            //判断是否已经创建表
            if(!collectionNames.Contains(collectionName))
            {
                //如果没有创建，进行表空间的创建
                _database.CreateCollection(collectionName);
            }
        }
        /// <summary>
        /// 用户通讯录集合
        /// </summary>
        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                CheckAndCreateCollection("ContactBook");
                return _database.GetCollection<ContactBook>("ContactBook");
            }
        }
        /// <summary>
        /// 好友申请请求集合
        /// </summary>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                CheckAndCreateCollection("ContactApplyRequest");
                return _database.GetCollection<ContactApplyRequest>("ContactApplyRequest");
            }
        }
    }
}