using DomainLogicEncap;
using SysProcessModel;
using SysProcessViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
//using Newtonsoft.Json;

namespace MobileBriefApp.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);//解密 
            string[] userData = authTicket.UserData.Split(new string[] { ",spt," }, StringSplitOptions.None);
            var user = UserLogic.GetUserWhenLogin(userData[0], userData[1]);
            if (user == null)
                return RedirectToAction("Login", "Account");
            var modules = UserLogic.ModuleProcessOfUser(user.ID);
            var jsconvert = new JavaScriptSerializer();
            ViewData["PoweredBrands"] = jsconvert.Serialize(VMGlobal.GetPoweredBrands(user));
            ViewData["CurrentUser"] = jsconvert.Serialize(user);
            return View(ConstructModules(modules));
        }

        List<ModuleBO> ConstructModules(List<SysModule> modules)
        {
            List<ModuleBO> bos = new List<ModuleBO>();
            foreach (var module in modules)
            {
                if (!string.IsNullOrEmpty(module.MobileUri))
                    bos.Add(new ModuleBO
                    {
                        MobileUri = module.MobileUri,
                        Name = module.Name,
                        ParentName = modules.Find(o => o.Code == module.ParentCode).Name
                    });
            }
            return bos;
        }
    }
}
