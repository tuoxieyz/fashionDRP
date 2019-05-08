using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DBAccess;
using Telerik.Windows.Controls;
using System.Collections.ObjectModel;
using DomainLogicEncap;
using Kernel;
using IWCFServiceForIM;
using UpdateOnline;
using System.Diagnostics;
using System.Configuration;
using CentralizeModel;
using System.Net.Http;

namespace SysProcessViewModel
{
    public class MainWindowVM : ViewModelBase
    {
        //private IMHelper _imHelper = new IMHelper();

        public static MainWindowVM Current { get; private set; }

        public event Action<bool> LogoutEvent;

        public string WelcomeMessage { get; set; }

        private MenuTreeVM _menuTreeVM;

        public MenuTreeVM MenuTreeVM
        {
            get { return _menuTreeVM; }
        }

        public ClientIMessage RecentMessage { get { return Messages.FirstOrDefault(); } }

        public int MessageCount { get { return Messages.Count; } }

        public ObservableCollection<ClientIMessage> Messages { get { return IMHelper.Messages; } }

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

        public Customer CustomerInfo { private set; get; }

        public MainWindowVM()
        {
            Current = this;
            Messages.CollectionChanged += delegate
            {
                OnPropertyChanged("RecentMessage");
                OnPropertyChanged("MessageCount");
            };
            this.SetCustomer();
        }

        private void SetCustomer()
        {
            HttpClient client = new HttpClient();
            var response = client.GetAsync(ConfigurationManager.AppSettings["PlatformSite"] + "api/customer/GetCustomer?key=" + UpdateSection.CustomerKey).Result;
            if (response.IsSuccessStatusCode)
            {
                CustomerInfo = response.Content.ReadAsAsync<Customer>().Result;
                if (CustomerInfo == null)
                    throw new Exception("非授权客户");
            }
        }

        public OPResult Login(string userCode, string password)
        {
            OPResult result = new OPResult { IsSucceed = false, Message = "登录失败." };
            var user = UserLogic.GetUserWhenLogin(userCode, password);
            if (user != null)
            {
                VMGlobal.CurrentUser = new SysUserBO(user);
                IMHelper.RegisterToIM();
                //IMHelper.StartService();
                WelcomeMessage = string.Format("当前登录用户: [{0}]{1}", user.Code, user.Name);
                OnPropertyChanged("WelcomeMessage");
                _menuTreeVM = new MenuTreeVM();
                OnPropertyChanged("MenuTreeVM");
                result.IsSucceed = true;
            }
            if (CustomerInfo == null || !(CustomerInfo.Name.Contains("铭吉") || CustomerInfo.Name.Contains("蝶讯") || CustomerInfo.Name.Contains("乔治") || CustomerInfo.Name.Contains("德融") || CustomerInfo.Name.Contains("慕之淇") || CustomerInfo.Name.Contains("加布瑞尔")))
                throw new Exception("非授权客户");
            return result;
        }

        public void Logout(bool isForced = false)
        {
            IMHelper.LogoutFromIM();
            VMGlobal.CleanUserData();
            if (LogoutEvent != null)
                LogoutEvent(isForced);
        }

        public void WindowClosing()
        {
            IMHelper.LogoutFromIM();
            IMHelper.CloseService();//这步貌似挺花时间的，要给予用户提示
        }
    }
}
