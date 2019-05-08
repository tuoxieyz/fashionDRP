using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using SysProcessModel;
using Kernel;

namespace SysProcessViewModel
{
    public class ProStylePictureBookVM : CommonViewModel<StylePictureAlbum>
    {
        public int BrandID { get; set; }

        public ProStylePictureBookVM()
        {
            if (VMGlobal.PoweredBrands.Count == 1)
            {
                BrandID = VMGlobal.PoweredBrands[0].ID;
                OnPropertyChanged("BrandID");
            }
        }

        protected override IEnumerable<StylePictureAlbum> SearchData()
        {
            List<ProBYQ> byqs = VMGlobal.BYQs.ToList();
            if (BrandID != default(int))
                byqs = byqs.FindAll(o => o.BrandID == BrandID);
            var byqIDs = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle, int>(o => o.BYQID).Distinct().ToList();
            byqs = byqs.FindAll(o => byqIDs.Contains(o.ID));
            var result = byqs.Select(o => new StylePictureAlbum
            {
                ID = o.ID,
                BrandID = o.BrandID,
                BrandName = VMGlobal.PoweredBrands.Find(b => b.ID == o.BrandID).Name,
                Year = o.Year,
                Quarter = o.Quarter,
                QuarterName = VMGlobal.Quarters.Find(q => q.ID == o.Quarter).Name
            });
            return result.OrderBy(o => o.BrandID).ThenByDescending(o => o.Year).ThenByDescending(o => o.Quarter).ToList();
        }

        public static OPResult DeleteMatchingGroup(int groupID)
        {
            try
            {
                VMGlobal.SysProcessQuery.LinqOP.Delete<ProStyleMatching>(o => o.GroupID == groupID);
                return new OPResult { IsSucceed = true, Message = "删除成功." };
            }
            catch (Exception e)
            {
                return new OPResult { IsSucceed = false, Message = "删除失败,失败原因:" + e.Message };
            }
        }

        public static void AddMatchingForAlbum(StylePictureAlbum album, ProStyleMatchingBO matching)
        {
            //同时在同个搭配组中的款色增加该搭配组
            foreach (var m in matching.Matchings)
            {
                var style = album.Styles.FirstOrDefault(o => o.ID == m.StyleID);
                if (style != null)
                {
                    var pic = style.Pictures.FirstOrDefault(o => o.ColorID == m.ColorID);
                    if (pic != null)
                    {
                        var pm = pic.Matchings.FirstOrDefault(o => o.GroupID == matching.GroupID);
                        pic.Matchings.Remove(pm);//若存在先移除
                        pic.Matchings.Insert(0, matching);
                    }
                }
            }
        }

        public static void DeleteMatchingForAlbum(StylePictureAlbum album, ProStyleMatchingBO matching)
        {
            //同时在同个搭配组中的款色删除该搭配组
            foreach (var m in matching.Matchings)
            {
                var style = album.Styles.FirstOrDefault(o => o.ID == m.StyleID);
                if (style != null)
                {
                    var pic = style.Pictures.FirstOrDefault(o => o.ColorID == m.ColorID);
                    if (pic != null)
                    {
                        var pm = pic.Matchings.FirstOrDefault(o => o.GroupID == matching.GroupID);
                        pic.Matchings.Remove(pm);
                    }
                }
            }
        }
    }

    public class ProStyleMatchManageVM : ProStylePictureBookVM
    {
        protected override IEnumerable<StylePictureAlbum> SearchData()
        {
            var result = base.SearchData();
            foreach (var r in result)
                r.IsReadOnly = false;
            return result;
        }
    }
}
