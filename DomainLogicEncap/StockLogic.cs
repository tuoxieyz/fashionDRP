using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using System.Linq.Expressions;
using DistributionModel;
using SysProcessModel;

namespace DomainLogicEncap
{
    public static class StockLogic
    {
        private static QueryGlobal _query = new QueryGlobal("DistributionConstr");

        /// <summary>
        /// 获取库存
        /// </summary>
        public static List<Stock> GetStockInStorage(int storageID, int brandID = 0, int[] productIDs = null)
        {
            var stocks = _query.LinqOP.Search<Stock>(o => o.StorageID == storageID);
            if (brandID != 0)
            {
                var products = _query.LinqOP.Search<ViewProduct>(o => o.BrandID == brandID);
                var pids = products.Select(o => o.ProductID).Distinct().ToArray();
                stocks = stocks.Where(o => pids.Contains(o.ProductID));
            }
            if (productIDs != null && productIDs.Length > 0)
            {
                stocks = stocks.Where(o => productIDs.Contains(o.ProductID));
            }
            return stocks.ToList();
        }

        /// <summary>
        /// 获取库存
        /// </summary>
        /// <param name="pcodes">条码区间(条码前几位)</param>
        public static List<Stock> GetStockInStorage(int storageID, int brandID, string[] pcodes)
        {
            var stocks = _query.LinqOP.Search<Stock>(o => o.StorageID == storageID);
            var products = _query.LinqOP.Search<ViewProduct>(o => o.BrandID == brandID);
            //Expression<Func<ViewProduct, bool>> condition = o => o.BrandID == brandID;
            var codeExp = GenerateOrElseConditionWithArray<ViewProduct>("ProductCode", "StartsWith", pcodes);
            if (codeExp != null)
                products = products.Where(codeExp);
            var pids = products.Select(o => o.ProductID).Distinct().ToArray();
            stocks = stocks.Where(o => pids.Contains(o.ProductID));
            return stocks.ToList();
        }

        /// <summary>
        /// 根据条件数据动态生成或连接条件
        /// </summary>
        /// <typeparam name="TSource">集合项类型</typeparam>
        /// <param name="sourcePropertyName">待比较的集合项属性</param>
        /// <param name="methodName">方法名称</param>
        /// <param name="objs">条件数据</param>
        /// <returns></returns>
        public static Expression<Func<TSource, bool>> GenerateOrElseConditionWithArray<TSource>(string sourcePropertyName, string methodName, IEnumerable<object> objs)
        {
            if (objs != null && objs.Count() > 0)
            {
                var len = objs.Count();
                var p = Expression.Parameter(typeof(TSource), "p");
                var propertyName = Expression.Property(p, sourcePropertyName);
                var body = Expression.Equal(Expression.Call(propertyName, methodName, null, Expression.Constant(objs.First())), Expression.Constant(true));
                for (int i = 1; i < len; i++)
                {
                    var pcode = objs.ElementAt(i);
                    body = Expression.OrElse(body, Expression.Call(propertyName, methodName, null, Expression.Constant(pcode)));
                }
                Expression<Func<TSource, bool>> orExp = Expression.Lambda<Func<TSource, bool>>(body, p);//此处p不能以Expression.Parameter(typeof(TSource), "p")代替,虽然生成的lambda结果一样,但执行时会报错
                return orExp;
            }
            return null;
        } 
    }
}
