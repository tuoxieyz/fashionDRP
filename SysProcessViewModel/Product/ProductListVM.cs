using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using Kernel;
using Telerik.Windows.Controls.Data.DataFilter;
using System.Collections.ObjectModel;
using Telerik.Windows.Data;
using System.Transactions;
using SysProcessModel;
using IWCFServiceForIM;
using DomainLogicEncap;

namespace SysProcessViewModel
{
    public class ProductListVM : PagedEditSynchronousVM<ProStyle>
    {
        #region 属性

        IEnumerable<ItemPropertyDefinition> _itemPropertyDefinitions;
        public IEnumerable<ItemPropertyDefinition> ItemPropertyDefinitions
        {
            get
            {
                if (_itemPropertyDefinitions == null)
                {
                    _itemPropertyDefinitions = new List<ItemPropertyDefinition>() 
                    {  
                        new ItemPropertyDefinition { DisplayName = "款号", PropertyName = "Code", PropertyType = typeof(string)}, 
                        new ItemPropertyDefinition { DisplayName = "品牌", PropertyName = "BrandID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "品名", PropertyName = "NameID", PropertyType = typeof(int)},
                        //new ItemPropertyDefinition { DisplayName = "颜色", PropertyName = "ColorID", PropertyType = typeof(string)},
                        //new ItemPropertyDefinition { DisplayName = "尺码", PropertyName = "SizeID", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "年份", PropertyName = "Year", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "季度", PropertyName = "Quarter", PropertyType = typeof(int)},
                        new ItemPropertyDefinition { DisplayName = "波段", PropertyName = "BoduanID", PropertyType = typeof(int)}
                    };
                }
                return _itemPropertyDefinitions;
            }
        }

        CompositeFilterDescriptorCollection _filterDescriptors;
        public override CompositeFilterDescriptorCollection FilterDescriptors
        {
            get
            {
                if (_filterDescriptors == null)
                {
                    _filterDescriptors = new CompositeFilterDescriptorCollection() 
                    {  
                        new FilterDescriptor("Code", FilterOperator.Contains,  FilterDescriptor.UnsetValue, false),
                        new FilterDescriptor("BrandID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false),
                        new FilterDescriptor("NameID", FilterOperator.IsEqualTo, FilterDescriptor.UnsetValue,false)
                    };
                }
                return _filterDescriptors;
            }
        }

        private FloatPriceHelper _fpHelper = new FloatPriceHelper();

        #endregion

        public ProductListVM()
            : base(VMGlobal.SysProcessQuery.LinqOP)
        {
            PageSize = 17;
            Entities = new List<ProStyleBO>();
        }

        protected override IEnumerable<ProStyle> SearchData()
        {
            var styles = LinqOP.GetDataContext<ProStyle>();
            var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
            var byqs = LinqOP.GetDataContext<ProBYQ>();
            var data = from style in styles
                       from byq in byqs
                       where style.BYQID == byq.ID && brandIDs.Contains(byq.BrandID)
                       select new ProStyleBO
                       {
                           BoduanID = style.BoduanID,
                           BrandID = byq.BrandID,
                           BYQID = style.BYQID,
                           Code = style.Code,
                           CreateTime = style.CreateTime,
                           CreatorID = style.CreatorID,
                           Flag = style.Flag,
                           ID = style.ID,
                           NameID = style.NameID,
                           //PictureUrl = style.PictureUrl,
                           Price = style.Price,
                           CostPrice = style.CostPrice,
                           Quarter = byq.Quarter,
                           UnitID = style.UnitID,
                           Year = byq.Year
                       };
            var temp = (IQueryable<ProStyleBO>)data.Where(FilterDescriptors);
            TotalCount = temp.Count();
            var result = temp.OrderBy(o => o.ID).Skip(PageIndex * PageSize).Take(PageSize).ToList();
            foreach (var r in result)
            {
                //r.BoduanName = VMGlobal.Boduans.Find(o => o.ID == r.BoduanID).Name;
                //r.Name = VMGlobal.ProNames.Find(o => o.ID == r.NameID).Name;
                //r.UnitName = VMGlobal.Units.Find(o => o.ID == r.UnitID).Name;
                if (VMGlobal.CurrentUser.OrganizationID != 1)
                {
                    r.CostPrice = _fpHelper.GetFloatPrice(OrganizationListVM.CurrentOrganization.ParentID, r.BYQID, r.Price);
                    r.Price = _fpHelper.GetFloatPrice(VMGlobal.CurrentUser.OrganizationID, r.BYQID, r.Price);
                }
            }
            return result;
        }

        private void SetStyleBYQ(ProStyleBO style)
        {
            var byq = VMGlobal.BYQs.Find(o => o.BrandID == style.BrandID && o.Year == style.Year && o.Quarter == style.Quarter);
            if (byq != null)
                style.BYQID = byq.ID;
            else
            {
                byq = LinqOP.Search<ProBYQ>(o => o.BrandID == style.BrandID && o.Year == style.Year && o.Quarter == style.Quarter).FirstOrDefault();
                if (byq != null)
                {
                    style.BYQID = byq.ID;
                    VMGlobal.BYQs.Add(byq);
                }
                else
                {
                    byq = new ProBYQ { BrandID = style.BrandID, Year = style.Year, Quarter = style.Quarter };
                    int byqID = LinqOP.Add<ProBYQ, int>(byq, o => o.ID);
                    style.BYQID = byqID;
                    byq.ID = byqID;
                    VMGlobal.BYQs.Add(byq);
                }
            }
        }

        private OPResult Add(ProStyleBO style)
        {
            style.CreatorID = VMGlobal.CurrentUser.ID;
            //TransactionScopeOption在事务嵌套时比较有用
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    SetStyleBYQ(style);
                    int id = LinqOP.Add<ProStyle, int>(style, s => s.ID);
                    style.ID = id;//将ID赋值，表明该对象不再是新增对象(新增对象ID都为0)
                    List<Product> products = new List<Product>();
                    foreach (var color in style.Colors)
                    {
                        foreach (var size in style.Sizes)
                        {
                            Product product = new Product
                            {
                                Code = style.Code + color.Code + size.Code,//成品SKU码生成规则=款号+色号+尺码
                                StyleID = style.ID,
                                ColorID = color.ID,
                                SizeID = size.ID,
                                CreatorID = VMGlobal.CurrentUser.ID
                            };
                            products.Add(product);
                        }
                    }
                    LinqOP.Add<Product>(products);
                    scope.Complete();
                    return new OPResult { IsSucceed = true, Message = "保存成功!" };
                }
                catch (Exception e)
                {
                    style.ID = default(int);
                    return new OPResult { IsSucceed = false, Message = "保存失败,失败原因:\n" + e.Message };
                }
            }
        }

