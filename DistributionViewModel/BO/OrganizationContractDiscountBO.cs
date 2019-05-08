using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class OrganizationContractDiscountBO : OrganizationContractDiscount, INotifyPropertyChanged
    {
        public override int OrganizationID
        {
            get
            {
                return base.OrganizationID;
            }
            set
            {
                base.OrganizationID = value;
                OnPropertyChanged("OrganizationCode");
                OnPropertyChanged("OrganizationName");
            }
        }
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public string OrganizationCode
        {
            get
            {
                return VMGlobal.ChildOrganizations.Find(o => o.ID == this.OrganizationID).Code;
            }
        }
        public string OrganizationName {
            get
            {
                return VMGlobal.ChildOrganizations.Find(o => o.ID == this.OrganizationID).Name;
            }
        }

        protected override string CheckData(string columnName)
        {
            var errorInfo = base.CheckData(columnName);

            if (columnName == "Quarter")
            {
                if (Quarter == 0)
                    errorInfo = "季度必选";
            }
            else if (columnName == "BrandID")
            {
                if (BrandID == 0)
                    errorInfo = "品牌必选";
            }
            else if (columnName == "Year")
            {
                if (Year == default(int))
                    errorInfo = "年份必选";
            }
            return errorInfo;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
