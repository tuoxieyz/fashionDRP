using ERPModelBO;
using IWCFService;
using Model.Extension;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace DistributionViewModel
{
    internal static class BillHelper
    {
        /// <summary>
        /// 生成单据号
        /// </summary>
        public static string GenerateBillCode<T>(T bill) where T : BillBase, new()
        {
            DateTime time = DateTime.Now;
            using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
            {
                IBillService service = channelFactory.CreateChannel();
                time = service.GetDateTimeOfServer();
            }
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var maxCode = lp.Search<T>(o => o.OrganizationID == bill.OrganizationID).Where(t => t.CreateTime >= time.Date && t.CreateTime <= time.AddDays(1).Date).Max(t => t.Code);
            if (string.IsNullOrEmpty(maxCode))
            {
                int tag = (int)Enum.Parse(typeof(BillTypeEnum), typeof(T).Name);
                string prefixion = Enum.GetName(typeof(BillCodePrefixion), tag);
                var ocode = lp.Search<ViewOrganization>(b => b.ID == bill.OrganizationID).Select(o => o.Code).First();
                maxCode = prefixion + ocode + "-" + time.ToString("yyyyMMdd") + "000";
            }
            int preLength = maxCode.Length - 3;
            return maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
        }
    }
}
