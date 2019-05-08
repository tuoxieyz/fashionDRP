using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using ViewModelBasic;

namespace ERPViewModelBasic
{
    public abstract class BillReportWithHorSizeVM<TEntity> : CommonViewModel<TEntity> where TEntity : ProductShow
    {
        private BillReportHelper _billReportHelper = new BillReportHelper();

        protected virtual IEnumerable<string> PropertyNamesForSum
        {
            get { return new string[] { "Quantity" }; }
        }

        private DataTable _tableData = null;
        public DataTable TableData
        {
            get
            {
                if (_tableData == null)
                {
                    if (Entities != null)
                        _tableData = _billReportHelper.TransferSizeToHorizontal<TEntity>(Entities, propertyNamesForSum: PropertyNamesForSum);
                }
                return _tableData;
            }
        }

        public BillReportWithHorSizeVM()
        {
            base.PropertyChanged += BillReportWithHorSizeVM_PropertyChanged;
        }

        void BillReportWithHorSizeVM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Entities")
            {
                _tableData = null;
                OnPropertyChanged("TableData");
            }
        }
    }
}
