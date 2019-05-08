using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DistributionModel;

namespace DistributionViewModel
{
    public class BillStocktakeBO : BillWithBrandBO
    {
        public int StorageID { get; set; }

        /// <summary>
        /// 盘点单状态:是否已更新库存
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDeleted { get; set; }

        public BillStocktakeBO()
        { }

        public BillStocktakeBO(BillStocktake stocktake)
        {
            this.ID = stocktake.ID;
            this.Code = stocktake.Code;
            this.CreatorID = stocktake.CreatorID;
            this.CreateTime = stocktake.CreateTime;
            this.Remark = stocktake.Remark;
            this.Status = stocktake.Status;
            this.StorageID = stocktake.StorageID;
            this.OrganizationID = stocktake.OrganizationID;
            this.BrandID = stocktake.BrandID;
            this.IsDeleted = stocktake.IsDeleted;
        }

        #region 类型转换操作符重载

        public static implicit operator BillStocktakeBO(BillStocktake stocktake)
        {
            return new BillStocktakeBO(stocktake);
        }

        public static implicit operator BillStocktake(BillStocktakeBO stocktake)
        {
            return new BillStocktake
            {
                ID = stocktake.ID,
                Code = stocktake.Code,
                CreatorID = stocktake.CreatorID,
                CreateTime = stocktake.CreateTime,
                Remark = stocktake.Remark,
                Status = stocktake.Status,
                StorageID = stocktake.StorageID,
                OrganizationID = stocktake.OrganizationID,
                BrandID = stocktake.BrandID,
                IsDeleted = stocktake.IsDeleted
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
                else if (columnName == "BrandID")
                {
                    if (BrandID == default(int))
                        errorInfo = "不能为空";
                }
            }
            return errorInfo;
        }
    }
}
