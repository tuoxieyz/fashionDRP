using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;
using System.Windows.Input;
using DBAccess;
using Kernel;

namespace ViewModelBasic
{
    public abstract class CommonViewModel<TEntity> : ViewModelBase
        where TEntity : class
    {
        private IEnumerable<TEntity> _entities = null;
        public IEnumerable<TEntity> Entities
        {
            get { return _entities; }
            set
            {
                _entities = value;
                OnPropertyChanged("Entities");
            }
        }

        public virtual ICommand SearchCommand
        {
            get
            {
                return new DelegateCommand(param =>
                {
                    Entities = this.SearchData();
                });
            }
        }

        protected abstract IEnumerable<TEntity> SearchData();
    }
}
