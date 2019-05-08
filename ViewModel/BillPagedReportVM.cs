using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ViewModelBasic;

namespace ERPViewModelBasic
{
    public abstract class BillPagedReportVM<TData> : PagedReportVM<TData>
        where TData : class
    {
        IEnumerable<ItemPropertyDefinition> _detailsPropertyDefinitions;
        public virtual IEnumerable<ItemPropertyDefinition> DetailsPropertyDefinitions
        {
            get
            {
                if (_detailsPropertyDefinitions == null)
                {
                    _detailsPropertyDefinitions = new List<ItemPropertyDefinition>() {
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "StyleCode", PropertyType = typeof(string) }
                     };
                }
                return _detailsPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _detailsDescriptors;
        public virtual CompositeFilterDescriptorCollection DetailsDescriptors
        {
            get
            {
                if (_detailsDescriptors == null)
                {
                    _detailsDescriptors = new CompositeFilterDescriptorCollection() 
                    {
                        new FilterDescriptor("StyleCode", FilterOperator.Contains, FilterDescriptor.UnsetValue, false)
                    };
                }
                return _detailsDescriptors;
            }
        }
    }
}
