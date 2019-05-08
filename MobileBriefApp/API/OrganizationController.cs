using DBEncapsulation;
using Kernel;
using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MobileBriefApp.API
{
    public class OrganizationController : ApiController
    {
        public IEnumerable<SysOrganization> GetAll()
        {
            using (var dbContext = new SysProcessEntities())
            {
                return dbContext.SysOrganization.Where(o => o.Flag).ToArray();
            }
        }

        [HttpPut]
        public OPResult SetPosition(int organizationID, decimal? lng, decimal? lat)
        {
            using (var dbContext = new SysProcessEntities())
            {
                try
                {
                    var organization = dbContext.SysOrganization.Find(organizationID);
                    organization.Longitude = lng;
                    organization.Latitude = lat;
                    dbContext.SaveChanges();
                }
                catch (Exception e)
                {
                    return new OPResult { IsSucceed = false, Message = e.Message };
                }
            }
            return new OPResult { IsSucceed = true };
        }
    }
}
