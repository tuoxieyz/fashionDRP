using DistributionModel;
using Kernel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Telerik.Windows.Controls;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class ShopExpensesSetVM : PagedReportVM<ShopExpensesBO>
    {
        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public DateTime BeginMonth { get; set; }

        public DateTime EndMonth { get; set; }

        public virtual ICommand NewCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    var list = Entities as ObservableCollection<ShopExpensesBO>;
                    list.Insert(0, new ShopExpensesBO());
                });
            }
        }

        public ShopExpensesSetVM()
        {
            BeginMonth = EndMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            Entities = new ObservableCollection<ShopExpensesBO>();
        }

        protected override IEnumerable<ShopExpensesBO> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            int beginYear = BeginMonth.Year, endYear = EndMonth.Year, beginMonth = BeginMonth.Month, endMonth = EndMonth.Month;
            var data = lp.Search<ShopExpenses>(o => o.Month >= beginMonth && o.Month <= endMonth && o.Year >= beginYear && o.Year <= endYear && oids.Contains(o.OrganizationID));
            TotalCount = data.Count();
            var pagedData = data.OrderByDescending(o => o.Year).ThenByDescending(o => o.Month).ThenByDescending(o => o.OrganizationID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            return new ObservableCollection<ShopExpensesBO>(pagedData.Select(o => new ShopExpensesBO(o)));
        }

        public OPResult Save(ShopExpenses expense)
        {
            if (expense.OrganizationID == default(int) || expense.Year == default(int) || expense.Month == default(int))
            {
                return new OPResult { IsSucceed = false, Message = "店铺、年月必填." };
            }

            var lp = VMGlobal.DistributionQuery.LinqOP;            
            try
            {
                if (expense.ID == default(int))
                {
                    if (lp.Any<ShopExpenses>(o => o.OrganizationID == expense.OrganizationID && o.Year == expense.Year && o.Month == expense.Month))
                    {
                        return new OPResult { IsSucceed = false, Message = "该店铺该年月已有记录, 请勿重复添加." };
                    }
                    expense.ID = lp.Add<ShopExpenses>(expense);
                }
                else
                {
                    lp.Update<ShopExpenses>(expense);
                }
            }
            catch(Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "保存失败：" + ex.Message };
            }
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }
    }
}
