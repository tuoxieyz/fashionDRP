using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using Manufacturing.ViewModel;
using System.Windows.Input;
using Telerik.Windows.Controls;
using System.Windows;
using System.Transactions;
using ManufacturingModel;
using ERPViewModelBasic;
using System.Collections.ObjectModel;

namespace DistributionViewModel
{
    public class BillStoringBO : BillStoring, IDataErrorInfo//BillWithBrandBO
    {
        public BillStoringBO()
        { }

        public BillStoringBO(BillStoring storing)
        {
            this.ID = storing.ID;
            this.Code = storing.Code;
            this.CreatorID = storing.CreatorID;
            this.CreateTime = storing.CreateTime;
            this.Remark = storing.Remark;
            this.BillType = storing.BillType;
            this.StorageID = storing.StorageID;
            this.OrganizationID = storing.OrganizationID;
            this.BrandID = storing.BrandID;
            this.RefrenceBillCode = storing.RefrenceBillCode;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "BrandID")
            {
                if (BrandID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "StorageID")
            {
                if (StorageID == default(int))
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

    public class BillStoringProductExchangeEntity : BillProductExchangeSearchEntity
    {
        /// <summary>
        /// 入库仓ID
        /// </summary>
        public int StorageID { get; set; }
    }

    public class ProductForStoringWhenReceiving : DistributionProductShow
    {
        private int _rquantity = 0;
        /// <summary>
        /// 收货数量
        /// </summary>
        public int ReceiveQuantity
        {
            get { return _rquantity; }
            set
            {
                _rquantity = value;
                OnPropertyChanged("ReceiveQuantity");
            }
        }

        private ObservableCollection<string> _receivedUniqueCodes;
        public ObservableCollection<string> ReceivedUniqueCodes
        {
            get
            {
                if (_receivedUniqueCodes == null)
                    _receivedUniqueCodes = new ObservableCollection<string>();
                return _receivedUniqueCodes;
            }
        }
    }

    /// <summary>
    /// 入库查询实体
    /// </summary>
    public class StoringSearchEntity
    {
        public int StorageID { get; set; }
        public DateTime CreateTime { get; set; }
        public int BillType { get; set; }
        public int BillID { get; set; }
        public int BrandID { get; set; }
        public string 单据编号 { get; set; }
        public string 入库品牌 { get; set; }
        public string 入库仓库 { get; set; }
        public string 入库类型 { get; set; }
        public string 相关单据编号 { get; set; }
        public DateTime 开单时间 { get; set; }
        public string 开单人 { get; set; }
        public int 入库数量 { get; set; }
        public string 备注 { get; set; }
    }

    /// <summary>
    /// 用于处入库汇总报表实体
    /// </summary>
    public class StoreOIEntityForAggregation : BillEntityForAggregation
    {
        //public int StorageID { get; set; }
        public int BillType { get; set; }
    }
}
