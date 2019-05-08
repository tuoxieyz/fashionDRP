using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class BillCannibalizeBO : BillWithBrandBO
    {
        public int StorageID { get; set; }
        public int ToOrganizationID { get; set; }
        public bool Status { get; set; }

        public BillCannibalizeBO()
        { }

        public BillCannibalizeBO(BillCannibalize cannibalize)
        {
            this.ID = cannibalize.ID;
            this.Code = cannibalize.Code;
            this.CreatorID = cannibalize.CreatorID;
            this.CreateTime = cannibalize.CreateTime;
            this.Remark = cannibalize.Remark;
            this.Status = cannibalize.Status;
            this.StorageID = cannibalize.StorageID;
            this.OrganizationID = cannibalize.OrganizationID;
            this.BrandID = cannibalize.BrandID;
            this.ToOrganizationID = cannibalize.ToOrganizationID;
        }

        #region 类型转换操作符重载

        public static implicit operator BillCannibalizeBO(BillCannibalize cannibalize)
        {
            return new BillCannibalizeBO(cannibalize);
        }

        public static implicit operator BillCannibalize(BillCannibalizeBO cannibalize)
        {
            return new BillCannibalize
            {
                ID = cannibalize.ID,
                Code = cannibalize.Code,
                CreatorID = cannibalize.CreatorID,
                CreateTime = cannibalize.CreateTime,
                Remark = cannibalize.Remark, 
                Status = cannibalize.Status,
                StorageID = cannibalize.StorageID,
                OrganizationID = cannibalize.OrganizationID,
                BrandID = cannibalize.BrandID,
                ToOrganizationID = cannibalize.ToOrganizationID
            };
        }

        #endregion

        protected override string CheckData(string columnName)
        {
            string errorInfo = base.CheckData(columnName);
            if (string.IsNullOrEmpty(errorInfo))
            {
                if (columnName == "StorageID")
                {
                    if (StorageID == default(int))
                        errorInfo = "不能为空";
                }
                else if (columnName == "ToOrganizationID")
                {
                    if (ToOrganizationID == default(int))
                        errorInfo = "不能为空";
                }
            }
            return errorInfo;
        }
    }
}
