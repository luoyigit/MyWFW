using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.FormModels
{
    public class CreateProjectFormModel
    {

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 项目基本介绍
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 项目logo
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 原bpfile
        /// </summary>
        public string OriginBPFile { get; set; }
        /// <summary>
        /// 转化后bffile
        /// </summary>
        public string FormatBPFile { get; set; }

        /// <summary>
        /// 是否显示敏感信息
        /// </summary>
        public int ShowSecurityInfo { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProvinceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ProvinceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CityId { get; set; }


        /// <summary>
        /// 
        /// </summary>
        public string CityName { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string AreaId { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 注册日期
        /// </summary>
        public DateTime RegisterTime { get; set; }

        /// <summary>
        /// 融资比例
        /// </summary>
        public int FinPercentage { get; set; }

        /// <summary>
        /// 融资阶段
        /// </summary>
        public int FinStage { get; set; }
        /// <summary>
        /// 融资金额
        /// </summary>
        public string FinMoney { get; set; }
        /// <summary>
        /// 收入
        /// </summary>
        public int Income { get; set; }

        /// <summary>
        /// 利润
        /// </summary>
        public int Revenue { get; set; }

        /// <summary>
        /// 估值
        /// </summary>
        public string Valuation { get; set; }
        /// <summary>
        /// 项目标签
        /// </summary>
        public string Tags { get; set; }
        /// <summary>
        /// 佣金分配方式 枚举  线下 商议 等比例分配
        /// </summary>
        public string BrokerageOption { get; set; }


        /// <summary>
        /// 原项目id
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// 引用项目id
        /// </summary>
        public int ReferenceId { get; set; }
    }
}
