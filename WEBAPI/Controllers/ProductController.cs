using DBEncapsulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WEBAPI.Controllers
{
    public class ProductController : ApiController
    {
        public ProductColorSize GetStyleColorSize(string bname, string pcode)
        {
            using (var dbContext = new SysProcessEntities())
            {
                var query = from product in dbContext.Product
                            from style in dbContext.ProStyle
                            where product.StyleID == style.ID && style.Code == pcode
                            from color in dbContext.ProColor
                            where product.ColorID == color.ID
                            from size in dbContext.ProSize
                            where product.SizeID == size.ID
                            select new
                            {
                                ColorName = color.Name,
                                SizeName = size.Name,
                                SizeID = size.ID
                            };
                var data = query.ToList();
                var pcs = new ProductColorSize();
                pcs.ColorNames = data.Select(o => o.ColorName).Distinct();
                pcs.SizeNames = data.OrderBy(o => o.SizeID).Select(o => o.SizeName).Distinct();
                return pcs;
            }
        }

        public Product_Stock_ColorSize GetStocks(string bname, string pcode)
        {
            using (var dbContext = new DistributionEntities())
            {
                var query = from stock in dbContext.Stock
                            from product in dbContext.ViewProduct
                            where stock.ProductID == product.ProductID && product.StyleCode == pcode && stock.Quantity > 0
                            from storage in dbContext.Storage
                            where stock.StorageID == storage.ID
                            from organization in dbContext.ViewOrganization
                            where organization.ID == storage.OrganizationID && organization.Name.EndsWith("店")
                            from color in dbContext.ViewProColor
                            where color.ID == product.ColorID
                            from size in dbContext.ViewProSize
                            where size.ID == product.SizeID
                            select new
                            {
                                StyleCode = product.StyleCode,
                                ProductCode = product.ProductCode,
                                ColorName = color.Name,
                                ShopID = organization.ID,
                                Quantity = stock.Quantity,
                                SizeID = size.ID,
                                SizeName = size.Name
                            };
                var data = query.GroupBy(o => new { o.ShopID, o.StyleCode, o.ProductCode, o.ColorName, o.SizeName, o.SizeID }).Select(
                    g => new
                    {
                        ColorName = g.Key.ColorName,
                        ShopID = g.Key.ShopID,
                        Quantity = g.Sum(o => o.Quantity),
                        SizeID = g.Key.SizeID,
                        SizeName = g.Key.SizeName,
                        StyleCode = g.Key.StyleCode,
                        ProductCode = g.Key.ProductCode
                    }).ToList();

                data = data.OrderBy(o => o.ShopID).ThenBy(o => o.ColorName).ThenBy(o => o.SizeID).ToList();

                var pcs = new ProductColorSize();
                pcs.ColorNames = data.Select(o => o.ColorName).Distinct();
                pcs.SizeNames = data.OrderBy(o => o.SizeID).Select(o => o.SizeName).Distinct();

                var stocks = new List<ProductStock>();
                foreach (var d in data)
                {
                    var stock = stocks.FirstOrDefault(r => r.ShopID == d.ShopID && r.StyleCode == d.StyleCode);
                    if (stock == null)
                    {
                        stock = new ProductStock { StyleCode = d.StyleCode, ShopID = d.ShopID };
                        stocks.Add(stock);
                    }
                    if (stock.ColorQuas == null)
                        stock.ColorQuas = new List<ColorQuantity>();
                    var cqua = stock.ColorQuas.FirstOrDefault(c => c.ColorName == d.ColorName);
                    if (cqua == null)
                    {
                        cqua = new ColorQuantity { ColorName = d.ColorName };
                        stock.ColorQuas.Add(cqua);
                    }
                    if (cqua.SizeQuas == null)
                        cqua.SizeQuas = new List<SizeQuantity>();
                    var squa = cqua.SizeQuas.FirstOrDefault(s => s.SizeName == d.SizeName);
                    if (squa == null)//当为null时添加，否则不做处理
                    {
                        squa = new SizeQuantity { SizeName = d.SizeName, Quantity = d.Quantity };
                        cqua.SizeQuas.Add(squa);
                    }
                }
                return new Product_Stock_ColorSize { ColorSize = pcs, ShopStocks = stocks };
            }
        }
    }
}
