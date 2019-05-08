using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telerik.Windows.Data;
using ViewModelBasic;
using SysProcessModel;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Configuration;
using Kernel;
using System.Threading.Tasks;

namespace SysProcessViewModel
{
    public static class ProductHelper
    {
        public static IEnumerable<int> GetProductIDArrayWithCondition(IEnumerable<IFilterDescriptor> filterDescriptors, IEnumerable<int> brandIDs = null)
        {
            if (FilterConditionHelper.IsConditionSetted(filterDescriptors, "StyleCode"))
            {
                if (brandIDs == null)
                    brandIDs = VMGlobal.PoweredBrands.Select(b => b.ID);
                var productContext = VMGlobal.SysProcessQuery.LinqOP.GetDataContext<ViewProduct>();
                var pdata = from p in productContext
                            where brandIDs.Contains(p.BrandID)
                            select new { ProductID = p.ProductID, StyleCode = p.StyleCode };
                //注意IQueryable<dynamic>类型的对象调用Where(filters)将产生0<>0的恒假条件
                IEnumerable<int> pIDs = ((IQueryable<dynamic>)pdata.Where(filterDescriptors)).ToList().Select(p => (int)p.ProductID);
                //if (pIDs.Count() == 0)
                //    return null;
                return pIDs;
            }
            return null;
        }

        public static ImageSource GetProductImage(ProSCPicture pic, bool isThumbnail = true)
        {
            if (pic == null)
                return GenerateNullImage();
            var style = VMGlobal.SysProcessQuery.LinqOP.Search<ProStyle>(o => o.ID == pic.StyleID).FirstOrDefault();
            if (style == null)
                return GenerateNullImage();
            var byq = VMGlobal.BYQs.Find(o => o.ID == style.BYQID);
            if (byq == null)
                return GenerateNullImage();
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            if (!dir.EndsWith("\\"))
                dir += "\\";
            dir += "StylePicture\\" + byq.BrandID.ToString("00") + "\\" + byq.Year + byq.Quarter.ToString("00") + "\\";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = dir + pic.PictureName;
            if (!File.Exists(path) || File.GetLastWriteTime(path) < pic.UploadTime)
            {
                var uri = ConfigurationManager.AppSettings["StylePictureUploadUri"];
                uri += byq.BrandID.ToString("00") + "/" + byq.Year + byq.Quarter.ToString("00") + "/";
                Image image = ImageHandler.DownloadImage(uri + pic.PictureName);
                if (image != null)
                {
                    try
                    {
                        image.Save(path);
                        ImageHandler.ToThumbnail(path, 200, 300);
                    }
                    catch
                    {
                        return GenerateNullImage();
                    }
                    finally
                    {
                        image.Dispose();
                    }
                }
                else
                    return GenerateNullImage();
            }
            if (isThumbnail)
                path = dir + "thumbnail\\" + pic.PictureName;
            return new BitmapImage(new Uri(path));
        }

        public static ImageSource GetProductImage(int styleID, int colorID, bool isThumbnail = true)
        {
            var pic = VMGlobal.SysProcessQuery.LinqOP.Search<ProSCPicture>(o => o.StyleID == styleID && o.ColorID == colorID).FirstOrDefault();
            return GetProductImage(pic, isThumbnail);
        }

        //public static List<ProSCPictureBO> GetProductImages(IEnumerable<string> scnames, bool isThumbnail = false)
        //{
        //    var pics = VMGlobal.SysProcessQuery.LinqOP.Search<ProSCPicture>(o => scnames.Contains(o.SCCode)).ToList();
        //    List<ProSCPictureBO> list = new List<ProSCPictureBO>();
        //    pics.ForEach(pic =>
        //    {
        //        ProSCPictureBO bo = new ProSCPictureBO(pic);
        //        //bo.Picture = GetProductImage(pic, isThumbnail);
        //        list.Add(bo);
        //    });
        //    return list;
        //}

        //public static List<ProSCPictureBO> GetProductImages(int byqID, bool isThumbnail = false)
        //{
        //    var pics = VMGlobal.SysProcessQuery.LinqOP.Search<ProSCPicture>(o => byqID == o.BYQID).ToList();
        //    List<ProSCPictureBO> list = new List<ProSCPictureBO>();
        //    pics.ForEach(pic =>
        //    {
        //        ProSCPictureBO bo = new ProSCPictureBO(pic);
        //        //bo.Picture = GetProductImage(pic, isThumbnail);
        //        list.Add(bo);
        //    });
        //    return list;
        //}

        internal static ImageSource GenerateNullImage()
        {
            //FormattedText text = new FormattedText("无", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
            //    new Typeface("Tahoma"), 16, System.Windows.Media.Brushes.Black);
            //Geometry geometry = text.BuildGeometry(new System.Windows.Point(5, 5));
            DrawingImage image = (DrawingImage)System.Windows.Application.Current.FindResource("NullProductImageSource");
            return image;
        }
    }
}
