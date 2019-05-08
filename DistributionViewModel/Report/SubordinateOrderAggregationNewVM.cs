using ERPViewModelBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class SubordinateOrderAggregationNewVM : SubordinateOrderAggregationVM
    {
        private BillReportHelper _billReportHelper = new BillReportHelper();

        protected IEnumerable<string> PropertyNamesForSum
        {
            get { return new string[] { "Quantity", "QuaStock","QuaAvailableStock" }; }
        }

        private DataTable _tableData = null;
        public DataTable TableData
        {
            get
            {
                if (_tableData == null)
                {
                    if (Entities != null)
                        _tableData = _billReportHelper.TransferSizeToHorizontal<OrderAggregationEntity>(Entities, propertyNamesForSum: PropertyNamesForSum);
                }
                return _tableData;
            }
        }

        private bool _isShowAll = false;
        public bool IsShowAll
        {
            get { return _isShowAll; }
            set
            {
                if (_isShowAll != value)
                {
                    _isShowAll = value;
                    if (!_isShowAll)
                    {
                        UnShowAll();
                        RefreshTableData();
                    }
                    else if (Entities != null)
                    {
                        Entities = this.SearchData();
                    }
                }
            }
        }

        public SubordinateOrderAggregationNewVM()
        {
            base.PropertyChanged += SubordinateOrderAggregationNewVM_PropertyChanged;
        }

        void SubordinateOrderAggregationNewVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Entities" || e.PropertyName == "IsShowStock")
                RefreshTableData();
        }

        private void RefreshTableData()
        {
            _tableData = null;
            OnPropertyChanged("TableData");
        }

        protected override ObservableCollection<OrderAggregationEntity> AggregateOrder(IQueryable<OrderEntityForAggregation> data)
        {
            return SelfOrderAggregationNewVM.AggregateOrder(data, IsShowAll);
        }

        private void UnShowAll()
        {
            var data = Entities as ObservableCollection<OrderAggregationEntity>;
            SelfOrderAggregationNewVM.UnShowAll(data);
        }
    }
}
