using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBAccess;
using DistributionModel;
using Model.Extension;

namespace DomainLogicEncap
{
    public static class CommonDataLogic
    {
        private static QueryGlobal _distributionQuery { get; set; }

        static CommonDataLogic()
        {
            _distributionQuery = new QueryGlobal("DistributionConstr");
        }

        public static string GenerateCodeForBusiDataDictionary(string parentCode, int extenLength = 2)
        {
            int len = parentCode.Length + extenLength;
            string maxCode = _distributionQuery.LinqOP.Search<BusiDataDictionary>(o => o.ParentCode == parentCode && o.Code.Length == len).Max(o => o.Code);
            int newCode = 1;
            if (!string.IsNullOrEmpty(maxCode))
                newCode = Convert.ToInt32(maxCode.Substring(parentCode.Length, extenLength)) + 1;
            return parentCode + newCode.ToString().PadLeft(extenLength, '0');
        }
    }
}
