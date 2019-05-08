using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManufacturingModel;
using System.ComponentModel;
using ERPViewModelBasic;
using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;

namespace Manufacturing.ViewModel
{
    public class BillProductExchangeBO : BillProductExchange, IDataErrorInfo
    {
        public BillProductExchange ConvertToBase()
        {
            return new BillProductExchange
            {
                OrganizationID = this.OrganizationID,
                BrandID = BrandID,
                Status = Status,
                IsDeleted = IsDeleted,
                OuterFactoryID = OuterFactoryID,
                Code = Code,
                CreateTime = CreateTime,
                CreatorID = CreatorID,
                ID = ID,
                Remark = Remark
            };
        }

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

    public class BillProductExchangeSearchEntity : BillProductExchange, INotifyPropertyChanged
    {
        public string CreatorName { get; set; }
        public string BrandName { get; set; }
        public DateTime CreateDate { get; set; }
        public string OuterFactoryName { get; set; }

        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        //private string _statusName;
        //public string StatusName
        //{
        //    get { return _statusName; }
        //    set
        //    {
        //        _statusName = value;
        //        OnPropertyChanged("StatusName");
        //    }
        //}

        public override int Status
        {
            get
            {
                return base.Status;
            }
            set
            {
                base.Status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("StatusName");
            }
        }

        public string StatusName
        {
            get
            {
                return Enum.GetName(typeof(BillProductExchangeStatusEnum), this.Status);
            }
        }

        public override bool IsDeleted
        {
            get
            {
                return base.IsDeleted;
            }
            set
            {
                base.IsDeleted = value;
                OnPropertyChanged("IsDeleted");
                OnPropertyChanged("IsDeletedName");
            }
        }

        public string IsDeletedName
        {
            get { return IsDeleted ? "已作废" : "有效"; }
        }

        private IEnumerable<ProductShow> _details;
        public IEnumerable<ProductShow> Details
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

        private List<ProductForProductExchange> GetBillDetails()
        {
            var lp = VMGlobal.ManufacturingQuery.LinqOP;
            var detailsContext = lp.Search<BillProductExchangeDetails>(o => o.BillID == this.ID);
            var productContext = lp.GetDataContext<ViewProduct>();
            var data = from details in detailsContext
                       from product in productContext
                       where details.ProductID == product.ProductID
                       select new ProductForProductExchange
                       {
                           ID = details.ID,
                           ProductID = details.ProductID,
                           ProductCode = product.ProductCode,
                           StyleCode = product.StyleCode,
                           BYQID = product.BYQID,
                           ColorID = product.ColorID,
                           SizeID = product.SizeID,
                           Quantity = details.Quantity
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

    public class ProductForProductExchange : ProductShow
    {
        public int ID { get; set; }
        public DateTime CreateDate { get; set; }
        public int OuterFactoryID { get; set; }
    }

    public enum BillProductExchangeStatusEnum
    {
        在途中 = 0,
        已入库 = 1,
        被退回 = 2
    }
}
