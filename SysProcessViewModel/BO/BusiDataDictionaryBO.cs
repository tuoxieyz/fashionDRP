using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using System.ComponentModel;
using DistributionModel;
using SysProcessViewModel;

namespace SysProcessViewModel
{
    public class BusiDataDictionaryBO : BusiDataDictionary, IDataErrorInfo
    {
        private DataChecker _checker;

        public BusiDataDictionaryBO()
        { }

        public BusiDataDictionaryBO(BusiDataDictionary dic)
        {
            this.ID = dic.ID;
            this.Name = dic.Name;
            this.Code = dic.Code;
            CreateTime = dic.CreateTime;
            CreatorID = dic.CreatorID;
            IsEnabled = dic.IsEnabled;
            ParentCode = dic.ParentCode;
            this.Value = dic.Value;
        }

        #region 类型转换操作符重载

        //public static implicit operator BusiDataDictionaryBO(BusiDataDictionary dic)
        //{
        //    return new BusiDataDictionaryBO(dic);
        //}

        //public static implicit operator BusiDataDictionary(BusiDataDictionaryBO dic)
        //{
        //    return new BusiDataDictionary
        //    {
        //        ID = dic.ID,
        //        Name = dic.Name,
        //        Code = dic.Code,
        //        CreateTime = dic.CreateTime,
        //        CreatorID = dic.CreatorID,
        //        IsEnabled = dic.IsEnabled,
        //        ParentCode = dic.ParentCode
        //    };
        //}

        #endregion

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

        private string CheckData(string columnName)
        {
            string errorInfo = null;

            if (columnName == "Code" || columnName == "Name")
            {
                if (_checker == null)
                {
                    _checker = new DataChecker(VMGlobal.SysProcessQuery.LinqOP);
                }
                errorInfo = _checker.CheckDataCodeName<BusiDataDictionary>(this, columnName);
            }
            return errorInfo;
        }
    }
}
