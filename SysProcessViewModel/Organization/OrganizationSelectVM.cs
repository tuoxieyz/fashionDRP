using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using SysProcessModel;
using System.ComponentModel;
using System.Data;

namespace SysProcessViewModel
{
    public class OrganizationSelectVM : CommonViewModel<OrganizationForSelect>
    {
        private IEnumerable<OrganizationForSelect> _currentDirLowerOrganizations;

        private IEnumerable<OrganizationForSelect> _allCurrentLowerOrganizations;

        private bool _showAllLower;

        public bool ShowAllLower
        {
            get { return _showAllLower; }
            set
            {
                if (_showAllLower != value)
                {
                    _showAllLower = value;
                    Entities = this.SearchData();
                    //OnPropertyChanged("Entities");
                    OnPropertyChanged("ShowAllLower");
                }
            }
        }

        private bool _canUserToggleShowAllLower;

        public bool CanUserToggleShowAllLower
        {
            get { return _canUserToggleShowAllLower; }
            set
            {
                if (_canUserToggleShowAllLower != value)
                {
                    _canUserToggleShowAllLower = value;
                    OnPropertyChanged("CanUserToggleShowAllLower");
                }
            }
        }

        public IEnumerable<SysOrganization> DefaultOrSelectedOrganizations
        {
            get
            {
                if (Entities == null)
                {
                    Entities = this.SearchData();
                }
                var temp = ShowAllLower ? _allCurrentLowerOrganizations : _currentDirLowerOrganizations;
                var selectedOrganizations = temp.Where(o => o.SelectState == SelectStateEnum.Selected || o.SelectState == SelectStateEnum.SelfSelected);
                if (selectedOrganizations.Count() == 0)
                    return _filterCurrent ? temp.Where(o => o.ID != VMGlobal.CurrentUser.OrganizationID) : temp;
                return selectedOrganizations;
            }
        }

        protected bool _filterCurrent = true;

        private bool _isShowShopOnly = false;

        public OrganizationSelectVM(bool filterCurrent, bool showAllLower, bool canUserToggleShowAllLower, bool isShowShopOnly)
        {
            _filterCurrent = filterCurrent;
            _showAllLower = showAllLower;
            _isShowShopOnly = isShowShopOnly;
            CanUserToggleShowAllLower = canUserToggleShowAllLower;
            this.Entities = this.SearchData();
        }