        private OPResult Update(ProStyleBO style)
        {
            SetStyleBYQ(style);
            ProStyle orginStyle = LinqOP.GetById<ProStyle>(style.ID);
            List<Product> productsExist = LinqOP.Search<Product>(p => p.StyleID == style.ID).ToList();
            List<Product> products = new List<Product>();
            foreach (var color in style.Colors)
            {
                foreach (var size in style.Sizes)
                {
                    var pcode = style.Code + color.Code + size.Code;
                    Product product = productsExist.FirstOrDefault(p => p.ColorID == color.ID && p.SizeID == size.ID);
                    if (product != null)
                    {
                        if (orginStyle.Code != style.Code)
                        {
                            product.Code = pcode;
                            products.Add(product);
                        }
                    }
                    else
                    {
                        product = new Product
                        {
                            Code = pcode,
                            StyleID = style.ID,
                            ColorID = color.ID,
                            SizeID = size.ID,
                            CreatorID = VMGlobal.CurrentUser.ID
                        };
                        products.Add(product);
                    }
                }
            }

            string changeMsg = "";
            if (orginStyle.Price != style.Price)
                changeMsg += string.Format("单价从{0}变动为{1},", orginStyle.Price, style.Price);
            if (orginStyle.Code != style.Code)
                changeMsg += string.Format("款号从{0}变动为{1},", orginStyle.Code, style.Code);
            if (products.Count > 0)
            {
                changeMsg += "增加了SKU码:";
                products.ForEach(o =>
                {
                    changeMsg += (o.Code + ",");
                });
            }
            changeMsg = changeMsg.TrimEnd(',');

            OPResult result = null;
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    LinqOP.Update<ProStyle>(style);
                    LinqOP.AddOrUpdate<Product>(products);
                    if (!string.IsNullOrEmpty(changeMsg))
                    {
                        ProStyleChange change = new ProStyleChange
                        {
                            CreateTime = DateTime.Now,
                            CreatorID = VMGlobal.CurrentUser.ID,
                            Description = changeMsg,
                            StyleID = style.ID
                        };
                        LinqOP.Add<ProStyleChange>(change);
                        style.Changes.Insert(0, change);
                    }
                    result = new OPResult { IsSucceed = true, Message = "更新成功!" };
                    scope.Complete();
                }
                catch (Exception e)
                {
                    result = new OPResult { IsSucceed = false, Message = "更新失败,失败原因:\n" + e.Message };
                }
            }
            if (result.IsSucceed && !string.IsNullOrEmpty(changeMsg))
            {
                IMHelper.AsyncSendMessageTo(IMHelper.OnlineUsers, new IMessage
                {
                    Message = changeMsg
                }, IMReceiveAccessEnum.成品资料变动);
            }
            return result;
        }

        public override OPResult AddOrUpdate(ProStyle style)
        {
            return style.ID == default(int) ? this.Add((ProStyleBO)style) : this.Update((ProStyleBO)style);
        }

        public override OPResult Delete(ProStyle style)
        {
            throw new NotImplementedException();
        }
    }
}
