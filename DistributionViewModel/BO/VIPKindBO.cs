using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using Kernel;
using DBAccess;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    public class VIPKindBO : VIPKind, IDataErrorInfo
    {
        private LinqOPEncap _linqOP = VMGlobal.DistributionQuery.LinqOP;

        public VIPKindBO()
        { }

        public VIPKindBO(VIPKind kind)
        {
            this.ID = kind.ID;
            this.BrandID = kind.BrandID;
            Name = kind.Name;
            Description = kind.Description;
            Discount = kind.Discount;
            CreateTime = kind.CreateTime;
            CreatorID = kind.CreatorID;
        }

        private string CheckData(string columnName)
        {
            string errorInfo = null;//base.CheckData(columnName);
            if (string.IsNullOrEmpty(errorInfo))
            {
                if (columnName == "BrandID")
                {
                    if (BrandID == default(int))
                        errorInfo = "不能为空";
                    else
                        errorInfo = CheckBrandAndName();
                }
            }
            else if (columnName == "Name")
            {
                if (Name.IsNullEmpty())
                    errorInfo = "不能为空";
                else
                    errorInfo = CheckBrandAndName();
            }
            return errorInfo;
        }

        private string CheckBrandAndName()
        {
            if (ID == 0)//新增
            {
                if (_linqOP.Any<VIPKind>(e => e.BrandID == BrandID && e.Name == Name))
                    return "该名称已经被该品牌使用";
            }
            else//编辑
            {
                if (_linqOP.Any<VIPKind>(e => e.ID != ID && e.BrandID == BrandID && e.Name == Name))
                    return "该名称已经被该品牌使用";
            }
            return null;
        }

        string IDataErrorInfo.Error
        {
            get { return null; }
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
