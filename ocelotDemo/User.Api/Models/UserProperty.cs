using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Api.Models
{
    /// <summary>
    ///用户属性
    /// </summary>
    public class UserProperty
    {
        /// <summary>
        /// 关联用户ID
        /// </summary>
        public int AppUserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Value { get; set; }
    }
}
