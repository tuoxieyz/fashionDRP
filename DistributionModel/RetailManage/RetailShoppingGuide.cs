using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Model.Extension;
using DBLinqProvider.Data.Mapping;

namespace DistributionModel
{
    /// <summary>
    /// 零售导购
    /// </summary>
    public class RetailShoppingGuide : CreatedData, IDCodeNameEntity
    {
        [ColumnAttribute(IsGenerated = true, IsPrimaryKey = true)]
        public int ID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int OrganizationID { get; set; }

        /// <summary>
        /// 所属班次
        /// </summary>
        //public int? ShiftID { get; set; }//若字段类型为int?，则貌似有bug，更新后值能保存到数据库，但界面上会显示为空
        public int ShiftID { get; set; }

        private DateTime _onBoardDate = DateTime.Now;
        /// <summary>
        /// 入职时间
        /// </summary>
        public virtual DateTime OnBoardDate
        {
            get { return _onBoardDate; }
            set
            {
                if (_onBoardDate != value)
                {
                    _onBoardDate = value;
                }
            }
        }

        private DateTime? _dimissionDate = null;
        /// <summary>
        /// 离职时间
        /// </summary>
        public virtual DateTime? DimissionDate { get { return _dimissionDate; }
            set {
                if (_dimissionDate != value)
                {
                    _dimissionDate = value;
                }
            }
        }

        private bool _state = true;
        public bool State
        {
            get { return _state; }
            set { _state = value; }
        }
    }
}
