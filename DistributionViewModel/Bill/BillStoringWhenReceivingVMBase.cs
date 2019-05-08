using DistributionModel;
using Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public abstract class BillStoringWhenReceivingVMBase : DistributionBillVM<BillStoring, BillStoringDetails, ProductForStoringWhenReceiving>
    {
        public bool IsChecked { get; set; }

        public int StorageID { get; set; }

        public BillStoringWhenReceivingVMBase(BillWithBrand bill)
            : base()
        {
            GridDataItems.Clear();
#if UniqueCode
            var ssdetails = BillStoringCannibalizeVM.GetSnapshotDetails(bill.Code);
#endif
            var data = this.GetBillReceiveDetails(bill.ID);
            foreach (var o in data)
            {
#if UniqueCode
                var temp = ssdetails.Where(u => u.ProductID == o.ProductID);
                foreach (var t in temp)
                {
                    o.UniqueCodes.Add(t.UniqueCode);
                }
#endif
                GridDataItems.Add(o);
            }
        }

        /// <summary>
        /// 收货单明细
        /// </summary>
        protected abstract IEnumerable<ProductForStoringWhenReceiving> GetBillReceiveDetails(int billID);

        public OPResult CheckWhenSave()
        {
            if (StorageID == default(int))
                return new OPResult { IsSucceed = false, Message = "请选择入库仓库" };
            Details = new List<BillStoringDetails>();
            if (IsChecked)
            {
                foreach (var product in GridDataItems)
                {
                    if (product.ReceiveQuantity != 0)
                    {
                        Details.Add(new BillStoringDetails { ProductID = product.ProductID, Quantity = product.ReceiveQuantity });
                    }
                }
            }
            else
            {
                foreach (var product in GridDataItems)
                {
                    if (product.Quantity != 0)
                    {
                        Details.Add(new BillStoringDetails { ProductID = product.ProductID, Quantity = product.Quantity });
                    }
                }
            }
            if (Details.Count == 0)
            {
                return new OPResult { IsSucceed = false, Message = "没有需要保存的数据" };
            }
            return new OPResult { IsSucceed = true };
        }

#if UniqueCode
        public override void UniqueCodeInput(string uniqueCode)
        {
            foreach (ProductForStoringWhenReceiving item in GridDataItems)
            {
                if (item.UniqueCodes.Contains(uniqueCode))
                {
                    if (!item.ReceivedUniqueCodes.Contains(uniqueCode))
                    {
                        item.ReceivedUniqueCodes.Add(uniqueCode);
                        item.ReceiveQuantity += 1;
                    }
                    return;
                }
            }
            throw new Exception("没有匹配的唯一码,请检查.");
        }

        protected override void SaveUniqueCodes()
        {
            if (IsChecked)
            {
                if (this.GridDataItems.Count > 0)
                {
                    List<BillSnapshotDetailsWithUniqueCode> snapshotDetails = new List<BillSnapshotDetailsWithUniqueCode>();
                    TraverseGridDataItems(
                        item =>
                        {
                            if (item.ReceivedUniqueCodes.Count > 0)
                            {
                                snapshotDetails.AddRange(item.ReceivedUniqueCodes.Select(
                                    o => new BillSnapshotDetailsWithUniqueCode
                                    {
                                        UniqueCode = o,
                                        ProductID = item.ProductID
                                    }
                                    ));
                            }
                        }
                    );
                    SaveUniqueCodes(snapshotDetails);
                }
            }
            else
            {
                base.SaveUniqueCodes();
            }
        }
#else
        public override void ProductCodeInput(string pcode, Action<IEnumerable<ProductForStoringWhenReceiving>> actionWhenMore)
        {
            if (GridDataItems.Count > 0)
            {
                if (TryReceiveProductOneByOne(pcode)) return;                
            }
            var datas = this.GetProductForShow(pcode);
            if (datas != null && datas.Count > 0)
            {
                if (datas.Count == 1)
                {
                    var item = GridDataItems.FirstOrDefault(o => o.ProductID == datas[0].ProductID);
                    if (item != null)
                        item.ReceiveQuantity += 1;
                    else
                    {
                        datas[0].ReceiveQuantity = 1;
                        GridDataItems.Add(datas[0]);
                    }
                }
                else
                {
                    if (actionWhenMore != null)
                        actionWhenMore(datas);
                }
            }
            else
                throw new Exception("没有相关成品信息.");
        }

        private bool TryReceiveProductOneByOne(string pcode)
        {
            foreach (var product in GridDataItems)
            {
                if (product.ProductCode == pcode)
                {
                    product.ReceiveQuantity += 1;
                    return true;
                }
            }
            return false;
        }

        public override void AddRangeToItems(IEnumerable<ProductForStoringWhenReceiving> datas)
        {
            foreach (var data in datas)
            {
                if (data.Quantity != 0)
                {
                    var item = GridDataItems.FirstOrDefault(o => o.ProductID == data.ProductID);
                    if (item != null)
                        item.ReceiveQuantity += data.Quantity;
                    else
                    {
                        data.ReceiveQuantity = data.Quantity;
                        data.Quantity = 0;
                        GridDataItems.Add(data);
                    }
                }
            }
        }
#endif
    }
}
