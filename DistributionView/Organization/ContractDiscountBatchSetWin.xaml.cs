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
using DomainLogicEncap;
using DistributionViewModel;
using SysProcessModel;
using DistributionModel;
using SysProcessViewModel;

namespace DistributionView.Organization
{
    /// <summary>
    /// Interaction logic for ContractDiscountBatchSetWin.xaml
    /// </summary>
    public partial class ContractDiscountBatchSetWin : Window
    {
        private List<SysOrganization> _organizations;
        internal event Action SetCompleted;

        public ContractDiscountBatchSetWin()
        {
            InitializeComponent();
            InitContext();
            this.Loaded += delegate
            {
                RadComboBox cb = (RadComboBox)cbxBrand.Content;
                cb.SelectionChanged += new SelectionChangedEventHandler(cbxBrand_SelectionChanged);
            };
        }

        private void InitContext()
        {
            this.DataContext = new BatchContractDiscount();
        }

        void cbxBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxLeft.Items.Clear();
            lbxRight.Items.Clear();
            var cb = sender as RadComboBox;
            if (cb.SelectedIndex != -1)//奇怪之处：就算cb.SelectedIndex == -1,cb.SelectedValue还是等于原选项的值
            {
                _organizations = OrganizationLogic.GetChildOrganizations(VMGlobal.CurrentUser.OrganizationID, (int)cb.SelectedValue, false);
                _organizations.ForEach(o => lbxLeft.Items.Add(o));
            }
        }

        private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                DateTime date = (DateTime)e.AddedItems[0];
                RadDatePicker picker = sender as RadDatePicker;
                picker.DateTimeText = date.Year.ToString();
            }
        }

        private void StackPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount % 2 == 0)
            {
                var sp = (StackPanel)sender;
                var item = sp.DataContext;
                if (lbxLeft.Items.Contains(item))
                {
                    lbxLeft.Items.Remove(item);
                    lbxRight.Items.Add(item);
                }
                else if (lbxRight.Items.Contains(item))
                {
                    lbxRight.Items.Remove(item);
                    lbxLeft.Items.Add(item);
                }
            }
        }

        private void spButtons_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;
            switch (btn.Name)
            {
                case "btnToRight":
                    LeftToRight();
                    break;
                case "btnToLeft":
                    RightToLeft();
                    break;
                case "btnAllToRight":
                    lbxLeft.SelectAll();
                    LeftToRight();
                    break;
                case "btnAllToLeft":
                    lbxRight.SelectAll();
                    RightToLeft();
                    break;
            }
        }

        private void LeftToRight()
        {
            for (int i = 0; i < lbxLeft.SelectedItems.Count; )
            {
                var item = lbxLeft.SelectedItems[i];
                lbxLeft.Items.Remove(item);
                lbxRight.Items.Add(item);
            }
        }

        private void RightToLeft()
        {
            for (int i = 0; i < lbxRight.SelectedItems.Count; )
            {
                var item = lbxRight.SelectedItems[i];
                lbxRight.Items.Remove(item);
                lbxLeft.Items.Add(item);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //if (Validation.GetHasError(this))
            if (!View.Extension.UIHelper.IsValid(this))
                return;
            if (lbxRight.Items.Count == 0)
            {
                MessageBox.Show("未选择机构");
                return;
            }
            BatchContractDiscount bcd = (BatchContractDiscount)this.DataContext;
            var byq = ProductLogic.GetBYQ(bcd.BrandID, bcd.Year, bcd.Quarter);
            if (byq == null)
            {
                MessageBox.Show("未找到相应的品牌年份季度信息");
                return;
            }
            bcd.OrganizationIDs = new List<int>();
            foreach (var item in lbxRight.Items)
            {
                bcd.OrganizationIDs.Add(((SysOrganization)item).ID);
            }
            List<OrganizationContractDiscount> ocds = new List<OrganizationContractDiscount>();
            
            bcd.OrganizationIDs.ForEach(oid => ocds.Add(new OrganizationContractDiscount
            {
                BYQID = byq.ID,
                Discount = bcd.Discount,
                OrganizationID = oid,
                CreatorID = VMGlobal.CurrentUser.ID,
                CreateTime = DateTime.Now
            }));
            try
            {
                OrganizationLogic.BatchSaveContractDiscount(ocds);
                MessageBox.Show("设置成功");
                if (SetCompleted != null)
                    SetCompleted();
                InitContext();
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置失败\n失败原因:" + ex.Message);
            }
        }

        private void radMaskedTextInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_organizations != null)
            {
                string key = radMaskedTextInput.Text;
                List<int> oids = new List<int>();
                foreach (var item in lbxRight.Items)
                {
                    oids.Add(((SysOrganization)item).ID);
                }
                var leftOrgs = _organizations.FindAll(o => !oids.Contains(o.ID) && (o.Code.Contains(key) || o.Name.Contains(key)));
                lbxLeft.Items.Clear();
                leftOrgs.ForEach(o => lbxLeft.Items.Add(o));
            }
        }
    }
}
