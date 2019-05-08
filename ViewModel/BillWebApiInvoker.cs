using ERPModelBO;
using Kernel;
using Model.Extension;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using ViewModelBasic;

namespace ERPViewModelBasic
{
    public class BillWebApiInvoker : WebApiInvoker
    {
        public static BillWebApiInvoker Instance = new BillWebApiInvoker();

        private BillWebApiInvoker()
            : base()
        { }

        public TResult SaveBill<TResult, T, TDetail>(BillBO<T, TDetail> bo, string apiName = null)
            where TResult : OPResult, new()
            where T : BillBase
            where TDetail : BillDetailBase
        {
            bo.Bill.CreatorID = VMGlobal.CurrentUser.ID;
            apiName = apiName ?? typeof(T).Name;

            try
            {
                return this.Invoke<TResult, BillBO<T, TDetail>>(bo, "Bill/Save" + apiName);
            }
            catch (Exception ex)
            {
                return new TResult { IsSucceed = false, Message = ex.Message };
            }
        }

        public OPResult SaveBill<T, TDetail>(BillBO<T, TDetail> bo, string apiName = null)
            where T : BillBase
            where TDetail : BillDetailBase
        {
            return this.SaveBill<OPResult, T, TDetail>(bo, apiName);
        }
    }
}
