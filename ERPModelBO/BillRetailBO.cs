using DistributionModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ERPModelBO
{
    public class BillRetailBO : BillBO<BillRetail, BillRetailDetails>
    {
        private bool _specifcCreateTime = false;
        /// <summary>
        /// 是否已经指定了制单日期，若否则指定为服务端日期
        /// </summary>
        public bool SpecifcCreateTime { get { return _specifcCreateTime; } set { _specifcCreateTime = value; } }

        public VIPPointTrack VIPPointRecord { get; set; }

        public VIPBirthdayConsumption VIPBirthdayConsumption { get; set; }

        public IEnumerable<VIPUpTacticBO> RefrenseVIPUpTactics { get; set; }

        public List<BillBO<BillStoreOut, BillStoreOutDetails>> BillStoreOuts { get; set; }

        public List<BillBO<BillStoring, BillStoringDetails>> BillStorings { get; set; }
    }
}
