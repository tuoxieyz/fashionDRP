using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ERPViewModelBasic;
using DistributionModel;
using DomainLogicEncap;
using System.Collections;
using System.Windows;
using Telerik.Windows.Controls;
using Kernel;
using ViewModelBasic;
using SysProcessViewModel;
using System.Windows.Forms;

namespace DistributionViewModel
{
    public class RevenueActualvsTargetVM : CommonViewModel<RetailMonthTagetBO>
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
                        new ItemPropertyDefinition { DisplayName = "零售月份", PropertyName = "YearMonth", PropertyType = typeof(DateTime)},
                        new ItemPropertyDefinition { DisplayName = "零售机构", PropertyName = "OrganizationID", PropertyType = typeof(int)}
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
                        new FilterDescriptor("YearMonth", FilterOperator.IsGreaterThanOrEqualTo,TransformYearMonth(DateTime.Now.AddMonths(-11))),
                        new FilterDescriptor("YearMonth", FilterOperator.IsLessThanOrEqualTo,TransformYearMonth(DateTime.Now)),
                        new FilterDescriptor("OrganizationID", FilterOperator.IsEqualTo,VMGlobal.CurrentUser.OrganizationID)
                    };
                }
                return _filterDescriptors;
            }
        }

        /// <summary>
        /// 是否只显示有指标的机构月份数据
        /// </summary>
        public bool OnlyShowHasTarget { get; set; }

        public override System.Windows.Input.ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    Entities = this.SearchData();
                    if (Entities == null)
                    {
                        MessageBox.Show("查询结果或许同时涉及多月份多机构,无法正常显示,请修改查询条件.\n注意:该报表只能查看单月份多机构或多月份单机构的指标完成情况.");
                    }
                });
            }
        }

        private bool _showSingleOrganization = false;
        public bool ShowSingleOrganization
        {
            get { return _showSingleOrganization; }
            private set
            {
                if (_showSingleOrganization != value)
                {
                    _showSingleOrganization = value;
                    OnPropertyChanged("ShowSingleOrganization");
                    OnPropertyChanged("ShowSingleMonth");
                }
            }
        }

        public bool ShowSingleMonth
        {
            get { return !_showSingleOrganization; }
        }

        #endregion

        protected override IEnumerable<RetailMonthTagetBO> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationListVM.CurrentAndChildrenOrganizations.Select(o => o.ID);
            //指标
            var targetData = lp.Search<RetailMonthTaget>(o => oids.Contains(o.OrganizationID)).Select(o => new RetailMonthTagetBO(o)
            {
                OrganizationID = o.OrganizationID,
                YearMonth = new DateTime(o.Year, o.Month, 1)
            });
            targetData = (IQueryable<RetailMonthTagetBO>)targetData.Where(FilterDescriptors);
            var targetResult = targetData.ToList();
            if (targetResult.GroupBy(o => o.YearMonth).Count() > 1 && targetResult.GroupBy(o => o.OrganizationID).Count() > 1)
                return null;

            //实际业绩
            var retailResult = this.GetActual(targetResult);
            if (retailResult == null)
                return null;

            List<RetailMonthTagetBO> result = new List<RetailMonthTagetBO>();
            foreach (var target in targetResult)
            {
                var retail = retailResult.Find(o => o.OrganizationID == target.OrganizationID && o.YearMonth == target.YearMonth);
                if (retail != null)
                {
                    target.SaleActual = retail.SaleActual;
                }
                else
                    target.SaleActual = 0;
                result.Add(target);
            }
            if (!OnlyShowHasTarget)
            {
                var exceptRetail = retailResult.Except(targetResult, new RetailMonthTagetBOComparer());
                foreach (var retail in exceptRetail)
                {
                    retail.SaleTaget = 0;
                    result.Add(retail);
                }
            }
            if (result.Select(o => o.OrganizationID).Distinct().Count() == 1)//单机构
            {
                ShowSingleOrganization = true;
                var organization = OrganizationListVM.CurrentAndChildrenOrganizations.Find(o => o.ID == result[0].OrganizationID);
                foreach (var r in result)
                {
                    r.OrganizationName = organization == null ? "" : organization.Name;
                    r.OrganizationCode = organization == null ? "" : organization.Code;
                }
            }
            else
            {
                ShowSingleOrganization = false;
                foreach (var r in result)
                {
                    var organization = OrganizationListVM.CurrentAndChildrenOrganizations.Find(o => o.ID == r.OrganizationID);
                    r.OrganizationName = organization == null ? "" : organization.Name;
                    r.OrganizationCode = organization == null ? "" : organization.Code;
                }
            }
            return result.OrderBy(o => o.YearMonth).ThenBy(o => o.OrganizationName);
        }

        private List<RetailMonthTagetBO> GetActual(List<RetailMonthTagetBO> targetResult)
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var oids = OrganizationListVM.CurrentAndChildrenOrganizations.Select(o => o.ID);
            if (OnlyShowHasTarget)
            {
                oids = targetResult.Select(o => o.OrganizationID).Distinct();
            }
            var retailData = lp.Search<BillRetail>(o => oids.Contains(o.OrganizationID)).Select(o => new RetailMonthTagetBO
                       {
                           OrganizationID = o.OrganizationID,
                           YearMonth = new DateTime(o.CreateTime.Year, o.CreateTime.Month, 1),
                           CreateTime = o.CreateTime,
                           //SaleActual = o.CostMoney
                           ID = o.ID
                       });
            retailData = (IQueryable<RetailMonthTagetBO>)retailData.Where(FilterDescriptors);
            if (OnlyShowHasTarget)
            {
                var months = targetResult.Select(o => o.YearMonth.Value).Distinct();
                retailData = retailData.Where(o => months.Contains(new DateTime(o.CreateTime.Year, o.CreateTime.Month, 1)));
            }
            var ids = retailData.Select(o => o.ID).ToArray();
            if (ids.Count() == 0)
                return new List<RetailMonthTagetBO>();
            string idarray = "";
            foreach (int id in ids)
            {
                idarray += (id + ",");
            }
            var db = VMGlobal.DistributionQuery.DB;
            var ds = db.ExecuteDataSet("GetOrganizationYMRetailAchievement", idarray.TrimEnd(','));
            //依然是GroupBy的问题,IQToolkit还需要自己去完善
            //retailData = retailData.GroupBy(o => new { o.OrganizationID, o.CreateTime.Year, o.CreateTime.Month }).Select(g => new RetailMonthTagetBO
            //retailData = from r in retailData
            //             group r.SaleActual by new { r.OrganizationID, r.YearMonth } into retailGroup
            //             select new RetailMonthTagetBO
            //{
            //    OrganizationID = g.Key.OrganizationID,
            //    YearMonth = new DateTime(g.Key.Year, g.Key.Month, 1),
            //    SaleActual = g.Sum(o => o.SaleActual)
            //});
            var retailResult = ds.Tables[0].ToList<RetailMonthTagetBO>();
            if (retailResult.GroupBy(o => o.YearMonth).Count() > 1 && retailResult.GroupBy(o => o.OrganizationID).Count() > 1)
                return null;
            else
                return retailResult;
        }

        private DateTime TransformYearMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        private class RetailMonthTagetBOComparer : IEqualityComparer<RetailMonthTagetBO>
        {
            public bool Equals(RetailMonthTagetBO x, RetailMonthTagetBO y)
            {
                if (x == null && y == null)
                    return false;
                return x.OrganizationID == y.OrganizationID && x.YearMonth == y.YearMonth;
            }

            public int GetHashCode(RetailMonthTagetBO obj)
            {
                return obj.ToString().GetHashCode();
            }
        }
    }
}
