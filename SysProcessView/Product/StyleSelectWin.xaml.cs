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
using SysProcessViewModel;
using SysProcessModel;
using System.Globalization;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for StyleSelectWin.xaml
    /// </summary>
    public partial class StyleSelectWin : Window
    {
        public event Action<IEnumerable<ProStyle>> SetCompleted;
        private List<ProStyleBO> _styles;

        public StyleSelectWin()
        {
            InitializeComponent();
            //dpYear.DateTimeText = DateTime.Now.Year.ToString();
            dpYear.NumberFormatInfo = new NumberFormatInfo() { NumberDecimalDigits = 0, NumberGroupSeparator="" };
            dpYear.Value = DateTime.Now.Year;
        }

        public StyleSelectWin(int brandID, bool brandSeletable = false, bool showOnly = false, IEnumerable<int> styleIDsSeleted = null)
            : this()
        {
            cbxBrand.SelectedValue = brandID;
            _styles = this.GetProStyles(brandID);
            foreach (var style in _styles)
            {
                if (!styleIDsSeleted.Contains(style.ID))
                    lbxLeft.Items.Add(style);
            }
            if (!brandSeletable)
            {
                //cbxBrand.IsReadOnly = true;//没用
                cbxBrand.IsEnabled = false;
            }
            if (showOnly)
            {
                btnOK.Visibility = Visibility.Collapsed;
                lbxLeft.IsEnabled = lbxRight.IsEnabled = false;
            }
            if (styleIDsSeleted != null && styleIDsSeleted.Count() > 0)
            {
                var lp = VMGlobal.SysProcessQuery.LinqOP;
                var byqs = lp.Search<ProBYQ>(o => o.BrandID == brandID);
                var styles = lp.Search<ProStyle>(o => styleIDsSeleted.Contains(o.ID));
                var stylesSeleted = GetProStyles(styles, byqs);
                foreach (var style in stylesSeleted)
                    lbxRight.Items.Add(style);
            }
        }

        private void FilterSourceStyles()
        {
            if (cbxBrand.SelectedIndex == -1)
                return;
            //_styles = ProductLogic.GetProStyles((int)cbxBrand.SelectedValue, dpYear.DateTimeText, (int)cbxQuarter.SelectedValue, txtStyleCode.Text.Trim());

            List<int> oids = new List<int>();
            foreach (var item in lbxRight.Items)
            {
                oids.Add(((ProStyleBO)item).ID);
            }
            int year = (int)dpYear.Value;
            int quarter = (cbxQuarter.SelectedValue == null ? 0 : (int)cbxQuarter.SelectedValue);
            string code = txtStyleCode.Text.Trim();
            var leftStyles = _styles.FindAll(o => !oids.Contains(o.ID) && o.Year == year && (quarter == 0 || o.Quarter == quarter) && (string.IsNullOrEmpty(code) || o.Code.Contains(code)));
            lbxLeft.Items.Clear();
            leftStyles.ForEach(o => lbxLeft.Items.Add(o));
        }

        private void cbxBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lbxLeft.Items.Clear();
            lbxRight.Items.Clear();
            var cb = sender as RadComboBox;
            if (cb.SelectedIndex != -1 && cb.SelectedValue != null)//奇怪之处：就算cb.SelectedIndex == -1,cb.SelectedValue还是等于原选项的值
            {
                _styles = this.GetProStyles((int)cb.SelectedValue);
                foreach (var style in _styles)
                {
                    lbxLeft.Items.Add(style);
                }
            }
        }

        //private void RadDatePicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    RadDatePicker picker = sender as RadDatePicker;
        //    if (e.AddedItems.Count > 0)
        //    {
        //        DateTime date = (DateTime)e.AddedItems[0];
        //        picker.DateTimeText = date.Year.ToString();
        //        this.FilterSourceStyles();
        //    }
        //    else if (string.IsNullOrEmpty(picker.DateTimeText))
        //        this.FilterSourceStyles();
        //}

        private void txtStyleCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.FilterSourceStyles();
        }

        private void cbxQuarter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.FilterSourceStyles();
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

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //if (lbxRight.Items.Count == 0)
            //{
            //    MessageBox.Show("未选择款式");
            //    return;
            //}
            if (SetCompleted != null)
            {
                var styles = GetSelectedStyles();
                SetCompleted(styles);
            }
        }

        private IEnumerable<ProStyle> GetSelectedStyles()
        {
            for (int i = 0; i < lbxRight.Items.Count; i++)
            {
                var item = (ProStyleBO)lbxRight.Items[i];
                yield return item;
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

        private List<ProStyleBO> GetProStyles(int brandID)
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            var byqs = lp.Search<ProBYQ>(o => o.BrandID == brandID);
            var styles = lp.Search<ProStyle>();
            return GetProStyles(styles, byqs);
        }

        private List<ProStyleBO> GetProStyles(IQueryable<ProStyle> styles, IQueryable<ProBYQ> byqs)
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            var names = lp.Search<ProName>();
            var data = from style in styles
                       from byq in byqs
                       where byq.ID == style.BYQID
                       from name in names
                       where style.NameID == name.ID
                       select new ProStyleBO
                       {
                           ID = style.ID,
                           BrandID = byq.BrandID,
                           Code = style.Code,
                           NameID = name.ID,
                           Year = byq.Year,
                           Quarter = byq.Quarter, 
                           BYQID=byq.ID
                       };
            return data.OrderByDescending(o => o.ID).ToList();
        }

        private void dpYear_ValueChanged(object sender, RadRangeBaseValueChangedEventArgs e)
        {
            this.FilterSourceStyles();
        }
    }
}
