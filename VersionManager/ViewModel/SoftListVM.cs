using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using CentralizeModel;
using DBAccess;
using System.Windows.Input;
using System.ComponentModel;
using VersionManager.BO;
using Telerik.Windows.Data;
using Kernel;
using System.Transactions;
using ViewModelBasic;

namespace VersionManager.ViewModel
{
    internal class SoftListVM : SynchronousViewModel<SoftToUpdateBO>, ICUDOper<SoftToUpdateBO>
    {
        private LinqOPEncap _linqOP = VMGlobal.PlatformCentralizeQuery.LinqOP;

        public SoftListVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<SoftToUpdateBO> SearchData()
        {
            var softs = _linqOP.Search<SoftToUpdate>().ToList();
            var bos = softs.Select(o => (SoftToUpdateBO)o).ToList();
            return bos;
        }

        public OPResult AddOrUpdate(SoftToUpdateBO soft)
        {
            var id = soft.ID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    if (id == default(int))
                    {
                        soft.IdentificationKey = Guid.NewGuid().ToString();
                        soft.ID = _linqOP.Add<SoftToUpdate, int>(soft, o => o.ID);
                    }
                    else
                    {
                        _linqOP.Update<SoftToUpdate>(soft);
                        _linqOP.Delete<SoftCustomerMapping>(o => o.SoftID == soft.ID);
                    }
                    var mapping = soft.Customers.Select(o => new SoftCustomerMapping { SoftID = soft.ID, CustomerID = o.ID });
                    _linqOP.Add(mapping);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    soft.ID = id;
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        public OPResult Delete(SoftToUpdateBO soft)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    _linqOP.Delete<SoftCustomerMapping>(o => o.SoftID == soft.ID);
                    var versions = soft.VersionTracks.Select(v => v.ID);
                    _linqOP.Delete<SoftVersionCustomerMapping>(o => versions.Contains(o.SoftVersionID));
                    _linqOP.Delete<SoftVersionTrack>(soft.VersionTracks.Select(o => (SoftVersionTrack)o));
                    _linqOP.Delete<SoftToUpdate>(soft);
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
                }
            }
            return new OPResult { IsSucceed = true, Message = "删除成功." };
        }

        //internal的话前台竟然无法绑定
        //public ICommand SaveCommand
        //{
        //    get
        //    {
        //        return new DelegateCommand(o =>
        //        {
        //            System.Windows.MessageBox.Show("test");
        //        }, o =>
        //        {
        //            SoftToUpdate soft = o as SoftToUpdate;
        //            if (soft != null)
        //            {
        //                bool flag = !string.IsNullOrWhiteSpace(soft.SoftName) && !string.IsNullOrWhiteSpace(soft.VersionFilesUrl);
        //                return flag;
        //            }
        //            return true;
        //        });
        //    }
        //}
    }
}
