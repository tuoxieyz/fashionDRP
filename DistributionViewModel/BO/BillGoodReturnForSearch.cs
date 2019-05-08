using DistributionModel;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistributionViewModel
{
    public class BillGoodReturnForSearch : BillWithBrand
    {
        public string BrandName { get; set; }
        public int StorageID { get; set; }
        public string StorageName { get; set; }
        public int Quantity { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreatorName { get; set; }
        public string OrganizationName { get; set; }
        /// <summary>
        /// 单据总价
        /// </summary>
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public string StatusName
        {
            get
            {
                return Enum.GetName(typeof(BillGoodReturnStatusEnum), Status);
            }
        }

        private IEnumerable<DistributionProductShow> _details;
        public IEnumerable<DistributionProductShow> Details
        {
            get
            {
                if (_details == null)
                {
                    var detailsContext = VMGlobal.DistributionQuery.LinqOP.Search<BillGoodReturnDetails>(o => o.BillID == this.ID);
                    var productContext = VMGlobal.DistributionQuery.LinqOP.GetDataContext<ViewProduct>();
                    var data = from details in detailsContext
                               from product in productContext
                               where details.ProductID == product.ProductID
                               select new DistributionProductShow
                               {
                                   ProductID = product.ProductID,                                   
                                   ProductCode = product.ProductCode,
                                   StyleCode = product.StyleCode,
                                   BYQID = product.BYQID,
                                   NameID = product.NameID,
                                   ColorID = product.ColorID,
                                   SizeID = product.SizeID,
                                   Price = details.Price,
                                   Quantity = details.Quantity,
                                   Discount = details.Discount
                               };
                    _details = data.ToList();
                    foreach (var r in _details)
                    {
                        r.ColorCode = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Code;
                        r.ColorName = VMGlobal.Colors.Find(o => o.ID == r.ColorID).Name;
                        r.SizeName = VMGlobal.Sizes.Find(o => o.ID == r.SizeID).Name;
                        r.ProductName = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
                        var byq = VMGlobal.BYQs.Find(o => o.ID == r.BYQID);
                        r.BrandID = byq.BrandID;
                        r.BrandCode = VMGlobal.PoweredBrands.Find(o => o.ID == r.BrandID).Code;
                        r.Year = byq.Year;
                        r.Quarter = byq.Quarter;                        
                    }
                }
                return _details;
            }
        }
    }
}
