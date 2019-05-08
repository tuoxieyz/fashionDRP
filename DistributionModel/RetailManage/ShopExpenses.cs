using DBLinqProvider.Data.Mapping;
using Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionModel
{
    //public class ShopExpenses
    //{
    //    [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
    //    public int ID { get; set; }
    //    public int OrganizationID { get; set; }
    //    public int Year { get; set; }
    //    public int Month { get; set; }
    //    public decimal BaseCost { get; set; }
    //    public decimal ElectricCharge { get; set; }
    //    public decimal ExpressCharge { get; set; }
    //    public decimal TelephoneCharge { get; set; }
    //    public decimal Rent { get; set; }
    //    public decimal RenovationCost { get; set; }
    //    public decimal SocialSecurityCharge { get; set; }
    //    public decimal Salary { get; set; }
    //    public decimal Bonus { get; set; }
    //    public decimal AdvertisingFee { get; set; }
    //    public decimal ManagerCost { get; set; }
    //    public decimal OtherCost { get; set; }
    //}

    public class ShopExpense : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int OrganizationID { get; set; }
        public int ExpenseKindID { get; set; }
        public decimal Expense { get; set; }
        private DateTime _occurDate = DateTime.Now;
        public DateTime OccurDate
        {
            get { return _occurDate; }
            set { _occurDate = value; }
        }
        public string Remark { get; set; }
    }
}
