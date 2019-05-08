using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;
using System.ComponentModel;
using DBAccess;

namespace DistributionViewModel
{
    public class OrganizationAllocationGradeBO : OrganizationAllocationGrade, IDataErrorInfo
    {
        public string OrganizationCode { get; set; }
        public string OrganizationName { get; set; }
        public string BrandName { get; set; }

        public OrganizationAllocationGradeBO()
        { }

        public OrganizationAllocationGradeBO(OrganizationAllocationGrade grade)
        {
            this.ID = grade.ID;
            this.BrandID = grade.BrandID;
            this.OrganizationID = grade.OrganizationID;
            this.Grade = grade.Grade;
            CreateTime = grade.CreateTime;
            CreatorID = grade.CreatorID;
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
}
