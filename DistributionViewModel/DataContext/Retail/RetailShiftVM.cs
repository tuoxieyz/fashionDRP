using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using Kernel;
using Telerik.Windows.Data;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class RetailShiftVM : EditSynchronousVM<RetailShift>
    {
        public RetailShiftVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o=>new RetailShiftBO(o)).ToList();
        }

        public override OPResult Delete(RetailShift shift)
        {
            if (LinqOP.Any<RetailShoppingGuide>(o => o.ShiftID == shift.ID) || LinqOP.Any<BillRetail>(o => o.ShiftID == shift.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该班次信息已使用,不能被删除,\n若以后不使用,请将状态置为禁用." };
            }
            return base.Delete(shift);
        }

        #region 暂无用代码

        //public static List<RetailShift> GetHierarchyRetailShifts()
        //{
        //    var lp = VMGlobal.DistributionQuery.LinqOP;
        //    var shifts = lp.GetDataContext<RetailShift>();
        //    var ds = VMGlobal.DistributionQuery.DB.ExecuteDataSet("GetOrganizationUpHierarchy", VMGlobal.CurrentUser.OrganizationID);
        //    var table = ds.Tables[0];
        //    if (table.Rows.Count > 0)
        //    {
        //        IEnumerable<int> os = table.ToList<OrganizationHierarchy>().Select(o => o.OrganizationID);
        //        shifts = shifts.Where(o => os.Contains(o.OrganizationID));
        //    }
        //    return shifts.OrderByDescending(o => o.OrganizationID).ToList();
        //}

        ///// <summary>
        ///// 按就近原则获取班次信息
        ///// </summary>
        //public static List<RetailShift> GetNearbyRetailShifts(bool containUnabled = false)
        //{
        //    var shifts = RetailShiftVM.GetHierarchyRetailShifts();
        //    shifts = shifts.FindAll(o => o.OrganizationID == shifts.Max(s => s.OrganizationID));
        //    if (!containUnabled)
        //        shifts.RemoveAll(o => !o.IsEnabled);
        //    return shifts;
        //}

        #endregion
    }
}
