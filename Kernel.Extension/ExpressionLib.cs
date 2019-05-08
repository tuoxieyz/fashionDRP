using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Kernel
{
    /// <summary>
    /// 表达式树工厂类
    /// </summary>
    public static class ExpressionLib
    {
        /// <summary>
        /// 获取给定属性值
        /// </summary>
        public static Expression<Func<T, object>> GetPropertyValue<T>(PropertyInfo pi)
        {
            ParameterExpression entity = Expression.Parameter(typeof(T), "t");
            Expression pValue = Expression.Property(entity, pi);
            return Expression.Lambda<Func<T, object>>(pValue, entity);
        }
    }
}
