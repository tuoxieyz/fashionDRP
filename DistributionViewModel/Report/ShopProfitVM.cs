using DistributionModel;
using DomainLogicEncap;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Input;
using Telerik.Windows.Controls;
using ViewModelBasic;

namespace DistributionViewModel
{
    public class ShopProfitVM : CommonViewModel<ShopProfitEntity>
    {
        private FloatPriceHelper _fpHelper = new FloatPriceHelper();

        private ContractDiscountHelper _cdhelper = new ContractDiscountHelper();

        public IEnumerable<ShopExpenseKind> ExpenseKinds { get; set; }

        public IEnumerable<SysOrganization> OrganizationArray
        {
            private get;
            set;
        }

        public DateTime BeginMonth { get; set; }

        public DateTime EndMonth { get; set; }

        private bool _isShowShopHasRetailOnly = true;
        public bool IsShowShopHasRetailOnly
        {
            get { return _isShowShopHasRetailOnly; }
            set { _isShowShopHasRetailOnly = value; }
        }

        public bool IsShowShopHasExpensesOnly { get; set; }

        private DataTable _tableData = null;
        public DataTable TableData
        {
            get { return _tableData; }
            set
            {
                _tableData = value;
                OnPropertyChanged("TableData");
            }
        }

        public ShopProfitVM()
        {
            ExpenseKinds = VMGlobal.DistributionQuery.LinqOP.Search<ShopExpenseKind>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            BeginMonth = EndMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        }

        protected override IEnumerable<ShopProfitEntity> SearchData()
        {
            var oids = OrganizationArray.Select(o => o.ID).ToArray();
            var organizations = OrganizationArray.ToArray();
            int by = BeginMonth.Year, bm = BeginMonth.Month, ey = EndMonth.Year, em = EndMonth.Month;
            var endMonth = EndMonth.AddMonths(1);

            var lp = VMGlobal.DistributionQuery.LinqOP;
            var retailData = lp.Search<BillRetail>(o => o.CreateTime >= BeginMonth && o.CreateTime < endMonth);
            var targets = lp.Search<RetailMonthTaget>(o => oids.Contains(o.OrganizationID) && o.Year >= by && o.Month >= bm && o.Year <= ey && o.Month <= em).ToArray();
            var expenses = lp.Search<ShopExpense>(o => o.OccurDate >= BeginMonth && o.OccurDate < endMonth && oids.Contains(o.OrganizationID)).ToArray();
            if (IsShowShopHasExpensesOnly)
            {
                var sids = expenses.Select(o => o.OrganizationID).Intersect(oids).ToArray();
                retailData = retailData.Where(o => sids.Contains(o.OrganizationID));
                organizations = organizations.Where(o => sids.Contains(o.ID)).ToArray();
            }
            else
                retailData = retailData.Where(o => oids.Contains(o.OrganizationID));
            var retails = retailData.Select(o => new
            {
                OrganizationID = o.OrganizationID,
                RetailID = o.ID,
                QuaRetail = o.Quantity,
                MoneyRetail = o.CostMoney,
                RetailTime = o.CreateTime
            }).ToList();
            if (IsShowShopHasRetailOnly)
            {
                var rids = retails.Select(o => o.OrganizationID);
                expenses = expenses.Where(o => rids.Contains(o.OrganizationID)).ToArray();
                organizations = organizations.Where(o => rids.Contains(o.ID)).ToArray();
            }
            var rbillIDs = retails.Select(o => o.RetailID).ToArray();
            var rDetailData = lp.Search<BillRetailDetails>(o => rbillIDs.Contains(o.BillID));
            var productData = lp.GetDataContext<ViewProduct>();
            var rDetailQuery = from rDetail in rDetailData
                               from product in productData
                               where rDetail.ProductID == product.ProductID
                               select new
                {
                    RetailID = rDetail.BillID,
                    rDetail.Price,
                    rDetail.ProductID,
                    rDetail.Quantity,
                    product.BYQID,
                    product.CostPrice
                };
            var rDetails = rDetailQuery.ToList();

            List<ShopProfitEntity> result = new List<ShopProfitEntity>();
            foreach (var organization in organizations)
            {
                var ryms = retails.Where(o => o.OrganizationID == organization.ID).Select(o => new DateTime(o.RetailTime.Year, o.RetailTime.Month, 1)).Distinct();
                var eyms = expenses.Where(o => o.OrganizationID == organization.ID).Select(o => new DateTime(o.OccurDate.Year, o.OccurDate.Month, 1)).Distinct();
                var yms = ryms.Union(eyms);
                foreach (var ym in yms)
                {
                    ShopProfitEntity entity = new ShopProfitEntity
                    {
                        OrganizationID = organization.ID,
                        OrganizationName = organization.Name,
                        YearMonth = ym.ToString("yyyy-MM")
                    };
                    var target = targets.FirstOrDefault(o => o.OrganizationID == organization.ID && ym.Year == o.Year && ym.Month == o.Month);
                    if (target != null)
                        entity.MonthTarget = target.SaleTaget;
                    var tempRetails = retails.Where(o => o.OrganizationID == organization.ID && ym.Year == o.RetailTime.Year && ym.Month == o.RetailTime.Month);
                    entity.SaleQuantity = tempRetails.Sum(o => o.QuaRetail);
                    entity.SaleMoney = tempRetails.Sum(o => o.MoneyRetail);
                    var tempRDetails = rDetails.Where(o => tempRetails.Select(r => r.RetailID).Contains(o.RetailID));
                    entity.OriginalPrice = tempRDetails.Sum(o => o.Quantity * o.Price);
                    if (VMGlobal.CurrentUser.OrganizationID == 1)
                        entity.CostPrice = tempRDetails.Sum(o => o.Quantity * o.CostPrice);
                    else
                        entity.CostPrice = tempRDetails.Sum(o => o.Quantity * (_fpHelper.GetFloatPrice(OrganizationListVM.CurrentOrganization.ParentID, o.BYQID, o.Price) * _cdhelper.GetDiscount(o.BYQID, VMGlobal.CurrentUser.OrganizationID) * 0.01M));
                    //        var expense = expenses.FirstOrDefault(o => o.OrganizationID == organization.ID && ym.Year == o.Year && ym.Month == o.Month);
                    //        if (expense != null)
                    //        {
                    //            entity.AdvertisingFee = expense.AdvertisingFee;
                    //            entity.BaseCost = expense.BaseCost;
                    //            entity.Bonus = expense.Bonus;
                    //            entity.ElectricCharge = expense.ElectricCharge;
                    //            entity.ExpressCharge = expense.ExpressCharge;
                    //            entity.ManagerCost = expense.ManagerCost;
                    //            entity.OrganizationID = expense.OrganizationID;
                    //            entity.OtherCost = expense.OtherCost;
                    //            entity.RenovationCost = expense.RenovationCost;
                    //            entity.Rent = expense.Rent;
                    //            entity.Salary = expense.Salary;
                    //            entity.SocialSecurityCharge = expense.SocialSecurityCharge;
                    //            entity.TelephoneCharge = expense.TelephoneCharge;
                    //        }

                    result.Add(entity);
                }
            }
            TableData = this.TransferToTable(result, expenses);
            return result;
        }

