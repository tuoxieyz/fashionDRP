using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data;
using Model.Extension;

namespace DBEncapsulation
{
    public abstract class DbCommonOPEncap
    {
        public abstract TEntity GetById<TEntity>(object id) where TEntity : class;
        public abstract List<TEntity> Search<TEntity>(Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue) where TEntity : class;
        public abstract List<R> Search<TEntity, R>(Expression<Func<TEntity, R>> selector, Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue) where TEntity : class;
        public abstract List<dynamic> Search<TEntity>(Expression<Func<TEntity, dynamic>> selector, Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue) where TEntity : class;
        public abstract int Add<TEntity>(TEntity entity) where TEntity : class;
        public abstract R Add<TEntity, R>(TEntity entity, Func<TEntity, R> result) where TEntity : class;
        public abstract void Add<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        public abstract void Update<TEntity>(TEntity instance) where TEntity : class;
        public abstract int Delete<TEntity>(TEntity entity) where TEntity : class;
        public abstract int Delete<TEntity>(Expression<Func<TEntity, bool>> deleteCheck) where TEntity : class;
        public abstract int Delete<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;
        public abstract bool Any<TEntity>(Expression<Func<TEntity, bool>> condition) where TEntity : class;
        public abstract int AddOrUpdate<TEntity>(TEntity entity) where TEntity : class,IDEntity;
        public abstract int AddOrUpdate<TEntity>(IEnumerable<TEntity> entities) where TEntity : class,IDEntity;

        public abstract void DoWithContext(Action<DbContext> action);
    }

    public class DbCommonOPEncap<TDbContext> : DbCommonOPEncap where TDbContext : DbContext, new()
    {
        #region 查询

        public override TEntity GetById<TEntity>(object id)
        {
            TEntity entity = null;
            this.DoWithContext(context =>
            {
                var dbset = context.Set<TEntity>();
                entity = dbset.Find(id);
            });
            return entity;
        }

        /// <summary>
        /// 查询符合条件的集合
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>符合条件的对象集合</returns>
        public override List<TEntity> Search<TEntity>(Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            return Search(t => t, condition, order, skip, take);
        }

        /// <summary>
        /// 从TEntity集合中查询出符合条件的R集合
        /// </summary>
        /// <typeparam name="TEntity">基础类型</typeparam>
        /// <typeparam name="R">构造类型</typeparam>
        /// <param name="selector">构造委托</param>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序。基于基础类型，即前排序，而非基于构造类型的后排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>构造类型对象集合</returns>
        public override List<R> Search<TEntity, R>(Expression<Func<TEntity, R>> selector, Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            using (var context = new TDbContext())
            {
                IQueryable<TEntity> query = ((IQueryable<TEntity>)context.Set<TEntity>());
                if (selector == null)
                    throw new ArgumentNullException("selector", "涉及类型转换的构造委托不能为空");
                if (condition != null)
                    query = query.Where(condition);
                if (order != null)
                    query = query.OrderBy(order).Skip(skip).Take(take);
                return query.Select(selector).ToList();
                //return entities.Where(condition).OrderBy(order).Select(selector).Skip(skip).Take(take).ToList();
            }
        }

        /// <summary>
        /// 从TEntity集合中查询出符合条件的集合
        /// </summary>
        /// <typeparam name="TEntity">基础类型</typeparam>       
        /// <param name="selector">构造委托</param>
        /// <param name="condition">查询条件</param>
        /// <param name="order">排序规则，目前只支持单属性升序排序。基于基础类型，即前排序，而非基于构造类型的后排序</param>
        /// <param name="skip">从第几条数据开始</param>
        /// <param name="take">取几条数据</param>
        /// <returns>构造类型对象集合</returns>
        public override List<dynamic> Search<TEntity>(Expression<Func<TEntity, dynamic>> selector, Expression<Func<TEntity, bool>> condition = null, Expression<Func<TEntity, dynamic>> order = null, int skip = 0, int take = int.MaxValue)
        {
            using (var context = new TDbContext())
            {
                IQueryable<TEntity> query = ((IQueryable<TEntity>)context.Set<TEntity>());
                if (selector == null)
                    selector = t => t;
                if (condition != null)
                    query = query.Where(condition);
                if (order != null)
                    query = query.OrderBy(order).Skip(skip).Take(take);
                return query.Select(selector).ToList();
                //.Select(selector).Where(condition)顺序换下，就报错，因为传入的参数类型可能（在此处一定）会发生改变
                //return entities.Where(condition).OrderBy(order).Select(selector).Skip(skip).Take(take).ToList();
            }
        }

