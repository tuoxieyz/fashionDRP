using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VersionManager.BO;
using System.IO;
using Telerik.Windows.Documents.Model;
using System.Windows.Markup;
using Telerik.Windows.Documents.FormatProviders;
using Telerik.Windows.Documents.FormatProviders.Xaml;

namespace VersionManager
{
    /// <summary>
    /// Interaction logic for SoftVersionCUWin.xaml
    /// </summary>
    public partial class SoftVersionCUWin : Window
    {
        public SoftVersionCUWin()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var version = this.DataContext as SoftVersionTrackBO;
            var customers = lbxCustomer.ItemsSource as List<CustomerBO>;
            version.Customers = customers.Where(o => o.IsHold).ToList();

            RadDocument document = descriptionEditor.Document;
            //MemoryStream s = new MemoryStream();
            //XamlWriter.Save(document, s);
            //version.Description = s.ToArray();
            IDocumentFormatProvider formator = new XamlFormatProvider();
            version.Description = formator.Export(document);

            var result = version.Soft.AddOrUpdate(version);
            MessageBox.Show(result.Message);
            if (result.IsSucceed)
                this.Close();
        }
    }
}
