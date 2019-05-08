using Model.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionModel
{
    public class VIPPredepositTrack : CreatedData
    {
        public int VIPID { get; set; }
        public decimal StoreMoney { get; set; }
        public decimal FreeMoney { get; set; }
        public decimal ConsumeMoney { get; set; }        
        public int OrganizationID { get; set; }
        /// <summary>
        /// true:预存 false:消费
        /// </summary>
        public bool Kind { get; set; }
        public string RefrenceBillCode { get; set; }
        public string Remark { get; set; }
    }
}