        #endregion

        #region 保存

        /// <summary>
        /// 保存实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型参数</typeparam>
        /// <param name="entity">实体对象</param>
        /// <returns>影响数据库的行数</returns>
        public override int Add<TEntity>(TEntity entity)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                dbset.Add(entity);
                return context.SaveChanges();//如果主键字段是自增标识列，会将该自增值返回给实体对象对应的属性
            }
        }

        public override R Add<TEntity, R>(TEntity entity, Func<TEntity, R> result)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                dbset.Add(entity);
                context.SaveChanges();
            }
            return result(entity);
        }

        /// <summary>
        /// 批量保存实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型参数</typeparam>
        /// <param name="entities">实体对象集合</param>
        public override void Add<TEntity>(IEnumerable<TEntity> entities)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                foreach (var entity in entities)
                    dbset.Add(entity);
                context.SaveChanges();
            }
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新符合条件的实体
        /// </summary>
        /// <typeparam name="TEntity">实体类型参数</typeparam>
        /// <param name="instance">待更新实体</param>
        public override void Update<TEntity>(TEntity instance)
        {
            using (var context = new TDbContext())
            {
                context.Entry(instance).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        #endregion

        #region 删除

        public override int Delete<TEntity>(TEntity entity)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                context.Entry(entity).State = EntityState.Deleted;
                return context.SaveChanges();
            }
        }

        /// <summary>
        /// 删除所有符合条件的记录
        /// </summary>
        /// <typeparam name="TEntity">实体类型参数</typeparam>
        /// <param name="deleteCheck">删除条件</param>
        /// <returns>影响的行数</returns>
        public override int Delete<TEntity>(Expression<Func<TEntity, bool>> deleteCheck)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                var entities = dbset.Where(deleteCheck).ToList();
                foreach (var entity in entities)
                {
                    dbset.Remove(entity);
                }
                return context.SaveChanges();
                //return dbset.Delete(deleteCheck);
                //return context.SaveChanges();//这句还要不要？——不需要了
            }
        }

        /// <summary>
        /// 删除集合中所有符合条件的记录
        /// </summary>
        /// <typeparam name="TEntity">实体类型参数</typeparam>
        /// <param name="entities">集合</param>
        /// <param name="deleteCheck">删除条件</param>
        /// <returns>每个实体删除所影响的数据库行数</returns>
        public override int Delete<TEntity>(IEnumerable<TEntity> entities)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                foreach (var entity in entities)
                {
                    context.Entry(entity).State = EntityState.Deleted;
                }
                return context.SaveChanges();
            }
        }

        #endregion

        #region 其它

        public override bool Any<TEntity>(Expression<Func<TEntity, bool>> condition)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                return dbset.Any(condition);
            }
        }

        public override int AddOrUpdate<TEntity>(TEntity entity)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                if (entity.ID == default(int))
                {
                    dbset.Add(entity);
                }
                else
                {
                    context.Entry(entity).State = EntityState.Modified;
                }
                return context.SaveChanges();
            }
        }

        public override int AddOrUpdate<TEntity>(IEnumerable<TEntity> entities)
        {
            using (var context = new TDbContext())
            {
                var dbset = context.Set<TEntity>();
                foreach (var entity in entities)
                {
                    if (entity.ID == default(int))
                    {
                        dbset.Add(entity);
                    }
                    else
                    {
                        context.Entry(entity).State = EntityState.Modified;
                    }
                }
                return context.SaveChanges();
            }
        }

        public override void DoWithContext(Action<DbContext> action)
        {
            using (var context = new TDbContext())
            {
                action(context);
            }
        }

        #endregion

    }
}
