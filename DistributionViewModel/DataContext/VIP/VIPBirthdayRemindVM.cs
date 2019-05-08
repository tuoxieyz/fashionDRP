using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DistributionModel;
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VIPBirthdayRemindVM : CommonViewModel<VIPCard>
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
                        new ItemPropertyDefinition { DisplayName = "生日", PropertyName = "BirthdayMD", PropertyType = typeof(DateTime) }
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
                        new FilterDescriptor("BirthdayMD", FilterOperator.IsGreaterThanOrEqualTo, DateTime.Now.Date),
                        new FilterDescriptor("BirthdayMD", FilterOperator.IsLessThanOrEqualTo, DateTime.Now.Date.AddDays(15))
                    };
                }
                return _filterDescriptors;
            }
        }

        public VIPBirthdayRemindVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<VIPCard> SearchData()
        {
            var lp = VMGlobal.DistributionQuery.LinqOP;
            var data = lp.Search<VIPCard>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).Select(o => new VIPCardBO(o)
            {
                BirthdayMD = new DateTime(DateTime.Now.Year, o.Birthday.Month, o.Birthday.Day)//该语法可行，看来条件貌似一定要显式赋值，在属性get块内写这段逻辑就会报错
            });
            var filtedData = (IQueryable<VIPCardBO>)data.Where(FilterDescriptors);
            var result = filtedData.ToList();
            VIPCardVM.ApplyVIPKind(result);
            return result;
        }
    }
}
