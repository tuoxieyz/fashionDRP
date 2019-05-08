using DistributionModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class ShopExpenseKindBO : ShopExpenseKind, IDataErrorInfo
    {
        private DataChecker _checker;

        public ShopExpenseKindBO()
        { }

        public ShopExpenseKindBO(ShopExpenseKind kind)
        {
            this.ID = kind.ID;
            this.Name = kind.Name;
            this.IsEnabled = kind.IsEnabled;
            this.OrganizationID = kind.OrganizationID;
            CreateTime = kind.CreateTime;
            CreatorID = kind.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Name")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.DistributionQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataName<ShopExpenseKind>(this);
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
