using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;

namespace ViewModelBasic
{
    public static class FilterConditionHelper
    {
        /// <summary>
        /// 用户是否设置了指定条件
        /// </summary>
        /// <param name="memberName">条件关联属性名称</param>
        private static bool IsConditionSetted(IFilterDescriptor descriptor, string memberName)
        {
            if (descriptor is FilterDescriptor)
            {
                var fd = (FilterDescriptor)descriptor;
                return fd.Member == memberName && fd.Value != FilterDescriptor.UnsetValue;
            }
            if (descriptor is CompositeFilterDescriptor)
            {
                var cfd = (CompositeFilterDescriptor)descriptor;
                return cfd.FilterDescriptors.Any(o => IsConditionSetted(o, memberName));
            }
            if (descriptor is FilterDescriptorCollection)
            {
                var fdc = (FilterDescriptorCollection)descriptor;
                return fdc.Any(o => IsConditionSetted(o, memberName));
            }
            return false;
        }

        /// <summary>
        /// 用户是否设置了指定条件
        /// </summary>
        /// <param name="memberName">条件关联属性名称</param>
        public static bool IsConditionSetted(IEnumerable<IFilterDescriptor> filterDescriptors, string memberName)
        {
            for (int i = 0; i < filterDescriptors.Count(); i++)
            {
                if (IsConditionSetted(filterDescriptors.ElementAt(i), memberName))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 清空指定条件的值
        /// </summary>
        /// <param name="memberName">条件关联属性名称</param>
        public static void ClearCondition(IFilterDescriptor descriptor, string memberName)
        {
            if (descriptor is FilterDescriptor)
            {
                var fd = (FilterDescriptor)descriptor;
                if (fd.Member == memberName)
                    fd.Value = FilterDescriptor.UnsetValue;
            }
            else if (descriptor is CompositeFilterDescriptor)
            {
                var cfd = (CompositeFilterDescriptor)descriptor;
                foreach (var fd in cfd.FilterDescriptors)
                {
                    ClearCondition(fd, memberName);
                }
            }
            else if (descriptor is FilterDescriptorCollection)
            {
                var fdc = (FilterDescriptorCollection)descriptor;
                foreach (var fd in fdc)
                {
                    ClearCondition(fd, memberName);
                }
            }
        }

        public static Func<T, bool> GetFilter<T>(this Telerik.Windows.Data.IFilterDescriptor filterDescriptor)
        {
            // Create parameter of Type T
            System.Linq.Expressions.ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(T));

            // Create filter expression
            var result = filterDescriptor.CreateFilterExpression(parameter);

            // Compile Lambda expression
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(result, parameter).Compile();
        }
    }
}
