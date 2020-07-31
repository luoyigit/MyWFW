using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contact.Api.Models
{
    /// <summary>
    /// 测试表
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Test
    {
        public string Title { get; set; }


        public DateTime CreateTime { get; set; }
    }
}
