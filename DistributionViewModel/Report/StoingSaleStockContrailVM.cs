using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using SysProcessViewModel;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using ERPModelBO;

namespace DistributionViewModel
{
    public class StoingSaleStockContrailVM : CommonViewModel<StoringSaleStockEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public virtual IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public virtual CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {                          
                        new FilterDescriptor("StyleCode", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue),
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        private DateTime _beginDate = DateTime.Now.Date;
        public DateTime BeginDate { get { return _beginDate; } set { _beginDate = value; } }

        private DateTime _endDate = DateTime.Now.Date;
        public DateTime EndDate { get { return _endDate; } set { _endDate = value; } }

        /// <summary>
        /// 获取进销存数据
        /// </summary>
        protected IEnumerable<StoringSaleStockEntity> SearchData(IEnumerable<int> storageIDs)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            //var storageContext = lp.Search<Storage>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID);
            //var storageIDs = StorageInfoVM.Storages.Select(o => o.ID);
            var stockContext = lp.Search<Stock>(o => storageIDs.Contains(o.StorageID));
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var productContext = lp.Search<ViewProduct>(o => brandIDs.Contains(o.BrandID));
            //目前库存量
            var data = from stock in stockContext
                       from product in productContext
                       where stock.ProductID == product.ProductID && stock.Quantity != 0
                       select new BillEntityForAggregation
                       {
                           ProductID = product.ProductID,
                           Quantity = stock.Quantity,
                           StorageID = stock.StorageID,
                           StyleCode = product.StyleCode,
                           BrandID = product.BrandID,
                           NameID = product.NameID,
                           Year = product.Year,
                           Quarter = product.Quarter
                       };
            data = (IQueryable<BillEntityForAggregation>)data.Where(FilterDescriptors);
            var stocks = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //出库记录
            var storeoutContext = lp.Search<BillStoreOut>(o => o.CreateTime >= _beginDate && storageIDs.Contains(o.StorageID) && brandIDs.Contains(o.BrandID));
            var soDetailContext = lp.GetDataContext<BillStoreOutDetails>();
            var soData = from soDetail in soDetailContext
                         from storeout in storeoutContext
                         where soDetail.BillID == storeout.ID
                         from product in productContext
                         where soDetail.ProductID == product.ProductID
                         select new StoringOutEntityForAggregation
                         {
                             ProductID = product.ProductID,
                             Quantity = soDetail.Quantity,
                             StorageID = storeout.StorageID,
                             StyleCode = product.StyleCode,
                             BrandID = product.BrandID,
                             BillType = storeout.BillType,
                             CreateDate = storeout.CreateTime.Date,
                             NameID = product.NameID,
                             Year = product.Year,
                             Quarter = product.Quarter
                         };
            soData = (IQueryable<StoringOutEntityForAggregation>)soData.Where(FilterDescriptors);
            var soDatas = soData.GroupBy(o => new { o.ProductID, o.BillType, o.CreateDate }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //入库记录
            var storingContext = lp.Search<BillStoring>(o => o.CreateTime >= _beginDate && storageIDs.Contains(o.StorageID) && brandIDs.Contains(o.BrandID));
            var siDetailContext = lp.GetDataContext<BillStoringDetails>();
            var siData = from siDetail in siDetailContext
                         from storing in storingContext
                         where siDetail.BillID == storing.ID
                         from product in productContext
                         where siDetail.ProductID == product.ProductID
                         select new StoringOutEntityForAggregation
                         {
                             ProductID = product.ProductID,
                             Quantity = siDetail.Quantity,
                             StorageID = storing.StorageID,
                             StyleCode = product.StyleCode,
                             BrandID = product.BrandID,
                             BillType = storing.BillType,
                             CreateDate = storing.CreateTime.Date,
                             NameID = product.NameID,
                             Year = product.Year,
                             Quarter = product.Quarter
                         };
            siData = (IQueryable<StoringOutEntityForAggregation>)siData.Where(FilterDescriptors);
            var siDatas = siData.GroupBy(o => new { o.ProductID, o.BillType, o.CreateDate }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //盈亏记录
            var contrastContext = lp.Search<BillStocktakeContrast>(o => o.CreateTime >= _beginDate && storageIDs.Contains(o.StorageID) && brandIDs.Contains(o.BrandID));
            var ctDetailContext = lp.GetDataContext<BillStocktakeContrastDetails>();
            var ctData = from ctDetail in ctDetailContext
                         from contrast in contrastContext
                         where ctDetail.BillID == contrast.ID && ctDetail.Quantity != 0
                         from product in productContext
                         where ctDetail.ProductID == product.ProductID
                         select new BillEntityForAggregation
                         {
                             ProductID = product.ProductID,
                             Quantity = ctDetail.Quantity,
                             StorageID = contrast.StorageID,
                             StyleCode = product.StyleCode,
                             BrandID = product.BrandID,
                             CreateTime = contrast.CreateTime.Date,
                             NameID = product.NameID,
                             Year = product.Year,
                             Quarter = product.Quarter
                         };
            ctData = (IQueryable<BillEntityForAggregation>)ctData.Where(FilterDescriptors);
            var ctDatas = ctData.GroupBy(o => new { o.ProductID, o.CreateTime }).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity) }).ToList();
            //汇总
            var pids = stocks.Select(o => o.Key).Concat(soDatas.Select(o => o.Key.ProductID))
                .Concat(siDatas.Select(o => o.Key.ProductID))
                .Concat(ctDatas.Select(o => o.Key.ProductID)).Distinct().ToArray();
            var result = productContext.Where(o => pids.Contains(o.ProductID)).Select(o => new StoringSaleStockEntity
            {
                ProductID = o.ProductID,
                BYQID = o.BYQID,
                ColorID = o.ColorID,
                SizeID = o.SizeID,
                NameID = o.NameID,
                ProductCode = o.ProductCode,
                StyleCode = o.StyleCode
            }).ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.BrandID = VMGlobal.BYQs.Find(o => o.ID == r.BYQID).BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                r.ProductName = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
            }
            foreach (var item in result)
            {
                var stock = stocks.Find(o => o.Key == item.ProductID);
                var nowQuantity = stock == null ? 0 : stock.Quantity;
                var tempso = soDatas.Where(o => o.Key.ProductID == item.ProductID);
                var tempsi = siDatas.Where(o => o.Key.ProductID == item.ProductID);
                var tempct = ctDatas.Where(o => o.Key.ProductID == item.ProductID);
                int soQuantity = tempso.Sum(o => o.Quantity);//出库
                int siQuantity = tempsi.Sum(o => o.Quantity);//入库
                int ctQuantity = tempct.Sum(o => o.Quantity);//盈亏
                item.OrginalQuantity = nowQuantity + soQuantity - siQuantity - ctQuantity;//反推得到期初库存数
                item.盈亏数量 = ctQuantity;

                soQuantity = tempso.Where(o => o.Key.CreateDate > _endDate).Sum(o => o.Quantity);//出库
                siQuantity = tempsi.Where(o => o.Key.CreateDate > _endDate).Sum(o => o.Quantity);//入库
                ctQuantity = tempct.Where(o => o.Key.CreateTime > _endDate).Sum(o => o.Quantity);//盈亏
                item.Quantity = nowQuantity + soQuantity - siQuantity - ctQuantity;//反推得到期末库存数
                item.盈亏数量 -= ctQuantity;

                tempso = tempso.Where(o => o.Key.CreateDate >= _beginDate && o.Key.CreateDate <= _endDate);
                tempsi = tempsi.Where(o => o.Key.CreateDate >= _beginDate && o.Key.CreateDate <= _endDate);
                item.发货出库 = tempso.Where(o => o.Key.BillType == (int)BillTypeEnum.BillDelivery).Sum(o => o.Quantity);
                item.零售出库 = tempso.Where(o => o.Key.BillType == (int)BillTypeEnum.BillRetail).Sum(o => o.Quantity);
                item.调拨出库 = tempso.Where(o => o.Key.BillType == (int)BillTypeEnum.BillCannibalize).Sum(o => o.Quantity);
                item.退货出库 = tempso.Where(o => o.Key.BillType == (int)BillTypeEnum.BillGoodReturn).Sum(o => o.Quantity);
                item.移库出库 = tempso.Where(o => o.Key.BillType == (int)BillTypeEnum.BillStoreMove).Sum(o => o.Quantity);
                item.发货入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillDelivery).Sum(o => o.Quantity);
                item.零售入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillRetail).Sum(o => o.Quantity);
                item.调拨入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillCannibalize).Sum(o => o.Quantity);
                item.退货入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillGoodReturn).Sum(o => o.Quantity);
                item.移库入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillStoreMove).Sum(o => o.Quantity);
                item.成品入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillStoring).Sum(o => o.Quantity);
                item.交接入库 = tempsi.Where(o => o.Key.BillType == (int)BillTypeEnum.BillProductExchange).Sum(o => o.Quantity);
            }
            return result;
        }

        protected override IEnumerable<StoringSaleStockEntity> SearchData()
        {
            return this.SearchData(StorageInfoVM.Storages.Select(o => o.ID));
        }
    }
}
