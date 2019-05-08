using DBLinqProvider.Data.Mapping;
using Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionModel
{
    public class ShopExpenseKind : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public int OrganizationID { get; set; }

        private bool _isEnabled = true;
        public bool IsEnabled
        {
            get { return _isEnabled; }
            set { _isEnabled = value; }
        }
    }
}
