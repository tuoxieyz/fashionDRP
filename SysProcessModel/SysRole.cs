using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using System.ComponentModel;
using System.Linq;
using Model.Extension;

namespace SysProcessModel
{
    //SysRole
    public class SysRole : CreatedData, IDNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int OrganizationID { get; set; }
        /// <summary>
        /// 操作权限
        /// </summary>
        public int OPAccess { get; set; }

        /// <summary>
        /// 即时消息接收权限
        /// </summary>
        public int IMAccess { get; set; }

        //private bool _organizationOP = false;

        ///// <summary>
        ///// 组织机构资料操作权限 
        ///// 0:只能查看;1:增删改查
        ///// </summary>
        //public bool OrganizationOP
        //{
        //    get { return _organizationOP; }
        //    set
        //    {
        //        if (_organizationOP != value)
        //        {
        //            _organizationOP = value;
        //            //OnPropertyChanged("OrganizationOP");
        //        }
        //    }
        //}
        ///// <summary>
        ///// 成品资料操作权限
        ///// 0:只能查看;1:增删改查
        ///// </summary>
        //public bool ProductOP { get; set; }
    }
}

