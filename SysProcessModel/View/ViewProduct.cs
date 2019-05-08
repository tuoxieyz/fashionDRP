//*********************************************
// 公司名称：
// 部    门：研发中心
// 创 建 者：徐时进
// 创建日期：2012-5-29 14:40:44
// 修 改 人：
// 修改日期：
// 修改说明：
// 文件说明：
//**********************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DBLinqProvider.Data.Mapping;

namespace SysProcessModel
{
    public class ViewProduct
    {
        [ColumnAttribute(IsPrimaryKey = true)]
        public int ProductID { get; set; }
        public string ProductCode { get; set; }
        public string StyleCode { get; set; }
        public int BrandID { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal Price { get; set; }
        public decimal CostPrice { get; set; }
        public int NameID { get; set; }
        public int BYQID { get; set; }
        public int StyleID { get; set; }
        public int ColorID { get; set; }
        //新加
        public int SizeID { get; set; }
    }
}
