using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ViewModelBasic;

namespace SysProcessViewModel
{
    public class MapDistributionVM : CommonViewModel<OrganizationShowOnMap>
    {
        public MapDistributionVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<OrganizationShowOnMap> SearchData()
        {
            var lp = VMGlobal.SysProcessQuery.LinqOP;
            var allOrganizations = lp.Search<SysOrganization>().Select(o => new OrganizationShowOnMap(o)).ToList();
            var ds = VMGlobal.SysProcessQuery.DB.ExecuteDataSet("GetOrganizationDownHierarchy", VMGlobal.CurrentUser.OrganizationID);
            var table = ds.Tables[0];
            foreach (DataRow row in table.Rows)
            {
                var organization = allOrganizations.Find(o => o.ID == (int)row["OrganizationID"]);
                if (organization != null)
                    organization.IsOwned = true;
            }
            return allOrganizations;
        }
    }

    public class OrganizationShowOnMap : SysOrganizationBO
    {
        public bool IsOwned { get; set; }

        public OrganizationShowOnMap(SysOrganization organization)
            : base(organization)
        { }
    }
}
