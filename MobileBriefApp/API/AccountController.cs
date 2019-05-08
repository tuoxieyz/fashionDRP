using DomainLogicEncap;
using SysProcessModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Security;

namespace MobileBriefApp.API
{
    public class AccountController : ApiController
    {
        [AcceptVerbsAttribute("GET")]
        public HttpResponseMessage Login(string userCode, string password, bool isRememberMe)
        {            
            var resp = new HttpResponseMessage();
            if (string.IsNullOrEmpty(userCode) || string.IsNullOrEmpty(password))
                resp.Content = new StringContent("用户名和密码不能为空.");
            else
            {
                var user = UserLogic.GetUserWhenLogin(userCode, password);
                if (user != null)
                {
                    //FormsAuthentication.SetAuthCookie(userCode, isRememberMe);//这是简单方式
                    //注意以下过期时间参数只是针对FormsAuthenticationTicket设置，而非Cookie设置
                    FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userCode, DateTime.Now, DateTime.Now.AddDays(30), isRememberMe, userCode + ",spt," + password, FormsAuthentication.FormsCookiePath);
                    string hashTicket = FormsAuthentication.Encrypt(ticket); //加密序列化验证票为字符串              
                    var cookie = new CookieHeaderValue(FormsAuthentication.FormsCookieName, hashTicket);
                    if (isRememberMe)
                        cookie.Expires = DateTimeOffset.Now.AddDays(30);
                    cookie.Domain = FormsAuthentication.CookieDomain;
                    cookie.Path = FormsAuthentication.FormsCookiePath;
                    resp.Headers.AddCookies(new CookieHeaderValue[] { cookie });

                    resp.Content = new ObjectContent<SysUser>(user, new JsonMediaTypeFormatter());
                }
                else
                    resp.Content = new StringContent("登录失败,请检查输入是否正确.");
            }
            return resp;
        }
    }
}