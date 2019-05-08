using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DBLinqProvider.Data.Mapping;
using Model.Extension;

namespace SysProcessModel
{
    public class SysOrganization : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsPrimaryKey = true, IsGenerated = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int ParentID { get; set; }
        public int TypeId { get; set; }
        public int AreaID { get; set; }
        public int ProvienceID { get; set; }
        public int CityID { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string Linkman { get; set; }
        private bool _flag = true;
        public bool Flag { get { return _flag; } set { _flag = value; } }
        public string Description { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }

        public string MapCityName { get; set; }
    }
}
