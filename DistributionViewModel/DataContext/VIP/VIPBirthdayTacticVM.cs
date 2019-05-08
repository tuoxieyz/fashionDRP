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
    public class VIPBirthdayTacticVM : EditSynchronousVM<VIPBirthdayTactic>
    {
        public VIPBirthdayTacticVM()
            : base(VMGlobal.DistributionQuery.LinqOP)
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<VIPBirthdayTactic> SearchData()
        {
            var ds = VMGlobal.DistributionQuery.DB.ExecuteDataSet("GetOrganizationVIPBirthdayTacticHierarchy", VMGlobal.CurrentUser.OrganizationID);
            var table = ds.Tables[0];
            if (table.Rows.Count > 0)
                return table.ToList<VIPBirthdayTactic>();
            else
                return new List<VIPBirthdayTactic>();
        }

        public override OPResult AddOrUpdate(VIPBirthdayTactic entity)
        {
            if (entity.ID == default(int))
            {
                if (LinqOP.Any<VIPBirthdayTactic>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID))
                {
                    return new OPResult { IsSucceed = false, Message = "一个机构职能设置一个VIP生日消费策略." };
                }
                entity.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            }
            return base.AddOrUpdate(entity);
        }

        public override OPResult Delete(VIPBirthdayTactic entity)
        {
            if (entity.OrganizationID != VMGlobal.CurrentUser.OrganizationID)
            {
                return new OPResult { IsSucceed = false, Message = "只能删除本机构创建的VIP生日消费策略." };
            }
            return base.Delete(entity);
        }

    }
}
