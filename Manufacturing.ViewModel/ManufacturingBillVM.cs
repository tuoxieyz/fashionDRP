using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERPViewModelBasic;
using Model.Extension;
using SysProcessViewModel;
using ERPModelBO;

namespace Manufacturing.ViewModel
{
    public class ManufacturingBillVM<T, TDetail, TForBill> : BillVMBase<T, TDetail, TForBill>
        where T : BillBase, new()
        where TDetail : BillDetailBase
        where TForBill : ProductShow, new()
    {
        public ManufacturingBillVM(): base(VMGlobal.ManufacturingQuery.LinqOP)
        { }

        protected override string GenerateBillCode()
        {
            DateTime time = DateTime.Now;
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var maxCode = lp.Search<T>(t => t.CreateTime >= time.Date && t.CreateTime <= time.AddDays(1).Date).Max(t => t.Code);
            if (string.IsNullOrEmpty(maxCode))
            {
                int tag = (int)Enum.Parse(typeof(BillTypeEnum), typeof(T).Name);
                string prefixion = Enum.GetName(typeof(BillCodePrefixion), tag);
                maxCode = prefixion + "-" + time.ToString("yyyyMMdd") + "000";
            }
            int preLength = maxCode.Length - 3;
            return maxCode.Substring(0, preLength) + (Convert.ToInt32(maxCode.Substring(preLength)) + 1).ToString("000");
        }
    }
}