        protected override IEnumerable<OrganizationForSelect> SearchData()
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            if (_currentDirLowerOrganizations == null)
            {
                IEnumerable<SysOrganizationBO> organizations = OrganizationListVM.CurrentAndChildrenOrganizations;
                if (_isShowShopOnly)
                    organizations = organizations.Where(o => o.Name.EndsWith("店"));
                //var types = lp.GetDataContext<SysOrganizationType>();
                //var areas = lp.GetDataContext<SysArea>();
                //var proviences = lp.GetDataContext<SysProvience>();
                //var data = from organization in organizations//iqtoolkit中,数据表关联内存中的数组对象,会N次连接数据库
                //           from type in types
                //           where organization.TypeId == type.ID
                //           from area in areas
                //           where organization.AreaID == area.ID
                //           from provience in proviences
                //           where organization.ProvienceID == provience.ID
                //           select new OrganizationForSelect
                //           {
                //               ID = organization.ID,
                //               Name = organization.Name,
                //               Code = organization.Code,
                //               TypeName = type.Name,
                //               AreaName = area.Name,
                //               ProvienceName = provience.Name
                //           };
                var ts = organizations.Select(o => o.TypeId).ToArray();
                var rs = organizations.Select(o => o.AreaID).ToArray();
                var ps = organizations.Select(o => o.ProvienceID).ToArray();
                var types = lp.Search<SysOrganizationType>(t => ts.Contains(t.ID)).ToList();
                var areas = lp.Search<SysArea>(t => rs.Contains(t.ID)).ToList();
                var proviences = lp.Search<SysProvience>(t => ps.Contains(t.ID)).ToList();
                var data = from organization in organizations
                           from type in types
                           where organization.TypeId == type.ID
                           from area in areas
                           where organization.AreaID == area.ID
                           from provience in proviences
                           where organization.ProvienceID == provience.ID
                           select new OrganizationForSelect
                           {
                               ID = organization.ID,
                               Name = organization.Name,
                               Code = organization.Code,
                               TypeName = type.Name,
                               AreaName = area.Name,
                               ProvienceName = provience.Name
                           };
                _currentDirLowerOrganizations = data.ToList();
            }
            var result = _filterCurrent ? _currentDirLowerOrganizations.Where(o => o.ID != VMGlobal.CurrentUser.OrganizationID) : _currentDirLowerOrganizations;
            if (!ShowAllLower)
            {
                foreach (var organization in result)
                {
                    organization.ChildrenOrganizations = null;
                    //if (organization.SelectState == SelectStateEnum.SelfUnSelected)
                    //    organization.SelectState = SelectStateEnum.UnSelected;
                    //else if (organization.SelectState == SelectStateEnum.SelfSelected)
                    //    organization.SelectState = SelectStateEnum.Selected;
                }
            }
            else
            {
                if (_allCurrentLowerOrganizations == null)
                {
                    var ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetOrganizationDownHierarchy", VMGlobal.CurrentUser.OrganizationID);
                    var table = ds.Tables[0];
                    if (table.Rows.Count > 0)
                    {
                        List<int> oids = new List<int>();
                        foreach (DataRow row in table.Rows)
                        {
                            oids.Add((int)row["OrganizationID"]);
                        }
                        oids.RemoveAll(o => result.Any(r => r.ID == o));
                        var oidArray = oids.ToArray();
                        var organizations = lp.Search<SysOrganization>(o => oidArray.Contains(o.ID));
                        var types = lp.GetDataContext<SysOrganizationType>();
                        var areas = lp.GetDataContext<SysArea>();
                        var proviences = lp.GetDataContext<SysProvience>();
                        var query = from organization in organizations
                                    from type in types
                                    where organization.TypeId == type.ID
                                    from area in areas
                                    where organization.AreaID == area.ID
                                    from provience in proviences
                                    where organization.ProvienceID == provience.ID
                                    select new OrganizationForSelect
                                    {
                                        ID = organization.ID,
                                        Name = organization.Name,
                                        Code = organization.Code,
                                        TypeName = type.Name,
                                        AreaName = area.Name,
                                        ProvienceName = provience.Name,
                                        ParentID = organization.ParentID
                                    };
                        var data = query.ToList();
                        data.AddRange(result);
                        _allCurrentLowerOrganizations = data;
                    }
                    else
                        _allCurrentLowerOrganizations = result;

                }
                foreach (var organization in result)
                {
                    if (organization.ID != VMGlobal.CurrentUser.OrganizationID)
                        ConstructOrganizations(organization);
                }
            }
            return result.OrderBy(o => o.Code);
        }

        private void ConstructOrganizations(OrganizationForSelect parent)
        {
            var children = _allCurrentLowerOrganizations.Where(o => o.ParentID == parent.ID);
            if (children.Count() > 0)
            {
                foreach (var child in children)
                {
                    ConstructOrganizations(child);
                    //if (parent.SelectState == SelectStateEnum.UnSelected && child.SelectState != SelectStateEnum.UnSelected)
                    //    parent.SelectState = SelectStateEnum.SelfUnSelected;//表明有子级被选中
                }
            }
            parent.ChildrenOrganizations = children;
        }

        public void ClearSelected()
        {
            if (this.Entities != null)
            {
                //foreach (var organization in Entities)
                //{
                //    organization.SelectState = SelectStateEnum.UnSelected;
                //}
                var temp = ShowAllLower ? _allCurrentLowerOrganizations : _currentDirLowerOrganizations;
                foreach (var organization in Entities)
                {
                    organization.SelectState = SelectStateEnum.UnSelected;
                }
            }
        }

        public OrganizationForSelect GetRootOrganization(OrganizationForSelect organization)
        {
            if (this.Entities == null || organization == null)
                return null;
            if (this.Entities.Any(o => o.ID == organization.ID))
                return organization;
            var temp = ShowAllLower ? _allCurrentLowerOrganizations : _currentDirLowerOrganizations;
            var parent = temp.FirstOrDefault(o => o.ID == organization.ParentID);
            return GetRootOrganization(parent);
        }

