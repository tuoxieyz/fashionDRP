using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kernel;

namespace ViewModelBasic
{
    public abstract class PagedReportVM<TEntity> : CommonViewModel<TEntity>
        where TEntity : class
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
    }
}
