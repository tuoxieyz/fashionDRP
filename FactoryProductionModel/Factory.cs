using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace ManufacturingModel
{
    /// <summary>
    /// 生产工厂/车间
    /// </summary>
    public class Factory : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LinkMan { get; set; }
        public string LinkPhone { get; set; }
        public string Remark { get; set; }

        private bool _isEnabled = true;
        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }

        /// <summary>
        /// 是否外部工厂
        /// </summary>
        public virtual bool IsOuter { get; set; }
    }
}
