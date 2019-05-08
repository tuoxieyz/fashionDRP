using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using System.Collections.ObjectModel;
using SysProcessViewModel;
using Telerik.Windows.Data;

namespace DistributionViewModel
{
    public class OrganizationGoodReturnRateBO : OrganizationGoodReturnRate, IDataErrorInfo
    {
        private ICollectionView _yearQuarterRates;
        public ICollectionView YearQuarterRates
        {
            get
            {
                if (_yearQuarterRates == null)
                {
                    var rates = VMGlobal.DistributionQuery.LinqOP.Search<OrganizationGoodReturnRatePerQuarter>(o => o.RateID == this.ID).Select(o => new OrganizationGoodReturnRatePerQuarterBO(o)).ToList();
                    _yearQuarterRates = new QueryableCollectionView(rates);
                }
                return _yearQuarterRates;
            }
        }

        public OrganizationGoodReturnRateBO()
        { }

        public OrganizationGoodReturnRateBO(OrganizationGoodReturnRate rate)
        {
            this.ID = rate.ID;
            this.BrandID = rate.BrandID;
            this.OrganizationID = rate.OrganizationID;
            this.GoodReturnRate = rate.GoodReturnRate;
            CreateTime = rate.CreateTime;
            CreatorID = rate.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "BrandID")
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

    public class OrganizationGoodReturnRatePerQuarterBO : OrganizationGoodReturnRatePerQuarter, IDataErrorInfo
    {
        public string YearString
        {
            get
            {
                if (base.Year == default(int))
                    base.Year = DateTime.Now.Year;
                return base.Year.ToString();
            }
            set
            {
                int year;
                bool flag = int.TryParse(value, out year);
                base.Year = flag ? year : DateTime.Now.Year;
            }
        }

        public OrganizationGoodReturnRatePerQuarterBO()
        { }

        public OrganizationGoodReturnRatePerQuarterBO(OrganizationGoodReturnRatePerQuarter rate)
        {
            this.ID = rate.ID;
            this.RateID = rate.RateID;
            this.Year = rate.Year;
            this.Quarter = rate.Quarter;
            this.GoodReturnRate = rate.GoodReturnRate;
            CreateTime = rate.CreateTime;
            CreatorID = rate.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Year")
            {
                if (Year == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "Quarter")
            {
                if (Quarter == default(int))
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
}
