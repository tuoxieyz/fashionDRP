using DistributionModel;
using Kernel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;

namespace DistributionViewModel
{
    public class BillAllocateManageVM : BillAllocateSearchVM
    {
        public BillAllocateManageVM()
        {
            var ipds = ItemPropertyDefinitions as List<ItemPropertyDefinition>;
            ipds.RemoveAll(o => o.PropertyName == "Status");
            //ipds.RemoveRange(0, 2);
            FilterDescriptors.RemoveAt(1);

            this.Entities = SearchData();
        }

        protected override IEnumerable<AllocateSearchEntity> SearchData()
        {
            return new ObservableCollection<AllocateSearchEntity>(base.SearchData());
        }

        protected override IQueryable<AllocateSearchEntity> SearchOrignData()
        {
            var filtedData = base.SearchOrignData();
            return filtedData.Where(o => !o.Status);
        }

        public OPResult Handle(AllocateSearchEntity entity)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var allocate = lp.GetById<BillAllocate>(entity.ID);            
            if (allocate == null)
                return new OPResult { IsSucceed = false, Message = "未找到相应单据." };
            if (allocate.Status)
                return new OPResult { IsSucceed = false, Message = "配货单已处理." };

            allocate.HandlerID = VMGlobal.CurrentUser.ID;
            allocate.HandleTime = DateTime.Now;
            allocate.Status = true;
            try
            {
                lp.Update<BillAllocate>(allocate);
            }
            catch (Exception ex)
            {
                return new OPResult { IsSucceed = false, Message = "操作失败,失败原因:\n" + ex.Message };
            }
            (this.Entities as ObservableCollection<AllocateSearchEntity>).Remove(entity);
            return new OPResult { IsSucceed = true, Message = "操作成功!" };
        }
    }
}
