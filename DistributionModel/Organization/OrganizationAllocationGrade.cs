using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    /// <summary>
    /// 机构分货等级
    /// </summary>
    public class OrganizationAllocationGrade : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public int OrganizationID { get; set; }
        public int BrandID { get; set; }
        public int Grade { get; set; }
    }
}
