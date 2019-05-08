using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    //VipInformation
    public class VIPCard : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        /// <summary>
        /// VIP卡号
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 发卡机构
        /// </summary>
        public int OrganizationID { get; set; }
        public string CustomerName { get; set; }
        public bool Sex { get; set; }
        private DateTime _birthday = DateTime.Now.Date;
        public DateTime Birthday { get { return _birthday; } set { _birthday = value; } }
        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }
        public string Address { get; set; }
        /// <summary>
        /// 身份证号
        /// </summary>
        public string IDCard { get; set; }

        /// <summary>
        /// 预存付款密码
        /// </summary>
        public string PrestorePassword { get; set; }
    }
}

