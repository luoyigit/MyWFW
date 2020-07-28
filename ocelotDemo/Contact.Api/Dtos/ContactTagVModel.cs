using System.Collections.Generic;

namespace Contact.Api.Dtos
{
    /// <summary>
    /// 给好友打标签
    /// </summary>
    public class ContactTagVModel
    {
        public ContactTagVModel()
        {
            Tags = new List<string>();
        }
        /// <summary>
        /// 好友ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 标签信息
        /// </summary>
        public List<string> Tags { get; set; }
    }
}