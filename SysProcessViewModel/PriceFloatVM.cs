using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using DBAccess;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DomainLogicEncap;
using Telerik.Windows.Controls.Data.DataFilter;
using Kernel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class PriceFloatVM : PagedEditSynchronousVM<OrganizationPriceFloat>
    {
        #region 属性

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "分支机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public override CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public PriceFloatVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = new List<OrganizationPriceFloatBO>();
        }

        protected override IEnumerable<OrganizationPriceFloat> SearchData()
        {
            var oids = VMGlobal.ChildOrganizations.Select(o => o.ID);
            var opfs = LinqOP.Search<OrganizationPriceFloat>(o => oids.Contains(o.OrganizationID));
            var byqs = LinqOP.GetDataContext<ProBYQ>();
            var result = from opf in opfs
                         from byq in byqs
                         where opf.BYQID == byq.ID
                         select new OrganizationPriceFloatBO
                         {
                             BrandID = byq.BrandID,
                             Year = byq.Year,
                             Quarter = byq.Quarter,
                             BYQID = byq.ID,
                             ID = opf.ID,
                             CreateTime = opf.CreateTime,
                             CreatorID = opf.CreatorID,
                             FloatRate = opf.FloatRate,
                             LastNumber = opf.LastNumber,
                             OrganizationID = opf.OrganizationID
                         };
            var filteredData = (IQueryable<OrganizationPriceFloatBO>)result.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            return filteredData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }

        public override OPResult AddOrUpdate(OrganizationPriceFloat entity)
        {
            OrganizationPriceFloatBO pricefloat = (OrganizationPriceFloatBO)entity;
            var byq = ProductLogic.GetBYQ(pricefloat.BrandID, pricefloat.Year, pricefloat.Quarter);
            if (byq == null)
            {
                return new OPResult { IsSucceed = false, Message = "未找到相应的品牌年份季度信息." };
            }
            pricefloat.BYQID = byq.ID;
            if (pricefloat.ID == default(int))
            {
                if (IsSetted(pricefloat.OrganizationID, byq.ID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应款式的价格上浮策略." };
                }
            }
            else
            {
                if (LinqOP.Any<OrganizationPriceFloat>(o => o.OrganizationID == pricefloat.OrganizationID && o.ID != pricefloat.ID && o.BYQID == byq.ID))
                {
                    return new OPResult { IsSucceed = false, Message = "已为该机构指定了对应款式的价格上浮策略." };
                }
            }
            return base.AddOrUpdate(entity);
        }

        /// <summary>
        /// 判断是否已经设置过相应的价格上浮策略
        /// </summary>
        private bool IsSetted(int organizationID, int byqID)
        {
            return LinqOP.Any<OrganizationPriceFloat>(o => o.OrganizationID == organizationID && o.BYQID == byqID);
        }
    }
}
