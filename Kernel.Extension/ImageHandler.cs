using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Kernel
{
    public static class ImageHandler
    {
        /// <summary>
        /// 生成缩略图
        /// </summary>
        public static void ToThumbnail(string sourceImagePath, int thumbnailImageWidth, int thumbnailImageHeight)
        {
            var sourceImage = Image.FromFile(sourceImagePath);
            // 计算图片的位置、尺寸等信息 
            int tWidth, tHeight, tLeft, tTop;
            double fScale = (double)thumbnailImageHeight / (double)thumbnailImageWidth; // 高度宽度比 
            if (((double)sourceImage.Width * fScale) > (double)sourceImage.Height) // 如果原图比较宽 
            {
                tWidth = thumbnailImageWidth;
                tHeight = (int)((double)sourceImage.Height * (double)tWidth / (double)sourceImage.Width);
                tLeft = 0;
                tTop = (thumbnailImageHeight - tHeight) / 2;
            }
            else
            {
                tHeight = thumbnailImageHeight;
                tWidth = (int)((double)sourceImage.Width * (double)tHeight / (double)sourceImage.Height);
                tLeft = (thumbnailImageWidth - tWidth) / 2;
                tTop = 0;
            }
            if (tLeft < 0) tLeft = 0;
            if (tTop < 0) tTop = 0;
            //用指定的大小和格式初始化 Bitmap 类的新实例  
            Image bitmap = new Bitmap(thumbnailImageWidth, thumbnailImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            //从指定的 Image 对象创建新 Graphics 对象  
            Graphics g = Graphics.FromImage(bitmap);
            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清除整个绘图面并以透明背景色填充 
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制 原图片 对象  
            g.DrawImage(sourceImage, new Rectangle(tLeft, tTop, tWidth, tHeight));
            sourceImage.Dispose();
            var thumbnailPath = Path.GetDirectoryName(sourceImagePath) + "\\thumbnail\\";
            if (!Directory.Exists(thumbnailPath))
                Directory.CreateDirectory(thumbnailPath);
            try
            {
                bitmap.Save(thumbnailPath + Path.GetFileName(sourceImagePath));
            }
            catch (System.Exception e)
            {
                throw e;
            }
            finally
            {
                bitmap.Dispose();
                g.Dispose();
            }
        }

        public static Image DownloadImage(string url)
        {
            Image tmpImage = null;

            try
            {
                // Open a connection  
                System.Net.HttpWebRequest httpWebRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                httpWebRequest.AllowWriteStreamBuffering = true;

                // You can also specify additional header values like the user agent or the referer: (Optional)  
                //_HttpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)";
                //_HttpWebRequest.Referer = "http://www.google.com/";
                // set timeout for 20 seconds (Optional)  
                //_HttpWebRequest.Timeout = 20000;

                // Request response:  
                System.Net.WebResponse webResponse = httpWebRequest.GetResponse();

                // Open data stream:  
                System.IO.Stream webStream = webResponse.GetResponseStream();

                // convert webstream to image  
                tmpImage = Image.FromStream(webStream);

                // Cleanup  
                webResponse.Close();
                webResponse.Close();
            }
            catch (Exception ex)
            {
                // Error  
                //Console.WriteLine("Exception caught in process: {0}", ex.ToString());
                //return null;
            }

            return tmpImage;
        }
    }
}