        //public void SelectOrganizationByCondition(Func<OrganizationForSelect, bool> condition, bool isToSelect)
        //{
        //    if (Entities != null)
        //    {
        //        foreach (var organization in Entities)
        //        {
        //            SelectOrganizationByCondition(organization, condition, isToSelect);
        //        }
        //    }
        //}

        //public void SelectOrganizationByCondition(OrganizationForSelect organization, Func<OrganizationForSelect, bool> condition, bool isToSelect)
        //{
        //    if (organization.ChildrenOrganizations != null && organization.ChildrenOrganizations.Count() > 0)
        //    {
        //        foreach (var child in organization.ChildrenOrganizations)
        //        {
        //            SelectOrganizationByCondition(child, condition, isToSelect);                    
        //        }
        //        //if (organization.SelectState == SelectStateEnum.UnSelected && organization.ChildrenOrganizations.Any(o=>o.SelectState != SelectStateEnum.UnSelected))
        //        //    organization.SelectState = SelectStateEnum.SelfUnSelected;//表明有子级被选中
        //        //if (organization.ChildrenOrganizations.All(o => o.SelectState == SelectStateEnum.Selected))
        //        //{
        //        //    if (!(condition(organization) ^ isToSelect))
        //        //    {
        //        //        organization.SelectState = SelectStateEnum.Selected;
        //        //    }
        //        //    else
        //        //    {
        //        //        organization.SelectState = SelectStateEnum.SelfUnSelected;
        //        //    }
        //        //}
        //        //else if (organization.ChildrenOrganizations.Any(o => o.SelectState != SelectStateEnum.UnSelected))
        //        //{
        //        //    if (!(condition(organization) ^ isToSelect))
        //        //    {
        //        //        organization.SelectState = SelectStateEnum.Selected;
        //        //    }
        //        //    else
        //        //    {
        //        //        organization.SelectState = SelectStateEnum.SelfUnSelected;
        //        //    }
        //        //}
        //    }
        //    if (condition(organization))
        //    {
        //        organization.SelectState = isToSelect ? SelectStateEnum.Selected : SelectStateEnum.UnSelected;
        //    }
        //}
    }

    public class OrganizationForSelect : SysOrganization, INotifyPropertyChanged
    {
        //private bool? _isSelected = false;
        //public bool? IsSelectedNew
        //{
        //    get { return _isSelected; }
        //    set
        //    {
        //        if (_isSelected != value)
        //        {
        //            _isSelected = value;
        //            if (this.ChildrenOrganizations != null && value != null)
        //            {
        //                foreach (var organization in ChildrenOrganizations)
        //                {
        //                    organization.IsSelectedNew = value;
        //                }
        //            }
        //            OnPropertyChanged("IsSelectedNew");
        //        }
        //    }
        //}

        private SelectStateEnum _selectState = SelectStateEnum.UnSelected;
        public SelectStateEnum SelectState
        {
            get { return _selectState; }
            set
            {
                if (_selectState != value)
                {
                    if (this.ChildrenOrganizations == null || this.ChildrenOrganizations.Count() == 0)
                    {
                        if (value == SelectStateEnum.SelfUnSelected)
                            value = SelectStateEnum.UnSelected;
                        else if (value == SelectStateEnum.SelfSelected)
                            value = SelectStateEnum.Selected;
                    }
                    //else
                    //{
                    //    foreach (var organization in ChildrenOrganizations)
                    //    {
                    //        if (value != SelectStateEnum.SelfUnSelected && value != SelectStateEnum.SelfSelected)
                    //            organization.SelectState = value;
                    //    }
                    //}
                    _selectState = value;
                    OnPropertyChanged("SelectState");
                }
            }
        }

        public string TypeName { get; set; }
        public string AreaName { get; set; }
        public string ProvienceName { get; set; }

        private IEnumerable<OrganizationForSelect> _childrenOrganizations;
        public IEnumerable<OrganizationForSelect> ChildrenOrganizations
        {
            get { return _childrenOrganizations; }
            set
            {
                if (_childrenOrganizations != value)
                {
                    _childrenOrganizations = value;
                    OnPropertyChanged("ChildrenOrganizations");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum SelectStateEnum
    {
        UnSelected = 0,
        Selected = 1,
        SelfUnSelected = 2,
        SelfSelected = 3
    }
}
