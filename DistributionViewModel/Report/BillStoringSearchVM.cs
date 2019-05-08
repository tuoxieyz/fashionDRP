using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using DistributionModel;
using SysProcessModel;
using ERPViewModelBasic;
using SysProcessViewModel;
using ERPModelBO;

namespace DistributionViewModel
{
    public class BillStoringSearchVM : BillPagedReportVM<StoringSearchEntity>
    {
        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateTime", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "入库仓库", PropertyName = "StorageID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "入库类型", PropertyName = "BillType", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "入库品牌", PropertyName = "BrandID", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("CreateTime", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("CreateTime", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("StorageID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        /// <summary>
        /// 入库单查询
        /// </summary>
        protected override IEnumerable<StoringSearchEntity> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var storingContext = lp.GetDataContext<BillStoring>();
            var storageContext = lp.GetDataContext<Storage>();
            var userContext = lp.GetDataContext<ViewUser>();

            var brands = VMGlobal.PoweredBrands;
            var brandIDs = brands.Select(b => b.ID);

            var billData = from storing in storingContext
                           from storage in storageContext
                           where storing.StorageID == storage.ID && storing.OrganizationID == VMGlobal.CurrentUser.OrganizationID && brandIDs.Contains(storing.BrandID)
                           from user in userContext
                           where storing.CreatorID == user.ID
                           select new StoringSearchEntity
                           {
                               StorageID = storing.StorageID,
                               CreateTime = storing.CreateTime.Date,
                               BillType = storing.BillType,
                               BillID = storing.ID,
                               BrandID = storing.BrandID,
                               单据编号 = storing.Code,
                               入库仓库 = storage.Name,
                               入库类型 = Enum.GetName(typeof(BillStoringTypeEnum), storing.BillType),//并不[泛滥地]生成sql，这个非常给力
                               相关单据编号 = storing.RefrenceBillCode,
                               开单时间 = storing.CreateTime,
                               开单人 = user.Name,
                               备注 = storing.Remark
                           };
            var storingDetailsContext = lp.GetDataContext<BillStoringDetails>();
            var pids = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors, brandIDs);
            if (pids != null)
            {
                if (pids.Count() == 0)
                    return null;
                billData = from d in billData
                           where storingDetailsContext.Any(sd => sd.BillID == d.BillID && pids.Contains(sd.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<StoringSearchEntity>)billData.Where(FilterDescriptors);
            TotalCount = filtedData.Count();
            var datas = filtedData.OrderByDescending(o => o.BillID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            var bIDs = datas.Select(o => (int)o.BillID);
            var sum = storingDetailsContext.Where(o => bIDs.Contains(o.BillID)).GroupBy(o => o.BillID).Select(g => new { BillID = g.Key, TotalQuantity = g.Sum(o => o.Quantity) }).ToList();
            datas.ForEach(o =>
            {
                o.入库品牌 = brands.Find(b => b.ID == o.BrandID).Name;
                o.入库数量 = sum.Find(s => s.BillID == o.BillID).TotalQuantity;
            });
            //var result = datas.Select(d => new
            //{
            //    BillID = d.BillID,
            //    单据编号 = d.单据编号,
            //    入库仓库 = d.入库仓库,
            //    入库类型 = d.入库类型,
            //    开单时间 = d.开单时间,
            //    开单人 = d.开单人,
            //    入库数量 = sum.Find(o => o.BillID == d.BillID).TotalQuantity
            //});
            return datas;
        }
    }
}
