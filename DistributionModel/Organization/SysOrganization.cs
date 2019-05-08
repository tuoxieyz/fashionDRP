using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Kernel;
using DBAccess;
using DBLinqProvider.Data.Mapping;

namespace ModelEntity.Organization
{
    public class SysOrganization : IDCodeNameEntity
    {
        private bool _flag = true;

        public int ParentID { get; set; }
        public int TypeId { get; set; }
        public int AreaID { get; set; }
        public int ProvienceID { get; set; }
        public int CityID { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Linkman { get; set; }
        public bool Flag { get { return _flag; } set { _flag = value; } }
        public string Description { get; set; }

        public SysOrganization()
        {
            Query = new QueryGlobal("SysProcessConstr");
        }

        protected override string CheckData(string columnName)
        {
            string errorInfo = base.CheckData(columnName);

            if (errorInfo == null)
            {
                if (columnName == "Telephone")
                {
                    if (!Telephone.IsNullEmpty() && !(Telephone.IsTelepone() || Telephone.IsMobile()))
                        errorInfo = "格式不正确";
                }
                else if (columnName == "TypeId")
                {
                    if (TypeId == default(int))
                        errorInfo = "不能为空";
                }
                else if (columnName == "AreaID")
                {
                    if (AreaID == default(int))
                        errorInfo = "不能为空";
                }
                else if (columnName == "ProvienceID")
                {
                    if (ProvienceID == default(int))
                        errorInfo = "不能为空";
                }
                else if (columnName == "CityID")
                {
                    if (CityID == default(int))
                        errorInfo = "不能为空";
                }
            }
            return errorInfo;
        }
    }
}
