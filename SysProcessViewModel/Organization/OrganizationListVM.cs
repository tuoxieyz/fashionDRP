using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessModel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using DomainLogicEncap;

using Kernel;
using System.Transactions;

using ViewModelBasic;
using System.Data;

namespace SysProcessViewModel
{
    public class OrganizationListVM : PagedEditSynchronousVM<SysOrganization>
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
                        new ItemPropertyDefinition { DisplayName = "编号", PropertyName = "Code", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "名称", PropertyName = "Name", PropertyType = typeof(string)},
                        new ItemPropertyDefinition { DisplayName = "类型", PropertyName = "TypeId", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "状态", PropertyName = "Flag", PropertyType = typeof(Boolean)},
                        new ItemPropertyDefinition { DisplayName = "创建时间", PropertyName = "CreateTime", PropertyType = typeof(DateTime)}
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
                        new FilterDescriptor("Code", FilterOperator.IsEqualTo,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("Name", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false),
                        new FilterDescriptor("Flag", FilterOperator.IsEqualTo, true)
                    };
                }
                return _filterDescriptors;
            }
        }

        private static SysOrganizationBO _currentOrganization;
        public static SysOrganizationBO CurrentOrganization
        {
            get
            {
                if (_currentOrganization == null)
                    _currentOrganization = new SysOrganizationBO(VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(VMGlobal.CurrentUser.OrganizationID));
                return _currentOrganization;
            }
            internal set { _currentOrganization = value; }
        }

        #endregion

        public OrganizationListVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = new List<SysOrganizationBO>();
        }

        private static List<SysOrganizationBO> _currentAndChildrenOrganizations;
        public static List<SysOrganizationBO> CurrentAndChildrenOrganizations
        {
            get
            {
                if (_currentAndChildrenOrganizations == null)
                {
                    _currentAndChildrenOrganizations = CurrentOrganization.ChildrenOrganizations.ToList();//copy,将开辟新的内存区域,若缓存起来将造成与原区域数据不同步
                    _currentAndChildrenOrganizations.Insert(0, CurrentOrganization);
                }
                return _currentAndChildrenOrganizations;
            }
            internal set { _currentAndChildrenOrganizations = value; }
        }

        protected override IEnumerable<SysOrganization> SearchData()
        {
            var currentOID = VMGlobal.CurrentUser.OrganizationID;
            var orgs = LinqOP.Search<SysOrganization>(o => o.ParentID == currentOID || o.ID == currentOID);
            var filteredOrgs = (IQueryable<SysOrganization>)orgs.Where(FilterDescriptors);
            TotalCount = filteredOrgs.Count();
            return filteredOrgs.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).Select(o => new SysOrganizationBO(o)).ToList();
        }

        private OPResult Add(SysOrganizationBO organization)
        {
            organization.CreatorID = VMGlobal.CurrentUser.ID;
            organization.ParentID = VMGlobal.CurrentUser.OrganizationID;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    int id = LinqOP.Add<SysOrganization, int>(organization, o => o.ID);
                    organization.ID = id;//将ID赋值，表明该对象不再是新增对象(新增对象ID都为0)

                    List<OrganizationBrand> obs = new List<OrganizationBrand>();
                    foreach (var b in organization.Brands)
                    {
                        OrganizationBrand ob = new OrganizationBrand
                        {
                            OrganizationID = id,
                            BrandID = b.ID,
                            CreatorID = organization.CreatorID,
                            CreateTime = DateTime.Now
                        };
                        obs.Add(ob);
                    }
                    VMGlobal.SysProcessQuery.LinqOP.Add<OrganizationBrand>(obs);
                    //同时建立仓库,由于存在跨数据库操作，需要在数据库服务器上启动DTC服务
                    //后注：虽然在服务器上启动了DTC并按网上所述全部配置完毕，但却抛出“MSDTC被禁用”的异常。
                    //OrganizationLogic.AddDefaultStorage(organization);

                    scope.Complete();
                }
                catch (Exception e)
                {
                    organization.ID = default(int);
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
            if (!VMGlobal.ChildOrganizations.Any(o => o.ID == organization.ID))
                VMGlobal.ChildOrganizations.Add(organization);
            _currentAndChildrenOrganizations = null;
            //OrganizationLogic.AddDefaultStorage(organization);这里去除新建默认仓库的逻辑
            return new OPResult { IsSucceed = true, Message = "保存成功!" };
        }

        private OPResult Update(SysOrganizationBO organization)
        {
            var brandIDs = CurrentOrganization.Brands.Select(o => o.ID);
            List<OrganizationBrand> obs = new List<OrganizationBrand>();
            foreach (var b in organization.Brands)
            {
                OrganizationBrand ob = new OrganizationBrand
                {
                    OrganizationID = organization.ID,
                    BrandID = b.ID,
                    CreatorID = VMGlobal.CurrentUser.ID,
                    CreateTime = DateTime.Now
                };
                obs.Add(ob);
            }
            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Update<SysOrganization>(organization);
                    VMGlobal.SysProcessQuery.LinqOP.Delete<OrganizationBrand>(ob => brandIDs.Contains(ob.BrandID) && ob.OrganizationID == organization.ID);
                    VMGlobal.SysProcessQuery.LinqOP.Add<OrganizationBrand>(obs);
                    scope.Complete();
                    result = new OPResult { IsSucceed = true, Message = "更新成功!" };
                }
                catch (Exception e)
                {
                    result = new OPResult { IsSucceed = false, Message = "更新失败,失败原因:\n" + e.Message };
                }
            }
            if (result.IsSucceed)
            {
                int index = VMGlobal.ChildOrganizations.FindIndex(o => o.ID == organization.ID);
                if (organization.Flag)
                {
                    if (index == -1)
                        VMGlobal.ChildOrganizations.Add(organization);
                    else
                        VMGlobal.ChildOrganizations[index] = organization;
                }
                else if (index != -1)
                {
                    VMGlobal.ChildOrganizations.RemoveAt(index);
                }
                _currentAndChildrenOrganizations = null;
            }
            return result;
        }

        public override OPResult AddOrUpdate(SysOrganization entity)
        {
            SysOrganizationBO bo = (SysOrganizationBO)entity;
            return bo.ID == default(int) ? this.Add(bo) : this.Update(bo);
        }

        /// <summary>
        /// 是否自营店
        /// </summary>
        public static bool IsSelfRunShop(int oid)
        {
            var type = VMGlobal.OrganizationTypes.Find(o => o.Name == "自营店");
            if (type != null && type.ID == VMGlobal.SysProcessQuery.LinqOP.GetById<SysOrganization>(oid).TypeId)
            {
                return true;
            }
            return false;
        }

        public static IEnumerable<int> GetOrganizationDownHierarchy(int organizationID)
        {
            var ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetOrganizationDownHierarchy", organizationID);
            var table = ds.Tables[0];
            List<int> oids = new List<int>();
            foreach (DataRow row in table.Rows)
            {
                oids.Add((int)row["OrganizationID"]);
            }
            return oids;
        }
    }
}
