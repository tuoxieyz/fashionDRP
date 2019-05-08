using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBAccess;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using Kernel;

namespace ViewModelBasic
{
    public class EditSynchronousVM<TEntity> : SynchronousViewModel<TEntity>, ICUDOper<TEntity>
        where TEntity : class,IDEntity
    {
        protected LinqOPEncap LinqOP;

        public virtual CompositeFilterDescriptorCollection FilterDescriptors
        {
            get { return new CompositeFilterDescriptorCollection(); }
        }

        public EditSynchronousVM(LinqOPEncap linqOP)
        {
            LinqOP = linqOP;
        }

        protected override IEnumerable<TEntity> SearchData()
        {
            var all = LinqOP.GetDataContext<TEntity>();
            var filteredData = (IQueryable<TEntity>)all.Where(FilterDescriptors);
            return filteredData.ToList();
        }

        public virtual OPResult AddOrUpdate(TEntity entity)
        {
            //var entity = bo as TEntity;//这样貌似不行，即使重载了类型转换符，使用as语法返回的仍然是null
            var id = entity.ID;
            try
            {
                if (id == default(int))
                {
                    entity.ID = LinqOP.Add<TEntity, int>(entity, o => o.ID);
                }
                else
                {
                    LinqOP.Update<TEntity>(entity);
                }
            }
            catch (Exception e)
            {
                entity.ID = id;
                return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
            }

            return new OPResult { IsSucceed = true, Message = "保存成功." };
        }

        public virtual OPResult Delete(TEntity entity)
        {
            if (entity.ID == default(int))
                return new OPResult { IsSucceed = true, Message = "删除成功!" };
            try
            {
                LinqOP.Delete<TEntity>(entity);
                return new OPResult { IsSucceed = true, Message = "删除成功!" };
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:\n" + e.Message };
            }
        }
    }
}
