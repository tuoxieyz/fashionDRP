using DistributionModel;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;

namespace DistributionViewModel
{
    public class BillAllocateSearchVM : BillPagedReportVM<AllocateSearchEntity>
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
                        new ItemPropertyDefinition { DisplayName = "开单日期", PropertyName = "CreateDate", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "单据编号", PropertyName = "Code", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "配货仓库", PropertyName = "StorageID", PropertyType = typeof(int) },
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Status", PropertyType = typeof(bool) }
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
                        new FilterDescriptor("CreateDate", FilterOperator.IsEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("Status", FilterOperator.IsEqualTo, false),
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue)
                    };
                }
                return _filterDescriptors;
            }
        }

        protected override IEnumerable<AllocateSearchEntity> SearchData()
        {
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var users = VMGlobal.DistributionQuery.LinqOP.Search<ViewUser>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            var filtedData = this.SearchOrignData();
            TotalCount = filtedData.Count();
            var allocates = filtedData.OrderByDescending(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            allocates.ForEach(
                o =>
                {
                    o.OrganizationName = childOrganizations.Find(c => c.ID == o.OrganizationID).Name;
                    var user = users.Find(u => u.ID == o.CreatorID);
                    o.CreatorName = user == null ? "" : user.Name;
                    if (o.HandlerID != null)
                    {
                        user = users.Find(u => u.ID == o.HandlerID);
                        o.HandlerName = user == null ? "" : user.Name;
                    }
                }
                );
            return allocates;
        }

        protected virtual IQueryable<AllocateSearchEntity> SearchOrignData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var childOrganizations = OrganizationListVM.CurrentOrganization.ChildrenOrganizations;
            var oids = childOrganizations.Select(o => o.ID).ToArray();
            var allocateContext = lp.Search<BillAllocate>(o => oids.Contains(o.OrganizationID));
            var storageContext = lp.GetDataContext<Storage>();

            var billData = from d in allocateContext
                           from storage in storageContext
                           where storage.ID == d.StorageID
                           select new AllocateSearchEntity
                           {
                               OrganizationID = d.OrganizationID,//跟查询条件相关的属性需要显式声明[赋值]，即使父类里已经定义，否则在生成SQL语句的过程中会报错
                               ID = d.ID,
                               Remark = d.Remark,
                               Code = d.Code,
                               CreatorID = d.CreatorID,
                               CreateTime = d.CreateTime,
                               CreateDate = d.CreateTime.Date,
                               Status = d.Status,
                               StorageName = storage.Name,
                               StorageID = d.StorageID,
                               Quantity = d.Quantity,
                               HandlerID = d.HandlerID,
                               HandleTime = d.HandleTime
                           };
            var detailsContext = lp.GetDataContext<BillAllocateDetails>();
            var pIDs = ProductHelper.GetProductIDArrayWithCondition(DetailsDescriptors);
            if (pIDs != null)
            {
                if (pIDs.Count() == 0)
                    return null;
                billData = from d in billData
                           where detailsContext.Any(od => od.BillID == d.ID && pIDs.Contains(od.ProductID))
                           select d;
            }
            var filtedData = (IQueryable<AllocateSearchEntity>)billData.Where(FilterDescriptors);
            return filtedData;
        }
    }

    public class AllocateSearchEntity : BillAllocate
    {
        public string OrganizationName { get; set; }
        public string StorageName { get; set; }
        public string CreatorName { get; set; }
        public DateTime CreateDate { get; set; }
        public string StatusName
        {
            get
            {
                return Status ? "已处理" : "未处理";
            }
        }
        public string HandlerName { get; set; }

        private List<DistributionProductShow> _details;
        public List<DistributionProductShow> Details
        {
            get
            {
                if (_details == null)
                {
                    _details = new List<DistributionProductShow>();
                    var details = VMGlobal.DistributionQuery.LinqOP.Search<BillAllocateDetails>(o => o.BillID == this.ID).ToList();
                    var pids = details.Select(o => o.ProductID).ToArray();
                    var products = VMGlobal.SysProcessQuery.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
                    foreach (var d in details)
                    {
                        var product = products.Find(p => p.ProductID == d.ProductID);
                        if (product != null)
                        {
                            DistributionProductShow item = new DistributionProductShow
                            {
                                ProductCode = product.ProductCode,
                                BYQID = product.BYQID,
                                NameID = product.NameID,
                                ColorID = product.ColorID,
                                SizeID = product.SizeID,
                                Quantity = d.Quantity,                               
                                StyleCode = product.StyleCode
                            };
                            _details.Add(item);
                        }
                    }
                    foreach (var r in _details)
                    {
                        r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                        r.ColorName = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Name;
                        r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                        r.ProductName = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
                        var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                        r.BrandID = byq.BrandID;
                        r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                    }
                }                
                return _details;
            }
        }
    }
}
