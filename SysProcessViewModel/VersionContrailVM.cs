using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViewModelBasic;
using DBAccess;
using CentralizeModel;
using System.Configuration;
using UpdateOnline;
using System.Diagnostics;

namespace SysProcessViewModel
{
    public class VersionContrailVM : CommonViewModel<SoftVersionTrackBO>
    {
        private LinqOPEncap _linqOP = null;//VMGlobal.PlatformCentralizeQuery.LinqOP;

        private UpdateOnlineSection _updateSection;
        private UpdateOnlineSection UpdateSection
        {
            get
            {
                if (_updateSection == null)
                {
                    var config = ConfigurationManager.OpenExeConfiguration(Process.GetCurrentProcess().MainModule.FileName);
                    _updateSection = config.Sections["UpdateOnline"] as UpdateOnlineSection;
                }
                return _updateSection;
            }
        }

        public VersionContrailVM()
        {
            Entities = this.SearchData();
        }

        protected override IEnumerable<SoftVersionTrackBO> SearchData()
        {
            if (UpdateSection == null)
                return null;

            var tracks = _linqOP.GetDataContext<SoftVersionTrack>();
            var softs = _linqOP.Search<SoftToUpdate>(o => o.IdentificationKey == UpdateSection.SoftKey);
            var customers = _linqOP.Search<Customer>(o => o.IdentificationKey == UpdateSection.CustomerKey);
            var mappings = _linqOP.GetDataContext<SoftVersionCustomerMapping>();
            var query = from t in tracks
                        from s in softs
                        where t.SoftID == s.ID
                        from map in mappings
                        where t.ID == map.SoftVersionID
                        from c in customers
                        where map.CustomerID == c.ID
                        select new SoftVersionTrack
                        {
                            CreateTime = t.CreateTime,
                            ID = t.ID,
                            IsCoerciveUpdate = t.IsCoerciveUpdate,
                            SoftID = t.SoftID,
                            UpdatedFileList = t.UpdatedFileList,
                            VersionCode = t.VersionCode
                        };
            var data = query.Select(o => new SoftVersionTrackBO(o)).ToList().OrderByDescending(o => o.CreateTime);
            return data;
        }
    }
}
