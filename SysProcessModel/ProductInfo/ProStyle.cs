using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
using DBLinqProvider.Data.Mapping;
using System.Linq;
using Model.Extension;

namespace SysProcessModel
{
    //ProStyle
    public class ProStyle : CreatedData, IDEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public virtual string Code { get; set; }
        public virtual int NameID { get; set; }
        //public int BrandID { get; set; }
        //public string Year { get; set; }
        //public int Quarter { get; set; }
        public int BYQID { get; set; }
        public virtual int UnitID { get; set; }
        public virtual int BoduanID { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal CostPrice { get; set; }

        //private string _pictureUrl = "";
        //public virtual string PictureUrl
        //{
        //    get { return _pictureUrl; }
        //    set
        //    {
        //        if (_pictureUrl != value)
        //        {
        //            _pictureUrl = value;
        //        }
        //    }
        //}

        private bool _flag = true;
        public bool Flag { get { return _flag; } set { _flag = value; } }

        /// <summary>
        /// 标准码
        /// </summary>
        public string EANCode { get; set; }

        //public virtual string Description { get; set; }

        //public override bool Equals(object obj)
        //{
        //    ProStyle style = obj as ProStyle;
        //    if (style == null)
        //        return false;
        //    else
        //    {
        //        if(this.ID == style.ID && this.NameID == style.NameID && this.Price == style.Price && this.BYQID == style.BYQID && this.Code == style.Code
        //            && this.UnitID == style.UnitID && this.BoduanID == style.BoduanID)
        //    }
        //}
    }
}

