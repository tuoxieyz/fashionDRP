using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using SysProcessModel;
using Kernel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class RetailMonthTagetBO : RetailMonthTaget, IDataErrorInfo
    {
        public DateTime? YearMonth
        {
            get
            {
                if (Year < 1 || Year > 9999 || Month < 1 || Month > 12)
                    return null;
                return new DateTime(Year, Month, 1);
            }
            set
            {
                if (value != null)
                {
                    Year = value.Value.Year;
                    Month = value.Value.Month;
                }
                else
                {
                    Year = Month = 0;
                }
            }
        }

        private string _creatorName;
        public string CreatorName
        {
            get
            {
                if (string.IsNullOrEmpty(_creatorName))
                {
                    _creatorName = VMGlobal.SysProcessQuery.LinqOP.GetById<SysUser>(this.CreatorID).Name;
                }
                return _creatorName;
            }
        }

        /// <summary>
        /// 实际销售业绩
        /// </summary>
        public decimal SaleActual { get; set; }

        private List<BillRetailBOTemp> _saleDetails;
        /// <summary>
        /// 实际销售明细
        /// </summary>
        public List<BillRetailBOTemp> SaleDetails
        {
            get
            {
                if (_saleDetails == null)
                {
                    var lp = VMGlobal.DistributionQuery.LinqOP;
                    var ym = this.YearMonth.Value;
                    var retails = lp.Search<BillRetail>(o => o.OrganizationID == this.OrganizationID && new DateTime(o.CreateTime.Year, o.CreateTime.Month, 1) == ym);
                    var ids = retails.Select(o => o.ID).ToArray();
                    if (ids.Count() == 0)
                        _saleDetails = new List<BillRetailBOTemp>();
                    else
                    {
                        string idarray = "";
                        foreach (int id in ids)
                        {
                            idarray += (id + ",");
                        }
                        var db = VMGlobal.DistributionQuery.DB;
                        var ds = db.ExecuteDataSet("GetOrganizationDayRetailAchievement", idarray.TrimEnd(','));
                        _saleDetails = ds.Tables[0].ToList<BillRetailBOTemp>();
                    }
                }
                return _saleDetails;
            }
        }

        public string OrganizationName { get; set; }
        public string OrganizationCode { get; set; }

        public RetailMonthTagetBO()
        { }

        public RetailMonthTagetBO(RetailMonthTaget target)
        {
            this.ID = target.ID;
            OrganizationID = target.OrganizationID;
            this.Year = target.Year;
            Month = target.Month;
            SaleTaget = target.SaleTaget;
            CreateTime = target.CreateTime;
            CreatorID = target.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "YearMonth")
            {
                if (YearMonth == null)
                    errorInfo = "不能为空";
            }
            else if (columnName == "OrganizationID")
            {
                if (OrganizationID == default(int))
                    errorInfo = "不能为空";
            }
            else if (columnName == "SaleTaget")
            {
                if (SaleTaget <= 0)
                    errorInfo = "不能为空且必须大于0";
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
