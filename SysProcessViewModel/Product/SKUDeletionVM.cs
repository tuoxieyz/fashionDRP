using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using ViewModelBasic;
using Kernel;
using System.Collections.ObjectModel;
using DistributionModel;
using ManufacturingModel;
using System.Transactions;
using IWCFServiceForIM;
using System.ServiceModel;
using IWCFService;

namespace SysProcessViewModel
{
    public class SKUDeletionVM : PagedReportVM<ViewProduct>
    {
        public string SKUCode { get; set; }

        protected override IEnumerable<ViewProduct> SearchData()
        {
            SKUCode = SKUCode ?? "";
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            IEnumerable<int> bids = VMGlobal.PoweredBrands.Select(o => o.ID);
            var data = lp.Search<ViewProduct>(o => o.ProductCode.Contains(SKUCode) && bids.Contains(o.BrandID));
            TotalCount = data.Count();
            var result = data.OrderBy(o => o.StyleID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            return new ObservableCollection<ViewProduct>(result);
        }

        public OPResult Delete(ViewProduct product)
        {
            var distlp = VMGlobal.DistributionQuery.LinqOP;
            var manulp = VMGlobal.ManufacturingQuery.LinqOP;
            var syslp = VMGlobal.SysProcessQuery.LinqOP;
            var pid = product.ProductID;
            if (distlp.Any<BillStoringDetails>(o => o.ProductID == pid))
                return new OPResult { IsSucceed = false, Message = "删除失败,该SKU已有入库记录." };
            if (distlp.Any<BillStocktakeDetails>(o => o.ProductID == pid))
                return new OPResult { IsSucceed = false, Message = "删除失败,该SKU已有盘点记录." };

            string changeMsg = string.Format("SKU码{0}被删除.", product.ProductCode);
            ProStyleChange change = new ProStyleChange
            {
                CreateTime = DateTime.Now,
                CreatorID = VMGlobal.CurrentUser.ID,
                Description = changeMsg,
                StyleID = product.StyleID
            };
            OPResult result = new OPResult { IsSucceed = false, Message = "删除失败!" };
            using (ChannelFactory<IBillService> channelFactory = new ChannelFactory<IBillService>("BillSVC"))
            {
                IBillService service = channelFactory.CreateChannel();
                result = service.DeleteSKU(product.ProductID, change);
            }
            if (result.IsSucceed)
            {
                ObservableCollection<ViewProduct> products = (ObservableCollection<ViewProduct>)Entities;
                products.Remove(product);
                IMHelper.AsyncSendMessageTo(IMHelper.OnlineUsers, new IMessage
                {
                    Message = changeMsg
                }, IMReceiveAccessEnum.成品资料变动);
            }
            return result;
        }
    }
}
