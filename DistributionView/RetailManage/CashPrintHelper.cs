using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Win32;
using System.Windows.Xps.Packaging;
using System.IO;
using System.Windows.Xps;
using System.Printing;

namespace DistributionView.RetailManage
{
    internal static class CashPrintHelper
    {
        public static string SaveXPS(FixedPage page, string pageName)
        {
            //FixedDocument fixedDoc = new FixedDocument();//创建一个文档
            //fixedDoc.DocumentPaginator
            //PageContent pageContent = new PageContent();
            //((IAddChild)pageContent).AddChild(page);
            //fixedDoc.Pages.Add(pageContent);//将对象加入到当前文档中
            string path = string.Format("{0}\\RetailTicket\\{1}", Environment.CurrentDirectory, DateTime.Now.ToString("yyyyMMdd"));
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            path += string.Format("\\{0}.xps", pageName);

            XpsDocument xpsDocument = new XpsDocument(path, FileAccess.Write);
            XpsDocumentWriter xpsdw = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
            xpsdw.Write(page);//写入XPS文件                  
            xpsDocument.Close();
            return path;
        }

        //private static string GetXPSFromDialog()
        //{
        //if (isSaved)
        //{
        //    SaveFileDialog saveFileDialog = new SaveFileDialog();

        //    saveFileDialog.Filter = "XPS Document files (*.xps)|*.xps|Txt Document files(*.txt)|*.txt";
        //    saveFileDialog.FilterIndex = 1;

        //    if (saveFileDialog.ShowDialog() == true)
        //    {
        //        return saveFileDialog.FileName;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}
        //else 
        //    return string.Format("{0}\\{1}\\temp.xps", Environment.CurrentDirectory, DateTime.Now.ToString("yyyyMMdd"));
        //}
    }
}
