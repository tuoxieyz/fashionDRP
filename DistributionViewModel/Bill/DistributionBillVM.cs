using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using DistributionViewModel;
using System.Data;
using System.Transactions;
using DomainLogicEncap;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls;
using Kernel;
using Model.Extension;
using ERPViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class DistributionBillVM<T, TDetail, TForBill> : BillVMBase<T, TDetail, TForBill>
        where T : BillBase, new()
        where TDetail : BillDetailBase
        where TForBill : DistributionProductShow, new()
    {
        #region 属性

        private FloatPriceHelper _fpHelper = null;
        protected FloatPriceHelper FPHelper
        {
            get
            {
                if (_fpHelper == null)
                    _fpHelper = new FloatPriceHelper();
                return _fpHelper;
            }
        }

        #endregion

        public DistributionBillVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        { }

        protected override List<TForBill> GetProductForShow(string code)
        {
            var ps = base.GetProductForShow(code);
            if (ps != null)
            {
                ps.ForEach(o => o.Price = GetFloatPrice(o));
            }
            return ps;
        }

        protected virtual decimal GetFloatPrice(TForBill product)
        {
            return FPHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, product.BYQID, product.Price);
        }

        /// <summary>
        /// 生成单据号
        /// </summary>
        protected override string GenerateBillCode()
        {
            return BillHelper.GenerateBillCode<T>(Master);
        }
    }

    public class DistributionCommonBillVM<T, TDetail> : DistributionBillVM<T, TDetail, DistributionProductShow>
        where T : BillBase, new()
        where TDetail : BillDetailBase
    {
    }
}
