using DBAccess;
using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;

namespace SysProcessViewModel
{
    public class OrganizationAreaSetVM : SynchronousViewModel<OrganizationArea>
    {
        private LinqOPEncap _linqOP;

        public OrganizationAreaSetVM()
        {
            _linqOP = VMGlobal.SysProcessQuery.LinqOP;
            Entities = this.SearchData();
        }

        protected override IEnumerable<OrganizationArea> SearchData()
        {
            var result = _linqOP.Search<OrganizationArea>(o => o.OrganizationID == VMGlobal.CurrentUser.OrganizationID).ToList();
            return result;
        }
    }
}
