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
using SysProcessViewModel;
using DistributionViewModel;
using Telerik.Windows.Controls.Data.DataForm;
using DistributionModel;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.Organization
{
    /// <summary>
    /// Interaction logic for OrganizationGoodReturnRateSet.xaml
    /// </summary>
    public partial class OrganizationGoodReturnRateSet : UserControl
    {
        OrganizationGoodReturnRateVM _dataContext = new OrganizationGoodReturnRateVM();

        public OrganizationGoodReturnRateSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            if (e.ItemPropertyDefinition.PropertyName == "BrandID")
            {
                RadComboBox cbxBrand = (RadComboBox)e.Editor;
                cbxBrand.ItemsSource = VMGlobal.PoweredBrands;
            }
        }

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {            
            SysProcessView.UIHelper.AddOrUpdateRecord<OrganizationGoodReturnRate>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var dr = MessageBox.Show("删除基准退货率将同时删除相应的年份季度退货率,确定删除吗?", "注意", MessageBoxButton.YesNo);
            if (dr == MessageBoxResult.Yes)
            {
                View.Extension.UIHelper.DeleteRecord<OrganizationGoodReturnRate>(myRadDataForm, _dataContext, e);
            }
            else
                e.Cancel = true;
        }

        private void details_EditEnding(object sender, EditEndingEventArgs e)
        {
            RadDataForm form = (RadDataForm)sender;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(form);
            OrganizationGoodReturnRate rate = row.DataContext as OrganizationGoodReturnRate;

            if (rate.ID == default(int))
            {
                MessageBox.Show("请先保存机构品牌基本退货率");
                e.Cancel = true;
                return;
            }

            if (form.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                OrganizationGoodReturnRatePerQuarter entity = (OrganizationGoodReturnRatePerQuarter)form.CurrentItem;
                if (entity.ID == default(int))
                {
                    entity.RateID = rate.ID;
                    entity.CreatorID = VMGlobal.CurrentUser.ID;
                    entity.CreateTime = DateTime.Now;
                }
                var result = _dataContext.AddOrUpdate(entity);
                MessageBox.Show(result.Message);
                if (!result.IsSucceed)
                {
                    e.Cancel = true;
                }
            }
        }

        private void details_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            RadDataForm form = (RadDataForm)sender;
            OrganizationGoodReturnRatePerQuarter entity = (OrganizationGoodReturnRatePerQuarter)form.CurrentItem;
            var result = _dataContext.Delete(entity);
            MessageBox.Show(result.Message);
            if (!result.IsSucceed)
            {
                e.Cancel = true;
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
    }
}
