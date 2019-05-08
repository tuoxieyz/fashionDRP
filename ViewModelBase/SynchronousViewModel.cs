using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Telerik.Windows.Data;
using DBAccess;

namespace ViewModelBasic
{
    public abstract class SynchronousViewModel<TEntity> : CommonViewModel<TEntity>
        where TEntity : class
    {
        private ICollectionView _synEntities;
        public ICollectionView SynEntities
        {
            get
            {
                if (_synEntities == null)
                {
                    if (Entities != null)
                        _synEntities = new QueryableCollectionView(Entities);
                }
                return _synEntities;
            }
        }

        public SynchronousViewModel()
        {
            base.PropertyChanged += new PropertyChangedEventHandler(SynchronousViewModel_PropertyChanged);
        }

        void SynchronousViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Entities")
            {
                _synEntities = null;
                OnPropertyChanged("SynEntities");
            }
        }
    }
}
