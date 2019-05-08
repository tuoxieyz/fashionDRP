using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using ERPViewModelBasic;
using System.ComponentModel;
using Kernel;
using SysProcessViewModel;

namespace DistributionViewModel
{
    //public class OrganizationHierarchyEntityWithIDName : CreatedData, IDataErrorInfo
    //{
    //    public int ID { get; set; }
    //    public string Name { get; set; }
    //    public int OrganizationID { get; set; }

    //    IQueryable<OrganizationHierarchyEntityWithIDName> _entities;

    //    private IQueryable<OrganizationHierarchyEntityWithIDName> Entities
    //    {
    //        get
    //        {
    //            if (_entities == null)
    //            {
    //                Type type = this.GetType(); ;//获取当前运行时类型
    //                _entities = VMGlobal.DistributionQuery.QueryProvider.GetTable(type, type.Name) as IQueryable<OrganizationHierarchyEntityWithIDName>;
    //            }
    //            return _entities;
    //        }
    //    }

    //    string IDataErrorInfo.Error
    //    {
    //        get { return ""; }
    //    }

    //    string IDataErrorInfo.this[string columnName]
    //    {
    //        get
    //        {
    //            return this.CheckData(columnName);
    //        }
    //    }

    //    protected virtual string CheckData(string columnName)
    //    {
    //        string errorInfo = null;

    //        if (columnName == "Name")
    //        {
    //            if (Name.IsNullEmpty())
    //                errorInfo = "不能为空";
    //            else if (ID == 0)//新增
    //            {
    //                if (Entities.Any(e => e.OrganizationID == OrganizationID && e.Name == Name))
    //                    errorInfo = "该名称已经被使用";
    //            }
    //            else//编辑
    //            {
    //                if (Entities.Any(e => e.OrganizationID == OrganizationID && e.ID != ID && e.Name == Name))
    //                    errorInfo = "该名称已经被使用";
    //            }
    //        }

    //        return errorInfo;
    //    }
    //}
}
