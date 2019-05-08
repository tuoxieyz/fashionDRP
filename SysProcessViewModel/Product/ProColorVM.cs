using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProColorVM : EditSynchronousVM<ProColor>
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
                        new ItemPropertyDefinition { DisplayName = "名称", PropertyName = "Name", PropertyType = typeof(string)}
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
                        new FilterDescriptor("Name", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        #endregion

        public ProColorVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            Entities = VMGlobal.Colors.Select(o => new ProColorBO(o)).ToList();
        }

        protected override IEnumerable<ProColor> SearchData()
        {
            //使UI层能使用ProColorBO的校验功能
            return base.SearchData().Select(o => new ProColorBO(o)).ToList();
        }

        public override OPResult Delete(ProColor color)
        {
            if (LinqOP.Any<Product>(p => p.ColorID == color.ID))
            {
                return new OPResult { IsSucceed = false, Message = "该颜色已经被使用，无法删除。" };
            }
            var result = base.Delete(color);
            if (result.IsSucceed)
            {
                VMGlobal.Colors.RemoveAll(o => o.ID == color.ID);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProColor entity)
        {
            var result = base.AddOrUpdate(entity);
            if (result.IsSucceed)
            {
                var color = VMGlobal.Colors.Find(o => o.ID == entity.ID);
                if (color == null)
                    VMGlobal.Colors.Add(entity);
                else
                {
                    int index = VMGlobal.Colors.IndexOf(color);
                    VMGlobal.Colors[index] = entity;
                }
            }
            return result;
        }
    }
}
