using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.IO;
using System.Windows.Markup;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Xaml;
using Telerik.Windows.Documents.FormatProviders;

namespace View.Extension
{
    public class RadRichTextBoxHelper
    {
        public static string GetDocumentXaml(DependencyObject obj)
        {
            return (string)obj.GetValue(DocumentXamlProperty);
        }
        public static void SetDocumentXaml(DependencyObject obj, string value)
        {
            obj.SetValue(DocumentXamlProperty, value);
        }
        public static readonly DependencyProperty DocumentXamlProperty =
          DependencyProperty.RegisterAttached(
            "DocumentXaml",
            typeof(string),
            typeof(RadRichTextBoxHelper),
            new FrameworkPropertyMetadata
            {
                PropertyChangedCallback = (obj, e) =>
                {
                    var richTextBox = (RichTextBox)obj;

                    // Parse the XAML to a document (or use XamlReader.Parse())
                    var xaml = GetDocumentXaml(richTextBox);
                    if (xaml != null)
                    {
                        //var doc = new FlowDocument();
                        //var range = new TextRange(doc.ContentStart, doc.ContentEnd);
                        //range.Load(new MemoryStream(Encoding.UTF8.GetBytes(xaml)),DataFormats.Xaml);
                        var stream = new MemoryStream(Encoding.Unicode.GetBytes(GetDocumentXaml(richTextBox)));
                        var doc = (FlowDocument)XamlReader.Load(stream);
                        // Set the document
                        richTextBox.Document = doc;
                    }
                    //var richTextBox = (RadRichTextBox)obj;
                    //var byteXaml = GetDocumentXaml(richTextBox).ToArray();
                    //if (byteXaml != null)
                    //{
                    //    IDocumentFormatProvider formator = new XamlFormatProvider();
                    //    richTextBox.Document = formator.Import(byteXaml);
                    //}
                }
            });

    }
}
