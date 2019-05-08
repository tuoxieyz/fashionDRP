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
using Telerik.Windows.Controls.Data.DataForm;
using DistributionViewModel;
using DistributionModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Data;
using Telerik.Windows.Controls;
using ERPViewModelBasic;
using ViewModelBasic;
using SysProcessViewModel;
using SysProcessView;
using Telerik.Windows.Controls.GridView;

namespace DistributionView.VIP
{
    /// <summary>
    /// Interaction logic for VIPInformationSet.xaml
    /// </summary>
    public partial class VIPInformationSet : UserControl
    {
        DataFormCommandButtonsVisibility _access;
        VIPCardVM _dataContext = new VIPCardVM();

        private IEnumerable<IDNameImplementEntity> _kindListFilter = null;

        private IEnumerable<IDNameImplementEntity> KindListFilter
        {
            get
            {
                if (_kindListFilter == null)
                {
                    var brandIDs = VMGlobal.PoweredBrands.Select(o => o.ID);
                    var kinds = VMGlobal.DistributionQuery.LinqOP.Search<VIPKind>(condition: o => brandIDs.Contains(o.BrandID)).ToList();
                    kinds = kinds.OrderBy(o => o.BrandID).ToList();
                    _kindListFilter = kinds.Select(o => new IDNameImplementEntity
                    {
                        ID = o.ID,
                        Name = (VMGlobal.PoweredBrands.Find(b => b.ID == o.BrandID).Name + o.Name)
                    });
                }
                return _kindListFilter;
            }
        }

        public VIPInformationSet()
        {
            this.DataContext = _dataContext;
            InitializeComponent();

            _access = RoleVM.GetCurrentUserDataFormCommand(BasicInfoEnum.VIP信息);
            myRadDataForm.CommandButtonsVisibility = _access;
        }

        //这方法在改变窗体大小等时候会频繁调用，更损耗性能
        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);
        //    btnSearch_Click(null, null);
        //}

        private void myRadDataForm_EditEnding(object sender, EditEndingEventArgs e)
        {
            if (myRadDataForm.CanCommitEdit && e.EditAction == EditAction.Commit)
            {
                if (myRadDataForm.Mode == RadDataFormMode.AddNew || myRadDataForm.Mode == RadDataFormMode.Edit)
                {
                    var icCardKinds = View.Extension.UIHelper.GetDataFormField<ItemsControl>(myRadDataForm, "icCardKinds");
                    List<VIPCardKindEntity> cks = (List<VIPCardKindEntity>)icCardKinds.ItemsSource;
                    VIPCardBO card = (VIPCardBO)myRadDataForm.CurrentItem;
                    card.Kinds = cks.Where(o => o.KindID != default(int)).Select(o => new VIPKind { ID = o.KindID }).ToList();
                    SysProcessView.UIHelper.AddOrUpdateRecord<VIPCard>(myRadDataForm, _dataContext, e);
                    if (!e.Cancel)
                    {
                        card.Kinds = null;//重新从数据库里获取
                        RadGridView1.Rebind();
                    }
                }
            }
        }

        private void myRadDataForm_DeletingItem(object sender, System.ComponentModel.CancelEventArgs e)
        {
            View.Extension.UIHelper.DeleteRecord<VIPCard>(myRadDataForm, _dataContext, e);
        }

        private void myRadDataForm_AddedNewItem(object sender, AddedNewItemEventArgs e)
        {
            VIPCard card = (VIPCard)myRadDataForm.CurrentItem;
            card.OrganizationID = VMGlobal.CurrentUser.OrganizationID;
        }

        private void radDataFilter_EditorCreated(object sender, Telerik.Windows.Controls.Data.DataFilter.EditorCreatedEventArgs e)
        {
            RadComboBox cbx = e.Editor as RadComboBox;
            if (cbx != null)
            {
                switch (e.ItemPropertyDefinition.PropertyName)
                {
                    case "KindID":
                        cbx.ItemsSource = KindListFilter;
                        break;
                }
            }
            SysProcessView.UIHelper.ToggleShowEqualFilterOperatorOnly(e.Editor);
        }

        private void btnSetPoint_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            VIPCardBO card = (VIPCardBO)btn.DataContext;
            if (card.ID != default(int))
            {
                if (!_dataContext.DownHierarchyOrganizationIDArray.Contains(card.OrganizationID))
                {
                    MessageBox.Show("只能为本级或下级机构创建的VIP设置积分.");
                    return;
                }
                VIPPointSetWin win = new VIPPointSetWin(card);
                win.DataContext = new VIPPointTrack { VIPID = card.ID };
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先填写VIP资料并保存.");
            }
        }

        private void myRadDataForm_BeginningEdit(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VIPCard card = (VIPCard)myRadDataForm.CurrentItem;
            if (!_dataContext.DownHierarchyOrganizationIDArray.Contains(card.OrganizationID))
            {
                MessageBox.Show("只能编辑本级或下级机构创建的VIP信息.");
                e.Cancel = true;
            }
        }

        private void btnPredeposit_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            VIPCardBO card = (VIPCardBO)btn.DataContext;
            if (card.ID != default(int))
            {
                //if (!_dataContext.DownHierarchyOrganizationIDArray.Contains(card.OrganizationID))
                //{
                //    MessageBox.Show("只能为本级或下级机构创建的VIP预存现金.");
                //    return;
                //}
                if (string.IsNullOrEmpty(card.PrestorePassword))
                {
                    MessageBox.Show("请先设置预存密码.");
                    return;
                }
                VIPPredepositSetWin win = new VIPPredepositSetWin(card);
                win.DataContext = new VIPPredepositTrack { VIPID = card.ID };
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先填写VIP资料并保存.");
            }
        }

        private void btnSetPassword_Click(object sender, RoutedEventArgs e)
        {
            RadButton btn = sender as RadButton;
            var row = View.Extension.UIHelper.GetAncestor<GridViewRow>(btn);
            row.IsSelected = true;
            VIPCardBO card = (VIPCardBO)btn.DataContext;
            if (card.ID != default(int))
            {
                PrestorePasswordSetWin win = new PrestorePasswordSetWin(card);
                win.Owner = View.Extension.UIHelper.GetAncestor<Window>(this);
                win.ShowDialog();
            }
            else
            {
                MessageBox.Show("请先填写VIP资料并保存.");
            }
        }

        //private void myRadDataForm_ValidatingItem(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    if (myRadDataForm.ValidationSummary != null)
        //        myRadDataForm.ValidationSummary.Errors.Clear();
        //}

        //private void myRadDataForm_CurrentItemChanged(object sender, EventArgs e)
        //{
        //    if (myRadDataForm.ValidationSummary != null)
        //        myRadDataForm.ValidationSummary.Errors.Clear();
        //}
    }
}
