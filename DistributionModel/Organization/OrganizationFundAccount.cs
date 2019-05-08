using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace DistributionModel
{
    /// <summary>
    /// 下级机构往来帐明细
    /// </summary>
    public class OrganizationFundAccount : CreatedData
    {
        public int OrganizationID { get; set; }
        public int BrandID { get; set; }

        /// <summary>
        /// 应收帐
        /// </summary>
        public decimal NeedIn { get; set; }

        /// <summary>
        /// 已收帐
        /// </summary>
        public decimal AlreadyIn { get; set; }

        public string Remark { get; set; }

        /// <summary>
        /// 产生该条往来帐记录的单据类型
        /// </summary>
        public int BillKind { get; set; }

        /// <summary>
        /// 产生该条往来帐记录的相关单据号
        /// </summary>
        public string RefrenceBillCode { get; set; }
    }
}
