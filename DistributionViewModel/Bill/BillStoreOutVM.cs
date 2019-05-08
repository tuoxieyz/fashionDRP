using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DistributionModel;
using System.Collections;
using DistributionViewModel;
using DomainLogicEncap;
using SysProcessModel;
using SysProcessViewModel;
using System.Data;
using ERPViewModelBasic;

namespace DistributionViewModel
{
    public class StoreOutSearchEntity : BillStoreOut
    {
        public string BrandName { get; set; }
        public string CreatorName { get; set; }
        public string StorageName { get; set; }
        public int Quantity { get; set; }
        public string StoreOutType { get; set; }
    }

    public class BillStoreOutVM : DistributionCommonBillVM<BillStoreOut, BillStoreOutDetails>
    {
        public override void SaveWithNoTran()
        {
            //减库存
            Details.ForEach(d => BillLogic.AddStock(Master.StorageID, d.ProductID, d.Quantity * (-1)));
            base.SaveWithNoTran();
        }
    }
}
