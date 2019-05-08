using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VersionManager.BO;
using DBAccess;
using CentralizeModel;
using Kernel;
using System.Transactions;
using ViewModelBasic;

namespace VersionManager.ViewModel
{
    internal class CustomerListVM : SynchronousViewModel<CustomerBO>, ICUDOper<CustomerBO>,IRefresh
    {
        private LinqOPEncap _linqOP = VMGlobal.PlatformCentralizeQuery.LinqOP;

        public CustomerListVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<CustomerBO> SearchData()
        {
            var softs = _linqOP.Search<Customer>().ToList();
            var bos = softs.Select(o => (CustomerBO)o).ToList();
            return bos;
        }

        public OPResult AddOrUpdate(CustomerBO customer)
        {
            try
            {
                if (customer.ID == default(int))
                {
                    customer.IdentificationKey = Guid.NewGuid().ToString();
                    customer.ID = _linqOP.Add<Customer, int>(customer, o => o.ID);
                }
                else
                    _linqOP.Update<Customer>(customer);
                return new OPResult { IsSucceed = true, Message = "保存成功." };
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
            }
        }

        public OPResult Delete(CustomerBO customer)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    _linqOP.Delete<SoftCustomerMapping>(o => o.CustomerID == customer.ID);
                    _linqOP.Delete<SoftVersionCustomerMapping>(o => o.CustomerID == customer.ID);
                    _linqOP.Delete<Customer>(customer);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "删除成功." };
        }

        public void Refresh()
        {
            Entities = this.SearchData();
        }
    }
}
