using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using DBLinqProvider;

namespace DBAccess
{
    /// <summary>
    /// 封装一些通用操作
    /// </summary>
    public class LinqOPEncap
    {
        private IEntityProvider _provider;

        public LinqOPEncap(IEntityProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider不能为空");

            this._provider = provider;
        }

        public IEntityProvider Provider
        {
            get { return this._provider; }
        }

        #region 保存

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns>影响数据库的行数</returns>
        public int Add<T>(T entity)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Insert(entity);
        }

        /// <summary>
        /// 保存实体并返回自定义对象
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entity">实体对象</param>
        /// <param name="result">自定义对象格式设置</param>
        /// <returns>自定义对象</returns>
        public dynamic Add<T>(T entity, Expression<Func<T, dynamic>> result)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Insert(entity, result);
        }

        public R Add<T, R>(T entity, Expression<Func<T, R>> result)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Insert(entity, result);
        }

        /// <summary>
        /// 批量保存实体（非事务性）
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entities">实体对象集合</param>
        /// <returns>每个实体保存所影响的数据库行数</returns>
        public IEnumerable<int> Add<T>(IEnumerable<T> entities)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.Insert(e));
        }

        /// <summary>
        /// 批量保存实体（非事务性）,并针对每个实体返回自定义对象
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entities">实体对象集合</param>
        /// <param name="result">自定义对象格式设置</param>
        /// <returns>每个实体保存所影响的数据库行数</returns>
        public IEnumerable<dynamic> Add<T>(IEnumerable<T> entities, Expression<Func<T, dynamic>> result)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.Insert(e, result));
        }

        #endregion

        #region 删除

        public int Delete<T>(T entity)
        {
            //转型为IUpdatable<T>，调用恰当的Delete方法，而非更基类的Delete方法
            return ((IUpdatable<T>)this._provider.GetTable<T>(typeof(T).Name)).Delete(entity);
        }

        /// <summary>
        /// 删除所有符合条件的记录
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="deleteCheck">删除条件</param>
        /// <returns>影响的行数</returns>
        public int Delete<T>(Expression<Func<T, bool>> deleteCheck)
        {
            //转型为IUpdatable<T>，调用恰当的Delete方法，而非更基类的Delete方法
            return ((IUpdatable<T>)this._provider.GetTable<T>(typeof(T).Name)).Delete(deleteCheck);
        }

        /// <summary>
        /// 删除集合中所有符合条件的记录
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entities">集合</param>
        /// <param name="deleteCheck">删除条件</param>
        /// <returns>每个实体删除所影响的数据库行数</returns>
        public IEnumerable<int> Delete<T>(IEnumerable<T> entities, Expression<Func<T, bool>> deleteCheck = null)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.Delete(e, deleteCheck));
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新符合条件的实体
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="instance">待更新实体</param>
        /// <param name="updateCheck">更新条件</param>
        /// <returns>受影响行数</returns>
        public int Update<T>(T instance, Expression<Func<T, bool>> updateCheck = null)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Update(instance, updateCheck);
        }

        /// <summary>
        /// 更新符合条件的实体并返回自定义类型
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="instance">待更新实体</param>
        /// <param name="result">自定义对象格式设置</param>
        /// <param name="updateCheck">更新条件</param>
        /// <returns>自定义对象</returns>
        public dynamic Update<T>(T instance, Expression<Func<T, dynamic>> result, Expression<Func<T, bool>> updateCheck = null)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Update(instance, updateCheck, result);
        }

        /// <summary>
        /// 批量更新符合条件的实体
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entities">待更新实体集合</param>
        /// <param name="updateCheck">更新条件</param>
        /// <returns>每个实体更新所影响的数据库行数</returns>
        public IEnumerable<int> Update<T>(IEnumerable<T> entities, Expression<Func<T, bool>> updateCheck = null)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.Update(e, updateCheck));
        }

        /// <summary>
        /// 批量更新符合条件的实体并返回自定义类型
        /// </summary>
        /// <typeparam name="T">实体类型参数</typeparam>
        /// <param name="entities">待更新实体集合</param>
        /// <param name="result">自定义对象格式设置</param>
        /// <param name="updateCheck">更新条件</param>
        /// <returns>自定义对象集合</returns>
        public IEnumerable<dynamic> Update<T>(IEnumerable<T> entities, Expression<Func<T, dynamic>> result, Expression<Func<T, bool>> updateCheck = null)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.Update(e, updateCheck, result));
        }

        #endregion

        #region 查询

        public T GetById<T>(object id)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            return entities.GetById(id);
        }

        /// <summary>
        /// 查询符合条件的集合
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>符合条件的对象集合</returns>
        public IQueryable<T> Search<T>(Expression<Func<T, bool>> condition = null, Expression<Func<T, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            return Search(t => t, condition, order, skip, take);
        }

        /// <summary>
        /// 从T集合中查询出符合条件的R集合
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>
        /// <typeparam name="R">构造类型</typeparam>
        /// <param name="selector">构造委托</param>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序。基于基础类型，即前排序，而非基于构造类型的后排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>构造类型对象集合</returns>
        public IQueryable<R> Search<T, R>(Expression<Func<T, R>> selector, Expression<Func<T, bool>> condition = null, Expression<Func<T, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            if (selector == null)
                throw new ArgumentNullException("selector", "涉及类型转换的构造委托不能为空");
            if (condition == null)
                condition = t => true;
            IQueryable<T> query = entities.Where(condition);
            if (order != null)
                query = query.OrderBy(order).Skip(skip).Take(take);
            return query.Select(selector);
            //return entities.Where(condition).OrderBy(order).Select(selector).Skip(skip).Take(take).ToList();
        }

        /// <summary>
        /// 从T集合中查询出符合条件的集合
        /// </summary>
        /// <typeparam name="T">基础类型</typeparam>       
        /// <param name="selector">构造委托</param>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序。基于基础类型，即前排序，而非基于构造类型的后排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>构造类型对象集合</returns>
        public IQueryable<dynamic> Search<T>(Expression<Func<T, dynamic>> selector, Expression<Func<T, bool>> condition = null, Expression<Func<T, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            if (selector == null)
                selector = t => t;
            if (condition == null)
                condition = t => true;
            IQueryable<T> query = entities.Where(condition);
            if (order != null)
                query = query.OrderBy(order).Skip(skip).Take(take);
            return query.Select(selector);
            //.Select(selector).Where(condition)顺序换下，就报错，因为传入的参数类型可能（在此处一定）会发生改变
            //return entities.Where(condition).OrderBy(order).Select(selector).Skip(skip).Take(take).ToList();
        }

        #endregion

        #region 其它

        public bool Any<T>(Expression<Func<T, bool>> condition)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            return entities.Any(condition);
        }

        public int AddOrUpdate<T>(T entity)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            return entities.InsertOrUpdate<T>(entity);
        }

        public R AddOrUpdate<T, R>(T entity, Expression<Func<T, R>> result)
        {
            var entities = this._provider.GetTable<T>(typeof(T).Name);
            return entities.InsertOrUpdate<T, R>(entity, o => true, result);
        }

        public IEnumerable<int> AddOrUpdate<T>(IEnumerable<T> entities)
        {
            return this._provider.GetTable<T>(typeof(T).Name).Batch(entities, (u, e) => u.InsertOrUpdate(e));
        }

        public IQueryable<T> GetDataContext<T>()
        {
            var entities = Provider.GetTable<T>(typeof(T).Name);
            return entities;
        }

        #endregion

    }
}
