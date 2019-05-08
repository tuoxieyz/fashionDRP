using DomainLogicEncap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace MobileBriefApp.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (!string.IsNullOrEmpty(Request["ReturnUrl"]))
            {
                string returnUrl = Request["ReturnUrl"];
                if (User.Identity.IsAuthenticated)
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    ViewData["ReturnUrl"] = returnUrl;
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            Response.Cookies[FormsAuthentication.FormsCookieName].Expires = DateTime.Now.AddDays(-1);
            return RedirectToAction("Login");
        }
    }
}
