using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SysProcessViewModel;
using System.Linq.Expressions;
using System.Reflection;

namespace ERPViewModelBasic
{
    public class BillReportHelper
    {
        private Dictionary<string, Delegate> _dicFunc;

        /// <summary>
        /// 将尺码横排显示
        /// </summary>
        /// <param name="products">原数据集</param>
        /// <param name="propertyNameForSize">转换到尺码列下的属性</param>
        /// <param name="propertyNamesForAggregation">转换完毕后需要汇总的属性集合</param>
        public DataTable TransferSizeToHorizontal<T>(IEnumerable<T> products, string propertyNameForSize = "Quantity", IEnumerable<string> propertyNamesForSum = null) where T : ProductShow
        {
            if (propertyNamesForSum == null)
                propertyNamesForSum = new string[] { propertyNameForSize };

            var props = typeof(T).GetProperties();
            var dt = new DataTable();
            IEnumerable<string> sizeNames = products.Select(p=>p.SizeName).Distinct();
            foreach (var prop in props)
            {
                if (prop.Name == "SizeName")
                {
                    //dt.Columns.AddRange(VMGlobal.Sizes.Select(s => new DataColumn(s.Name, typeof(int))).ToArray());
                    dt.Columns.AddRange(sizeNames.Select(s => new DataColumn(s, typeof(int))).ToArray());
                }
                else
                    dt.Columns.Add(new DataColumn(prop.Name, prop.PropertyType));
            }
            var func = GetGetDelegate<T>(dt.Columns, propertyNamesForSum,sizeNames);
            var scs = products.Select(o => o.StyleCode + o.ColorCode).Distinct().ToArray();
            foreach (string sc in scs)
            {
                var ps = products.Where(o => (o.StyleCode + o.ColorCode) == sc);
                var row = dt.Rows.Add(func(ps.ElementAt(0)));
                foreach (var p in ps)
                {
                    row[p.SizeName] = GetGetDelegate<T>(propertyNameForSize)(p);
                    foreach (var psum in propertyNamesForSum)
                    {
                        switch (dt.Columns[psum].DataType.Name.ToLower())
                        {
                            case "int32":
                                row[psum]=(int)row[psum]+(int)GetGetDelegate<T>(psum)(p);
                                break;
                            case "decimal":
                                row[psum] = (decimal)row[psum] + (decimal)GetGetDelegate<T>(psum)(p);
                                break;
                            case "float":
                                row[psum] = (float)row[psum] + (float)GetGetDelegate<T>(psum)(p);
                                break;
                        }
                    }
                }                
            }
            return dt;
        }

        Func<T, object[]> GetGetDelegate<T>(DataColumnCollection props, IEnumerable<string> propertyNamesForSum, IEnumerable<string> sizeNames)
        {
            var param_obj = Expression.Parameter(typeof(T), "obj");
            List<Expression> memberExps = new List<Expression>();
            foreach (DataColumn prop in props)
            {
                if (sizeNames.Contains(prop.ColumnName) || propertyNamesForSum.Contains(prop.ColumnName))
                {
                    memberExps.Add(Expression.Convert(Expression.Constant(0), typeof(object)));
                }
                else
                    memberExps.Add(Expression.Convert(Expression.Property(param_obj, prop.ColumnName), typeof(object)));
            }
            Expression newArrayExpression = Expression.NewArrayInit(typeof(object), memberExps);
            return Expression.Lambda<Func<T, object[]>>(newArrayExpression, param_obj).Compile();
        }

        Func<T, object> GetGetDelegate<T>(string propertyName)
        {
            if (_dicFunc == null)
                _dicFunc = new Dictionary<string, Delegate>();
            if (!_dicFunc.ContainsKey(propertyName))
            {
                var param_obj = Expression.Parameter(typeof(T), "obj");
                //lambda的方法体 u.Age
                var pGetter = Expression.Convert(Expression.Property(param_obj, propertyName), typeof(object));
                //编译lambda
                _dicFunc.Add(propertyName, Expression.Lambda<Func<T, object>>(pGetter, param_obj).Compile());
            }
            return (Func<T, object>)_dicFunc[propertyName];
        }
    }
}
