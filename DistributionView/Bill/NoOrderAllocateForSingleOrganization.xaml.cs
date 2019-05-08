using DistributionViewModel;
using SysProcessModel;
using SysProcessView;
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
using Telerik.Windows.Controls;

namespace DistributionView.Bill
{
    /// <summary>
    /// NoOrderAllocateForSingleOrganization.xaml 的交互逻辑
    /// </summary>
    public partial class NoOrderAllocateForSingleOrganization : UserControl
    {
        NoOrderAllocateForSingleOrganizationVM _dataContext = new NoOrderAllocateForSingleOrganizationVM();

        public NoOrderAllocateForSingleOrganization()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void btnSelectStyles_Click(object sender, RoutedEventArgs e)
        {
            StyleSelectWin win = new StyleSelectWin();
            win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
            win.SetCompleted += delegate(IEnumerable<ProStyle> styles)
            {
                _dataContext.Styles = styles;
                if (styles == null || styles.Count() == 0)
                {
                    tbStyles.Clear();
                }
                else
                {
                    string info = "";
                    foreach (var o in _dataContext.Styles)
                    {
                        info += o.Code + ",";
                    }
                    tbStyles.Text = info.TrimEnd(',');
                }
                win.Close();
            };
            win.ShowDialog();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            var result = _dataContext.CheckCondition();
            if (!result.IsSucceed)
            {
                MessageBox.Show(result.Message);
                e.Handled = true;
            }
            else
            {
                _dataContext.SearchCommand.Execute(null);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var result = _dataContext.Save();
            MessageBox.Show(result.Message);
        }
    }
}
