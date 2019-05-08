using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Telerik.Windows.Controls;
using DomainLogicEncap;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class Locations
    {
        //一定要写成属性形式，WPF界面Binding才能获取到
        public CollectionViewSource Areas { get; private set; }
        private CollectionViewSource _filteredProvinces;
        private CollectionViewSource _filteredCities;

        private SysArea _selectedArea;
        private SysProvience _selectedProvince;

        //public static IEnumerable<SysArea> Areas { get; private set; }
        private static List<SysArea> _areas;
        private static List<SysProvience> _provinces;
        private static List<SysCity> _cities;

        public CollectionViewSource FilteredProvinces { get { return _filteredProvinces; } }
        public CollectionViewSource FilteredCities { get { return _filteredCities; } }

        //public SysArea SelectedArea
        //{
        //    get { return _selectedArea; }
        //    set
        //    {
        //        if (this._selectedArea != value)
        //        {
        //            this._selectedArea = value;
        //            this.OnPropertyChanged("SelectedArea");
        //            this.FilteredProvinces.View.Refresh();
        //            this.FilteredCities.View.Refresh();
        //        }
        //    }
        //}

        //public SysProvience SelectedProvince
        //{
        //    get { return _selectedProvince; }
        //    set
        //    {
        //        if (this._selectedProvince != value)
        //        {
        //            this._selectedProvince = value;
        //            this.OnPropertyChanged("SelectedProvince");
        //            this.FilteredCities.View.Refresh();
        //        }
        //    }
        //}

        static Locations()
        {
            _areas = OrganizationLogic.GetAreas();
            _provinces = OrganizationLogic.GetProvinces();
            _cities = OrganizationLogic.GetCities();
        }

        public Locations()
        {
            Areas = new CollectionViewSource();
            Areas.Source = _areas.Select(a => a);
            Areas.View.CurrentChanged += new EventHandler(AreaView_CurrentChanged);

            _filteredProvinces = new CollectionViewSource();
            _filteredProvinces.Source = _provinces.Select(p => p);
            _filteredProvinces.View.CurrentChanged += new EventHandler(ProView_CurrentChanged);
            _filteredProvinces.Filter += new FilterEventHandler(_filteredProvinces_Filter);

            _filteredCities = new CollectionViewSource();
            _filteredCities.Source = _cities.Select(c => c);
            _filteredCities.Filter += new FilterEventHandler(_filteredCities_Filter);
        }

        void AreaView_CurrentChanged(object sender, EventArgs e)
        {
            CollectionView view = (CollectionView)sender;
            if (view.CurrentItem != null)
            {
                this._selectedArea = (SysArea)view.CurrentItem;
                this.FilteredProvinces.View.Refresh();
                this.FilteredCities.View.Refresh();
            }
            else
                this._selectedArea = null;
        }

        void ProView_CurrentChanged(object sender, EventArgs e)
        {
            CollectionView view = (CollectionView)sender;
            if (view.CurrentItem != null)
            {
                this._selectedProvince = (SysProvience)view.CurrentItem;
                this.FilteredCities.View.Refresh();
            }
            else
                this._selectedProvince = null;
        }

        void _filteredCities_Filter(object sender, FilterEventArgs e)
        {
            SysCity c = e.Item as SysCity;
            if (c != null)
            {
                e.Accepted = (this._selectedProvince != null) && this._selectedProvince.ID == c.ProvienceId;
            }
        }

        void _filteredProvinces_Filter(object sender, FilterEventArgs e)
        {
            SysProvience p = e.Item as SysProvience;
            if (p != null)
            {
                e.Accepted = (this._selectedArea != null) && p.AreaId == this._selectedArea.ID;
            }
        }
    }
}
