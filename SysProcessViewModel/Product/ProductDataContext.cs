using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Telerik.Windows.Data;
using DBAccess;
using Telerik.Windows.Controls;
using System.Linq.Expressions;
using System.Collections;
using System.Transactions;
using Kernel;
using System.ComponentModel;
using ViewModelBasic;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class ProductDataContext : ViewModelBase
    {
        private static QueryGlobal _query = VMGlobal.SysProcessQuery;

        private List<ProBoduan> _boduans;

        private List<ProName> _pronames;

        private List<ProSize> _sizes;

        private List<ProUnit> _units;

        private List<ProColor> _colors;

        public QueryGlobal Query
        {
            get { return _query; }
        }

        /// <summary>
        /// 品牌(登录用户拥有的品牌权限和用户机构拥有的品牌权限的交集)
        /// </summary>
        public List<ProBrand> Brands
        {
            get
            {
                return VMGlobal.PoweredBrands;
            }
        }

        //品名
        public List<ProName> ProNames
        {
            get
            {
                if (_pronames == null)
                {
                    _pronames = _query.LinqOP.Search<ProName>().ToList();
                }
                return _pronames;
            }
        }

        //波段
        public List<ProBoduan> Boduans
        {
            get
            {
                if (_boduans == null)
                {
                    _boduans = _query.LinqOP.Search<ProBoduan>().ToList();
                }
                return _boduans;
            }
        }

        //尺码
        public List<ProSize> Sizes
        {
            get
            {
                if (_sizes == null)
                {
                    _sizes = _query.LinqOP.Search<ProSize>().ToList();
                }
                return _sizes;
            }
        }

        //单位
        public List<ProUnit> Units
        {
            get
            {
                if (_units == null)
                {
                    _units = _query.LinqOP.Search<ProUnit>().ToList();
                }
                return _units;
            }
        }

        public List<ProQuarter> Quarters
        {
            get
            {
                return VMGlobal.Quarters;
            }
        }

        #region Color

        public List<ProColor> Colors
        {
            get
            {
                if (_colors == null)
                {
                    _colors = _query.LinqOP.Search<ProColor>().ToList();
                }
                return _colors;
            }
        }

        /// <summary>
        /// 根据款号ID获得款式对应的颜色集合
        /// </summary>
        //public IEnumerable<ProColor> GetColorsOfStyle(int id)
        //{
        //    //_query.GeneralOper.Search<Product>(p=>p.StyleID==id)
        //    var products = _query.QueryProvider.GetTable<Product>("Product");
        //    var colors = _query.QueryProvider.GetTable<ProColor>("ProColor");
        //    var query = from p in products
        //                from c in colors
        //                where p.ColorID == c.ID && p.StyleID == id
        //                select c;
        //    return query.Distinct().ToList();
        //}

        #endregion

        #region 成品资料

        //public IList Products { get; set; }

        //public ICollectionView Products { get; set; }

        //public void GetProducts(CompositeFilterDescriptorCollection condition, int pageIndex, int pageSize, ref int totalCount)
        //{
        //    #region 由于dynamic不能从UI界面传值更新，并且丧失了实体类内置的验证功能，因此下列代码删除
        //    //var products = _query.QueryProvider.GetTable<Product>("Product");
        //    //var styles = _query.QueryProvider.GetTable<ProStyle>("ProStyle");
        //    //var pInfos = from p in products
        //    //             from st in styles
        //    //             where p.StyleID == st.ID
        //    //             select new
        //    //             {
        //    //                 Code = st.Code,
        //    //                 BrandID = st.BrandID,
        //    //                 SizeID = p.SizeID,
        //    //                 NameID = st.NameID,
        //    //                 BoduanID = st.BoduanID,
        //    //                 Year = st.Year,
        //    //                 Quarter = st.Quarter,
        //    //                 Price = st.Price
        //    //             };
        //    //var filteredProducts = pInfos.Where(condition).ToIList();
        //    //List<dynamic> transProducts = new List<dynamic>();
        //    //for (int i = 0, len = filteredProducts.Count; i < len; i++)
        //    //{
        //    //    var fp = filteredProducts[i] as dynamic;
        //    //    transProducts.Add(new
        //    //    {
        //    //        Code = fp.Code,
        //    //        BrandName = Brands.Find(bd => bd.ID == fp.BrandID).Name,
        //    //        BoduanName = Boduans.Find(bd => bd.ID == fp.BoduanID).Name,
        //    //        Name = ProNames.Find(pn => pn.ID == fp.NameID).Name,
        //    //        Price = fp.Price,
        //    //        YearQuarter = fp.Year + Quarters.Find(q => q.ID == fp.Quarter).Name
        //    //    });
        //    //}
        //    //Products = transProducts;
        //    #endregion

        //    #region 以下为改写代码
        //    var styles = _query.LinqOP.GetDataContext<ProStyle>();
        //    var brandIDs = this.Brands.Select(o => o.ID);
        //    var byqs = _query.QueryProvider.GetTable<ProBYQ>("ProBYQ");
        //    var result = from style in styles
        //                 from byq in byqs
        //                 where style.BYQID == byq.ID && brandIDs.Contains(byq.BrandID)
        //                 select new ExtProStyle
        //                 {
        //                     BoduanID = style.BoduanID,
        //                     BrandID = byq.BrandID,
        //                     BYQID = byq.ID,
        //                     Code = style.Code,
        //                     CreateTime = style.CreateTime,
        //                     CreatorID = style.CreatorID,
        //                     Flag = style.Flag,
        //                     ID = style.ID,
        //                     NameID = style.NameID,
        //                     PictureUrl = style.PictureUrl,
        //                     Price = style.Price,
        //                     Quarter = byq.Quarter,
        //                     UnitID = style.UnitID,
        //                     Year = byq.Year
        //                 };
        //    var temp = (IQueryable<ExtProStyle>)result.Where(condition);
        //    totalCount = temp.Count();
        //    Products = new QueryableCollectionView(temp.OrderBy(o => o.ID).Skip(pageIndex * pageSize).Take(pageSize).ToList());//看项目手记105条
        //    #endregion

        //    OnPropertyChanged("Products");
        //}

        //public ExtProStyle GetProduct(int pid)
        //{
        //    var styles = _query.QueryProvider.GetTable<ProStyle>("ProStyle");
        //    var byqs = _query.QueryProvider.GetTable<ProBYQ>("ProBYQ");
        //    var result = from style in styles
        //                 from byq in byqs
        //                 where style.BYQID == byq.ID && style.ID == pid
        //                 select new ExtProStyle
        //                 {
        //                     BoduanID = style.BoduanID,
        //                     BrandID = byq.BrandID,
        //                     BYQID = byq.ID,
        //                     Code = style.Code,
        //                     CreateTime = style.CreateTime,
        //                     CreatorID = style.CreatorID,
        //                     Flag = style.Flag,
        //                     ID = style.ID,
        //                     NameID = style.NameID,
        //                     PictureUrl = style.PictureUrl,
        //                     Price = style.Price,
        //                     Quarter = byq.Quarter,
        //                     UnitID = style.UnitID,
        //                     Year = byq.Year
        //                 };
        //    return result.FirstOrDefault();
        //}

        ///// <summary>
        ///// 根据款号ID获得款式对应的尺码集合
        ///// </summary>
        //public IEnumerable<ProSize> GetSizesOfStyle(int id)
        //{
        //    var products = _query.QueryProvider.GetTable<Product>("Product");
        //    var sizes = _query.QueryProvider.GetTable<ProSize>("ProSize");
        //    var query = from p in products
        //                from s in sizes
        //                where p.SizeID == s.ID && p.StyleID == id
        //                select s;
        //    return query.Distinct().ToList();
        //}

        #endregion

        //public static List<ProStyle> GetProStyleList(int brandID, string year, int quarter)
        //{
        //    return _query.LinqOP.Search<ProStyle>(o => o.Year == year && o.Quarter == quarter && o.BrandID == brandID).ToList();
        //}

        //public static List<ProSCPicture> GetProStyleColorListForPictureAlbum(int byqID)
        //{
        //    //var products = _query.QueryProvider.GetTable<Product>("Product");
        //    //var styles = _query.QueryProvider.GetTable<ProStyle>("ProStyle");
        //    //var colors = _query.QueryProvider.GetTable<ProColor>("ProColor");
        //    //var scps = _query.QueryProvider.GetTable<ProSCPicture>("ProSCPicture");
        //    ////var byqs = _query.LinqOP.GetDataContext<ProBYQ>();
        //    //var query = from s in styles
        //    //            from p in products
        //    //            where p.StyleID == s.ID && s.BYQID == byqID
        //    //            from c in colors
        //    //            where p.ColorID == c.ID
        //    //            select new { StyleColor = s.Code + c.Code };
        //    //var result = from q in query
        //    //             from scp in scps
        //    //             where q.StyleColor == scp.SCCode
        //    //             select scp;
        //    return _query.LinqOP.Search<ProSCPicture>(o => o.BYQID == byqID).ToList();
        //    //return result.Distinct().ToList();//会生成Distinct的sql语句，我记得之前有地方不会生成，不知为何
        //}

        public static OPResult SaveSCPicture(ProSCPicture scp)
        {
            scp.UploadTime = DateTime.Now;
            lock (_query)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        _query.LinqOP.Delete<ProSCPicture>(o => o.StyleID == scp.StyleID && o.ColorID == scp.ColorID);
                        _query.LinqOP.Add<ProSCPicture>(scp);
                        scope.Complete();
                        return new OPResult { IsSucceed = true, Message = "保存成功!" };
                    }
                    catch (Exception e)
                    {
                        return new OPResult { IsSucceed = false, Message = "保存失败:" + e.Message };
                    }
                }
            }
        }
    }
}
