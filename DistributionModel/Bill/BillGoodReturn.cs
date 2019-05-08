using System;
using System.Text;
using System.Collections.Generic;
using System.Data;
//using System.ComponentModel;
namespace DistributionModel
{
    /// <summary>
    /// 退货单
    /// </summary>
    public class BillGoodReturn : BillWithBrand, IStorageID//, INotifyPropertyChanged
    {
        public int StorageID { get; set; }
        /// <summary>
        /// 0:在途中 1:已入库 2:退回 3:退回已入库 4:未审核
        /// </summary>
        public int Status { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }

        public bool IsDefective { get; set; }

        //public override int OrganizationID
        //{
        //    get
        //    {
        //        return base.OrganizationID;
        //    }
        //    set
        //    {
        //        base.OrganizationID = value;
        //        if (PropertyChanged != null)
        //        {
        //            PropertyChanged(this, new PropertyChangedEventArgs("OrganizationID"));
        //        }
        //    }
        //}

        //public event PropertyChangedEventHandler PropertyChanged;
    }
}

