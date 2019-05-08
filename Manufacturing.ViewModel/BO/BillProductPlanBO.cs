using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using System.ComponentModel;
using SysProcessViewModel;
using SysProcessModel;
using ERPViewModelBasic;

namespace Manufacturing.ViewModel
{
    public class BillProductPlanBO : BillProductPlan, IDataErrorInfo
    {
        protected virtual string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }

            return errorInfo;
        }

        string IDataErrorInfo.Error
        {
            get { return ""; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                return this.CheckData(columnName);
            }
        }
    }

    public class ProductForProduceBrush : ProductShow
    {
        public DateTime DeliveryDate { get; set; }
        public int ID { get; set; }
        private int _quaCancel;
        public int QuaCancel
        {
            get { return _quaCancel; }
            set
            {
                _quaCancel = value;
                OnPropertyChanged("QuaCancel");
                OnPropertyChanged("Status");
            }
        }

        private int _quaCompleted;
        public int QuaCompleted
        {
            get { return _quaCompleted; }
            set
            {
                _quaCompleted = value;
                OnPropertyChanged("QuaCompleted");
                OnPropertyChanged("Status");
            }
        }

        public int MaxQuaCancel { get { return Quantity - QuaCompleted; } }
        public int MaxQuaCompleted { get { return Quantity - QuaCancel; } }

        /// <summary>
        /// 单据状态
        /// </summary>
        public string Status
        {
            get
            {
                var realQua = Quantity - QuaCancel;
                var status = realQua == QuaCompleted ? "已完成" : (QuaCompleted == 0 ? "未交货" : ((realQua > QuaCompleted ? "部分已交货" : "数据有误")));
                return status;
            }
        }

        public DateTime CreateDate { get; set; }
    }

    public class BillProductPlanSearchEntity : BillProductPlan, INotifyPropertyChanged
    {
        public string CreatorName { get; set; }
        public string BrandName { get; set; }
        public DateTime CreateDate { get; set; }
        public int Quantity { get; set; }

        private int _quaCancel;
        public int QuaCancel
        {
            get { return _quaCancel; }
            set
            {
                _quaCancel = value;
                OnPropertyChanged("QuaCancel");
            }
        }

        private int _quaCompleted;
        public int QuaCompleted
        {
            get { return _quaCompleted; }
            set
            {
                _quaCompleted = value;
                OnPropertyChanged("QuaCompleted");
            }
        }

        private string _statusName;
        public string StatusName
        {
            get { return _statusName; }
            set
            {
                _statusName = value;
                OnPropertyChanged("StatusName");
            }
        }

        private IEnumerable<ProductForProduceBrush> _details;
        public IEnumerable<ProductForProduceBrush> Details
        {
            get
            {
                if (_details == null)
                {
                    _details = this.GetBillDetails();
                }
                return _details;
            }
        }

        private List<ProductForProduceBrush> GetBillDetails()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var detailsContext = lp.Search<BillProductPlanDetails>(o => o.BillID == this.ID);
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID// && details.IsDeleted == false
                       select new ProductForProduceBrush
                       {
                           ProductID = details.ProductID,
                           ID = details.ID,
                           ProductCode = product.ProductCode,
                           StyleCode = product.StyleCode,
                           BYQID = product.BYQID,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity,
                           QuaCancel = details.QuaCancel,
                           QuaCompleted = details.QuaCompleted,
                           DeliveryDate = details.DeliveryDate
                       };
            var result = data.ToList();
            foreach (var r in result)
            {
                r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                r.BrandID = VMGlobal.BYQs.Find(o => o.ID == r.BYQID).BrandID;
                r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
            }
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
