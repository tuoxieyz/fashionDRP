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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using DistributionViewModel;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using DistributionModel;
using System.IO;
using System.Printing;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using SysProcessViewModel;

namespace DistributionView.Reports
{
    /// <summary>
    /// Interaction logic for BillRetailSearch.xaml
    /// </summary>
    public partial class BillRetailSearch : UserControl
    {
        public BillRetailSearch()
        {
            InitializeComponent();
        }

        private void RadGridView1_RowDetailsVisibilityChanged(object sender, Telerik.Windows.Controls.GridView.GridViewRowDetailsEventArgs e)
        {
            if (e.DetailsElement != null && e.Visibility == Visibility.Visible)
            {
                var item = (RetailSearchEntity)e.Row.Item;
                var gv = (RadGridView)e.DetailsElement;
                gv.ItemsSource = ReportDataContext.GetBillRetailDetails(item.ID);
            }
        }

        private void billFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "StorageID":
                        cbx.ItemsSource = StorageInfoVM.Storages;
                        break;
                    case "ShiftID":
                        cbx.ItemsSource = VMGlobal.DistributionQuery.LinqOP.Search<RetailShift>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnRePrint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image btn = (Image)sender;
            RetailSearchEntity entity = (RetailSearchEntity)btn.DataContext;
            string path = string.Format("{0}\\RetailTicket\\{1}\\{2}.xps", Environment.CurrentDirectory, entity.CreateTime.ToString("yyyyMMdd"), entity.Code);
            if (!File.Exists(path))
            {
                MessageBox.Show("没有找到可供打印的票据.");
                return;
            }
            else
            {
                try
                {
                    //Uri printTemplate = new Uri(path, UriKind.Absolute);
                    XpsDocument printPage = new XpsDocument(path, FileAccess.Read);//(XpsDocument)Application.LoadComponent(printTemplate);
                    PrintQueue defaultPrintQueue = LocalPrintServer.GetDefaultPrintQueue();
                    XpsDocumentWriter xpsdw = PrintQueue.CreateXpsDocumentWriter(defaultPrintQueue);
                    xpsdw.Write(printPage.GetFixedDocumentSequence());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打印失败,失败原因:" + ex.Message);
                }
            }
        }
    }
}
