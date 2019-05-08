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
using SysProcessViewModel;
using Telerik.Windows.Data;
using System.Collections.ObjectModel;
using Telerik.Windows.Controls.Data.DataFilter;
using Telerik.Windows.Controls;
using Telerik.Windows;
using Telerik.Windows.Controls.GridView;
using DomainLogicEncap;
using SysProcessModel;
using View.Extension;

namespace SysProcessView
{
    /// <summary>
    /// Interaction logic for OrganizationSelectorWin.xaml
    /// </summary>
    public partial class OrganizationSelectorWin : Window
    {
        static List<SysOrganizationType> _types = null;
        static List<SysArea> _areas = null;
        static List<SysProvience> _proviences = null;

        public event Action<SysOrganization> OrganizationSelected;

        static OrganizationSelectorWin()
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            _types = lp.Search<SysOrganizationType>().ToList();
            _areas = lp.Search<SysArea>().ToList();
            _proviences = lp.Search<SysProvience>().ToList();
        }

        public OrganizationSelectorWin(IEnumerable<SysOrganization> datas)
        {
            InitializeComponent();
            RadGridView1.ItemsSource = datas.Select(o => new OrganizationForSelect
            {
                ID = o.ID,
                Code = o.Code,
                Name = o.Name,
                TypeId = o.TypeId,
                AreaID = o.AreaID,
                ParentID = o.ParentID,
                ProvienceID = o.ProvienceID,
                TypeName = _types.Find(t => t.ID == o.TypeId).Name,
                AreaName = _areas.Find(t => t.ID == o.AreaID).Name,
                ProvienceName = _proviences.Find(t => t.ID == o.ProvienceID).Name
            });

            //同行双击
            RadGridView1.RowActivated += new EventHandler<RowEventArgs>(RadGridView1_RowActivated);
            //RadGridView1.AddHandler(GridViewRow.MouseDoubleClickEvent, new MouseButtonEventHandler(Row_MouseDoubleClick));
        }

        void RadGridView1_RowActivated(object sender, RowEventArgs e)
        {
            SysOrganization o = e.Row.Item as SysOrganization;
            if (o != null && OrganizationSelected != null)
            {
                OrganizationSelected(o);
                this.Close();
            }
        }
    }
}
