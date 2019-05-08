using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using DistributionModel;
using DistributionViewModel;
using System.Collections;
using Telerik.Windows.Controls;
using DomainLogicEncap;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;
using System.Data;

namespace DistributionViewModel
{
    #region 实体类

    public class StocktakeSearchEntity : ViewModelBase
    {
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public int BillID { get; set; }
        public string Code { get; set; }
        public int BrandID { get; set; }
        public string BrandName { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 开单人姓名
        /// </summary>
        public string CreatorName { get; set; }
        /// <summary>
        /// 盘点仓库
        /// </summary>
        public string StorageName { get; set; }
        /// <summary>
        /// 盘点数量
        /// </summary>
        public int Quantity { get; set; }
        public string Remark { get; set; }
        public bool Status { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string StatusName { get { return Status ? "已更新库存" : "未更新库存"; } }

        private bool _isDeleted = false;
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                if (_isDeleted != value)
                {
                    _isDeleted = value;
                    OnPropertyChanged("IsDeleted");
                    OnPropertyChanged("IsDeletedName");
                }
            }
        }

        public string IsDeletedName { get { return IsDeleted ? "已作废" : "有效"; } }
    }

    internal class StocktakeEntityForSearch : BillStocktake
    {
        public DateTime CreateDate { get; set; }
    }

    public class StocktakeEntityForAggregation : BillStocktake, IProductForAggregation
    {
        public int ProductID { get; set; }
        public string StyleCode { get; set; }
        public int Quantity { get; set; }
    }

    public class StocktakeAggregationEntityForStockUpdate : DistributionProductShow
    {
        public int StockQuantity { get; set; }
    }

    #endregion

    public static partial class ReportDataContext
    {
        public static IList GetBillStocktakeDetails(int billID)
        {
            var detailsContext = _query.LinqOP.Search<BillStocktakeDetails>(o => o.BillID == billID);
            var productContext = _query.LinqOP.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new DistributionProductShow
                       {
                           BYQID=product.BYQID,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           ProductCode = product.ProductCode,                           
                           StyleCode = product.StyleCode,
                           Price = product.Price,
                           Quantity = details.Quantity
                       };
            var result = data.ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.BrandID = VMGlobal.BYQs.Find(o => o.ID == r.BYQID).BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
            }
            return result;
        }

        private static IQueryable<StocktakeEntityForAggregation> GetStocktakeAggregation(CompositeFilterDescriptorCollection filters)
        {
            var lp = _query.LinqOP;
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var stocktakeContext = lp.Search<BillStocktake>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID && !o.IsDeleted);
            FilterBillWithBrand(stocktakeContext, filters, brandIDs);
            var detailsContext = lp.GetDataContext<BillStocktakeDetails>();
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from st in stocktakeContext
                       from details in detailsContext
                       where st.ID == details.BillID
                       from product in productContext
                       where product.ProductID == details.ProductID //&& brandIDs.Contains(product.BrandID)
                       select new StocktakeEntityForAggregation
                       {
                           ID = st.ID,
                           ProductID = product.ProductID,
                           BrandID = product.BrandID,
                           Code = st.Code,
                           CreateTime = st.CreateTime.Date,
                           Status = st.Status,
                           StorageID = st.StorageID,
                           StyleCode = product.StyleCode,
                           Quantity = details.Quantity
                       };
            return (IQueryable<StocktakeEntityForAggregation>)data.Where(filters);
        }

        public static DataTable AggregateStocktake(CompositeFilterDescriptorCollection filters)
        {
            var filtedData = GetStocktakeAggregation(filters);
            return new BillReportHelper().TransferSizeToHorizontal<DistributionProductShow>(AggregateBill(filtedData));
        }

        public static List<DistributionProductShow> AggregateStocktakeForStockUpdate(CompositeFilterDescriptorCollection filters, out List<int> refrenceStocktakeIDs)
        {
            var filtedData = GetStocktakeAggregation(filters);
            refrenceStocktakeIDs = filtedData.Select(o => o.ID).Distinct().ToList();
            return AggregateBill(filtedData);
        }
    }

    public class BillStocktakeVM : DistributionCommonBillVM<BillStocktake, BillStocktakeDetails>
    {
        public override void TraverseGridDataItems(Action<DistributionProductShow> action)
        {
            foreach (var item in GridDataItems)
            {
                action(item);
            }
        }
    }
}