        private DataTable TransferToTable(IEnumerable<ShopProfitEntity> entities, IEnumerable<ShopExpense> expenses)
        {
            var props = typeof(ShopProfitEntity).GetProperties();
            var dt = new DataTable();
            var expenseKindIDs = expenses.Select(p => p.ExpenseKindID).Distinct();
            IEnumerable<string> expenseKindNames = ExpenseKinds.Where(o => expenseKindIDs.Contains(o.ID)).Select(o => o.Name).ToList();
            foreach (var prop in props)
                dt.Columns.Add(new DataColumn(prop.Name, prop.PropertyType));
            foreach (string kn in expenseKindNames)
                dt.Columns.Add(new DataColumn(kn, typeof(decimal)));
            var func = GetGetDelegate(dt.Columns);
            foreach (var entity in entities)
            {
                var row = dt.Rows.Add(func(entity));
                var tempexpenses = expenses.Where(o => o.OrganizationID == entity.OrganizationID && o.OccurDate.ToString("yyyy-MM") == entity.YearMonth).ToArray();
                foreach (string kn in expenseKindNames)
                {
                    var kid = ExpenseKinds.FirstOrDefault(o => o.Name == kn).ID;
                    var expense = tempexpenses.Where(o => o.ExpenseKindID == kid).Sum(o => o.Expense);
                    row[kn] = expense;
                }
                row["GrossProfit"] = entity.SaleMoney - tempexpenses.Sum(o => o.Expense);
            }
            return dt;
        }

        private Func<ShopProfitEntity, object[]> GetGetDelegate(DataColumnCollection props)
        {
            var param_obj = Expression.Parameter(typeof(ShopProfitEntity), "obj");
            List<Expression> memberExps = new List<Expression>();
            IEnumerable<string> expenseKindNames = ExpenseKinds.Select(o => o.Name).ToList();
            foreach (DataColumn prop in props)
            {
                if (expenseKindNames.Contains(prop.ColumnName))
                {
                    memberExps.Add(Expression.Convert(Expression.Constant(0), typeof(object)));
                }
                else
                    memberExps.Add(Expression.Convert(Expression.Property(param_obj, prop.ColumnName), typeof(object)));
            }
            Expression newArrayExpression = Expression.NewArrayInit(typeof(object), memberExps);
            return Expression.Lambda<Func<ShopProfitEntity, object[]>>(newArrayExpression, param_obj).Compile();
        }
    }

    public class ShopProfitEntity //: ShopExpense
    {
        public int OrganizationID { get; set; }
        public string OrganizationName { get; set; }
        public string YearMonth { get; set; }
        public int MonthTarget { get; set; }
        public int SaleQuantity { get; set; }
        public decimal SaleMoney { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal CostPrice { get; set; }
        public decimal GrossProfit { get; set; }
    }
}
