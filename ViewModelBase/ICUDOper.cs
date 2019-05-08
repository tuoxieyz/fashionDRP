using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;

namespace ViewModelBasic
{
    public interface ICUDOper<TEntity> where TEntity : class
    {
        OPResult AddOrUpdate(TEntity entity);
        OPResult Delete(TEntity entity);
    }
}
