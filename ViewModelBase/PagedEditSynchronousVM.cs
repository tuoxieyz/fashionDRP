using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;
using DBAccess;
using Model.Extension;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;

namespace ViewModelBasic
{
    public class PagedEditSynchronousVM<TEntity> : EditSynchronousVM<TEntity>
        where TEntity : class,IDEntity
    {
        private int _pageIndex = 0;
        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                _pageIndex = value;
                Entities = this.SearchData();
                //OnPropertyChanged("PageIndex");
            }
        }

        private int _pageSize = 20;
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                _pageSize = value;
                //OnPropertyChanged("PageSize");
            }
        }

        private int _totalCount = 20;
        public int TotalCount
        {
            get { return _totalCount; }
            set
            {
                _totalCount = value;
                OnPropertyChanged("TotalCount");
            }
        }

        public PagedEditSynchronousVM(LinqOPEncap linqOP)
            : base(linqOP)
        {
            //Entities = new List<TEntity>();
        }

        protected override IEnumerable<TEntity> SearchData()
        {
            var all = LinqOP.GetDataContext<TEntity>();
            var filteredData = (IQueryable<TEntity>)all.Where(FilterDescriptors);
            TotalCount = filteredData.Count();
            return filteredData.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
        }
    }
}
