using DBEncapsulation;
using Kernel;
using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WEBAPI.Controllers
{
    public class OrganizationController : ApiController
    {
        public IEnumerable<SysOrganization> GetAllShops()
        {
            using (var dbContext = new SysProcessEntities())
            {
                return dbContext.SysOrganization.Where(o => o.Flag && o.Name.EndsWith("店")).OrderBy(o => o.Name).ToArray();
            }
        }

        /// <param name="bname">品牌别名，本系统对应品牌编号</param>
        public IEnumerable<SysOrganization> GetAllShops(string bname)
        {
            using (var dbContext = new SysProcessEntities())
            {
                var query = from brand in dbContext.ProBrand
                            from ob in dbContext.OrganizationBrand
                            where ob.BrandID == brand.ID && brand.Code == bname
                            from organization in dbContext.SysOrganization
                            where ob.OrganizationID == organization.ID && organization.Flag && organization.Name.EndsWith("店")
                            orderby organization.Name
                            select organization;
                return query.ToArray();
            }
        }

        [HttpPut]
        public OPResult SetPosition(int shopid, decimal? lng, decimal? lat)
        {
            using (var dbContext = new SysProcessEntities())
            {
                try
                {
                    var organization = dbContext.SysOrganization.Find(shopid);
                    organization.Longitude = lng;
                    organization.Latitude = lat;
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }

        //public IEnumerable<ProductStock> GetStocks(string styleCode)
        //{
        //    using (var dbContext = new DistributionEntities())
        //    {
        //        var query = from stock in dbContext.Stock
        //                    from product in dbContext.ViewProduct
        //                    where stock.ProductID == product.ProductID && product.StyleCode == styleCode
        //                    from storage in dbContext.Storage
        //                    where stock.StorageID == storage.ID
        //                    from organization in dbContext.ViewOrganization
        //                    where organization.ID == storage.OrganizationID
        //                    from color in dbContext.ViewProColor
        //                    where color.ID == product.ColorID
        //                    from size in dbContext.ViewProSize
        //                    where size.ID == product.SizeID
        //                    select new ProductStock
        //                    {
        //                        ColorName = color.Name,
        //                        OrganizationID = organization.ID,
        //                        Quantity = stock.Quantity,
        //                        SizeID = size.ID,
        //                        SizeName = size.Name
        //                    };
        //        var data = query.GroupBy(o => new { o.OrganizationID, o.ColorName, o.SizeName, o.SizeID }).Select(
        //            g => new ProductStock
        //            {
        //                ColorName = g.Key.ColorName,
        //                OrganizationID = g.Key.OrganizationID,
        //                Quantity = g.Sum(o => o.Quantity),
        //                SizeID = g.Key.SizeID,
        //                SizeName = g.Key.SizeName
        //            }).ToList();

        //        data.RemoveAll(o => o.Quantity <= 0);
        //        data.ForEach(o => o.StyleCode = styleCode);
        //        return data.OrderBy(o => o.OrganizationID).ThenBy(o => o.ColorName).ThenBy(o => o.SizeID).ToArray();
        //    }
        //}
    }
}
