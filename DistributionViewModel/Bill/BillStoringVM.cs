using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using DomainLogicEncap;
using System.Transactions;
using DistributionViewModel;
using Kernel;
using SysProcessViewModel;
using ERPViewModelBasic;
using SysProcessModel;
using ViewModelBasic;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillStoringVM : DistributionCommonBillVM<BillStoring, BillStoringDetails>
    {
        public BillStoringVM()
            : base()
        {
            Master = new BillStoringBO();
        }

        public override void AddRangeToItems(IEnumerable<DistributionProductShow> datas)
        {
            foreach (var data in datas)
            {
                if (data.Quantity > 0)
                {
                    var item = GridDataItems.FirstOrDefault(o => o.ProductID == data.ProductID);
                    if (item != null)
                        item.Quantity += data.Quantity;
                    else
                        GridDataItems.Add(data);
                }
            }
        }

        public override OPResult Save()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var bill = Master;
            bill.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
            var details = this.Details = new List<BillStoringDetails>();
            this.TraverseGridDataItems(product =>
            {
                details.Add(new BillStoringDetails { ProductID = product.ProductID, Quantity = product.Quantity });
            });
            if (details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            bill.CreatorID = VMGlobal.CurrentUser.ID;
            bill.Code = this.GenerateBillCode();
            //if (string.IsNullOrEmpty(bill.RefrenceBillCode))
            bill.RefrenceBillCode = bill.Code;//假如没有相关单据号，则相关单据号就是入库单自身单据编号
            bill.BillType = (int)BillTypeEnum.BillStoring;
            if (string.IsNullOrEmpty(bill.Remark))
                bill.Remark = "成品入库";
            //TransactionOptions transactionOption = new TransactionOptions();
            ////设置事务隔离级别
            //transactionOption.IsolationLevel = System.Transactions.IsolationLevel.Snapshot;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    bill.ID = lp.Add<BillStoring, int>(bill, b => b.ID);
                    details.ForEach(d => d.BillID = bill.ID);
                    lp.Add<BillStoringDetails>(details);
                    Details.ForEach(d => BillLogic.AddStock(bill.StorageID, d.ProductID, d.Quantity));
                    scope.Complete();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }

        public override void SaveWithNoTran()
        {
            //加库存
            Details.ForEach(d => BillLogic.AddStock(Master.StorageID, d.ProductID, d.Quantity));
            base.SaveWithNoTran();
        }
    }
}
