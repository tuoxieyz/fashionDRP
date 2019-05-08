using DistributionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace MobileBriefApp.API
{
    public class VIPController : ApiController
    {
        [System.Web.Mvc.AcceptVerbsAttribute("GET")]
        public object[] GetVIPProportion(int brandID, int organizationID)
        {
            VIPProportionVM context = new VIPProportionVM(brandID, organizationID);
            object[] data = new object[] { context.GetKindProportion(), context.GetConsumeProportion(), context.GetActiveProportion() };
            return data;
        }
    }
}
