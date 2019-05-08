using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBAccess;

namespace SysProcessViewModel
{
    public class DataChecker
    {
        private LinqOPEncap _linqOP;

        public DataChecker(LinqOPEncap linqOP)
        {
            _linqOP = linqOP;
        }

        public string CheckDataName<TData>(TData data) where TData : IDNameEntity
        {
            string errorInfo = null;

            if (string.IsNullOrWhiteSpace(data.Name))
                errorInfo = "不能为空";
            else if (data.ID == 0)//新增
            {
                if (_linqOP.Any<TData>(e => e.Name == data.Name))
                    errorInfo = "该名称已经被使用";
            }
            else//编辑
            {
                if (_linqOP.Any<TData>(e => e.ID != data.ID && e.Name == data.Name))
                    errorInfo = "该名称已经被使用";
            }

            return errorInfo;
        }

        public string CheckDataCodeName<TData>(TData data,string propertyName) where TData : IDCodeNameEntity
        {
            string errorInfo = null;

            if (propertyName == "Name")
            {
                if (string.IsNullOrWhiteSpace(data.Name))
                    errorInfo = "不能为空";
                else if (data.ID == 0)//新增
                {
                    if (_linqOP.Any<TData>(e => e.Name == data.Name))
                        errorInfo = "该名称已经被使用";
                }
                else//编辑
                {
                    if (_linqOP.Any<TData>(e => e.ID != data.ID && e.Name == data.Name))
                        errorInfo = "该名称已经被使用";
                }
            }
            else if (propertyName == "Code")
            {
                if (string.IsNullOrWhiteSpace(data.Code))
                    errorInfo = "不能为空";
                else if (data.ID == 0)//新增
                {
                    if (_linqOP.Any<TData>(e => e.Code == data.Code))
                        errorInfo = "该编号已经被使用";
                }
                else//编辑
                {
                    //下句方式，并不能在编码时就确定运行时类型，并且没有方法替换类型参数，因此泛型的作用是编码时“多态”，而遇到需要运行时真正的多态情况仍需要借助其它方式
                    //if (Query.GeneralOper.Search<ProBrand>(brand => brand.ID != ID && brand.Code == Code).Count() > 0)
                    if (_linqOP.Any<TData>(e => e.ID != data.ID && e.Code == data.Code))
                        errorInfo = "该编号已经被使用";
                }
            }

            return errorInfo;
        }
    }
}
