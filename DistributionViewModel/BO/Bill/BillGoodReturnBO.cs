using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;

namespace DistributionViewModel
{
    public class BillGoodReturnForSubordinate : BillGoodReturn, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public int SubordinateOrganizationID
        {
            get { return base.OrganizationID; }
            set
            {
                base.OrganizationID = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("SubordinateOrganizationID"));
                }
            }
        }
    }

    public class BillGoodReturnBO : BillGoodReturn, IDataErrorInfo//BillWithBrandBO
    {
        public BillGoodReturnBO()
        { }

        public BillGoodReturnBO(BillGoodReturn goodreturn)
        {
            this.ID = goodreturn.ID;
            this.Code = goodreturn.Code;
            this.CreatorID = goodreturn.CreatorID;
            this.CreateTime = goodreturn.CreateTime;
            this.Remark = goodreturn.Remark;
            this.Status = goodreturn.Status;
            this.StorageID = goodreturn.StorageID;
            this.OrganizationID = goodreturn.OrganizationID;
            this.BrandID = goodreturn.BrandID;
            this.IsDefective = goodreturn.IsDefective;
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

    public enum BillGoodReturnStatusEnum
    {
        在途中 = 0,
        已入库 = 1,
        被退回 = 2,
        退回已入库 = 3,
        未审核 = 4
    }
}
