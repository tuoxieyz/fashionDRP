using DomainLogicEncap;
using ERPViewModelBasic;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class SelfOrderAggregationNewVM : SelfOrderAggregationVM
    {
        private BillReportHelper _billReportHelper = new BillReportHelper();

        protected IEnumerable<string> PropertyNamesForSum
        {
            get { return new string[] { "Quantity", "QuaStock" }; }
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

        public SelfOrderAggregationNewVM()
        {
            base.PropertyChanged += SelfOrderAggregationNewVM_PropertyChanged;
        }

        void SelfOrderAggregationNewVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Entities" || e.PropertyName == "IsShowStock")
                RefreshTableData();
        }

        private void RefreshTableData()
        {
            _tableData = null;
            OnPropertyChanged("TableData");
        }

        internal static ObservableCollection<OrderAggregationEntity> AggregateOrder(IQueryable<OrderEntityForAggregation> data, bool isShowAll)
        {
            var tempData = data.GroupBy(o => o.ProductID).Select(g => new { g.Key, Quantity = g.Sum(o => o.Quantity), QuaDelivered = g.Sum(o => o.QuaDelivered) });
            if (!isShowAll)
                tempData = tempData.Where(o => o.Quantity != o.QuaDelivered);
            var temp = tempData.ToList();
            var pids = temp.Select(o => o.Key).ToArray();
            var products = VMGlobal.DistributionQuery.LinqOP.Search<ViewProduct>(o => pids.Contains(o.ProductID)).ToList();
            FloatPriceHelper fpHelper = new FloatPriceHelper();
            temp.RemoveAll(o => o.Quantity == 0 && o.QuaDelivered == 0);
            return new ObservableCollection<OrderAggregationEntity>(temp.Select(o =>
            {
                var product = products.First(p => p.ProductID == o.Key);
                var entity = new OrderAggregationEntity
                {
                    ProductID = o.Key,
                    ProductCode = product.ProductCode,
                    StyleCode = product.StyleCode,
                    BYQID = product.BYQID,
                    NameID = product.NameID,
                    ColorID = product.ColorID,
                    SizeID = product.SizeID,
                    Price = fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, product.BYQID, product.Price),
                    Quantity = isShowAll ? o.Quantity : o.Quantity - o.QuaDelivered,
                    QuaDelivered = o.QuaDelivered
                };
                entity.ColorCode = VMGlobal.Colors.Find(c => c.ID == entity.ColorID).Code;
                entity.SizeName = VMGlobal.Sizes.Find(c => c.ID == entity.SizeID).Name;
                entity.ProductName = VMGlobal.ProNames.Find(c => c.ID == entity.NameID).Name;
                var byq = VMGlobal.BYQs.Find(c => c.ID == entity.BYQID);
                entity.BrandID = byq.BrandID;
                entity.BrandCode = VMGlobal.PoweredBrands.Find(c => c.ID == entity.BrandID).Code;
                return entity;
            }).ToList());
        }

        protected override ObservableCollection<OrderAggregationEntity> AggregateOrder(IQueryable<OrderEntityForAggregation> data)
        {
            return SelfOrderAggregationNewVM.AggregateOrder(data, IsShowAll);
        }

        internal static void UnShowAll(ObservableCollection<OrderAggregationEntity> data)
        {
            if (data != null)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    var d = data[i];
                    d.Quantity = d.Quantity - d.QuaDelivered;
                }
            }
        }

        private void UnShowAll()
        {
            var data = Entities as ObservableCollection<OrderAggregationEntity>;
            SelfOrderAggregationNewVM.UnShowAll(data);
        }
    }
}
