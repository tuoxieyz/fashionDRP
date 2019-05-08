using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class BillStocktakeContrastBO : BillWithBrandBO
    {
        public int StorageID { get; set; }

        public BillStocktakeContrastBO()
        { }

        public BillStocktakeContrastBO(BillStocktakeContrast contrast)
        {
            this.ID = contrast.ID;
            this.Code = contrast.Code;
            this.CreatorID = contrast.CreatorID;
            this.CreateTime = contrast.CreateTime;
            this.Remark = contrast.Remark;
            this.BrandID = contrast.BrandID;
            this.StorageID = contrast.StorageID;
            this.OrganizationID = contrast.OrganizationID;
        }

        #region 类型转换操作符重载

        public static implicit operator BillStocktakeContrastBO(BillStocktakeContrast contrast)
        {
            return new BillStocktakeContrastBO(contrast);
        }

        public static implicit operator BillStocktakeContrast(BillStocktakeContrastBO contrast)
        {
            return new BillStocktakeContrast
            {
                ID = contrast.ID,
                Code = contrast.Code,
                CreatorID = contrast.CreatorID,
                CreateTime = contrast.CreateTime,
                Remark = contrast.Remark,
                BrandID = contrast.BrandID,
                StorageID = contrast.StorageID,
                OrganizationID = contrast.OrganizationID
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
            }
            return errorInfo;
        }
    }
}
