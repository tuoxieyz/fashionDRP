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
using Telerik.Windows.Controls;

namespace SysProcessView
{
    /// <summary>
    /// MultiLowerOrganizationSelectWin.xaml 的交互逻辑
    /// </summary>
    public partial class MultiLowerOrganizationSelectWin : Window
    {
        public event Action<IEnumerable<SysOrganization>> SetCompleted;
        OrganizationSelectVM _dataContext = null;

        public MultiLowerOrganizationSelectWin(bool filterCurrent = true, IEnumerable<int> selectedIDs = null)
        {
            _dataContext = new OrganizationSelectVM(filterCurrent);
            this.DataContext = _dataContext;
            InitializeComponent();
            if (selectedIDs != null)
            {
                //不放到Loaded中就不行(默认项未选中)，只能说坑爹
                this.Loaded += delegate
                {
                    _dataContext.SelectedIDs = selectedIDs;
                    RadGridView1_Filtered(null, null);
                };
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (SetCompleted != null)
            {
                SetCompleted(_dataContext.Entities.Where(o => o.IsSelectedNew));
            }
            this.Close();
        }

        private void cbToggleAll_Click(object sender, RoutedEventArgs e)
        {
            if (_dataContext.Entities != null && _dataContext.Entities.Count() > 0)
            {
                CheckBox cb = sender as CheckBox;
                bool check = cb.IsChecked.Value;
                foreach (var item in RadGridView1.Items)
                {
                    ((OrganizationForSelect)item).IsSelectedNew = check;
                }
            }
        }

        private void RadGridView1_Filtered(object sender, Telerik.Windows.Controls.GridView.GridViewFilteredEventArgs e)
        {
            cbToggleAll.IsChecked = true;
            foreach (var item in RadGridView1.Items)
            {
                if (!((OrganizationForSelect)item).IsSelectedNew)
                {
                    cbToggleAll.IsChecked = false;
                    return;
                }
            }
        }

        private void checkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CheckBox cb = e.OriginalSource as CheckBox;
            if (cb != null)
            {
                if (cb.IsChecked.Value)
                {
                    RadGridView1_Filtered(null, null);
                }
                else
                    cbToggleAll.IsChecked = false;
            }
        }
    }
}
