using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Kernel;
using SysProcessModel;
using SysProcessViewModel;

namespace HabilimentERP
{
    #region 辅助类

    //internal class OldPasswordValidationRule : ValidationRule
    //{
    //    public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
    //    {
    //        string pwd = value as string;
    //        if (string.IsNullOrEmpty(pwd))
    //            return new ValidationResult(false,"请正确输入当前密码.");
    //        if (pwd != VMGlobal.CurrentUser.Password)
    //            return new ValidationResult(false, "当前密码输入错误.");
    //        return ValidationResult.ValidResult;
    //    }
    //}

    #endregion

    /// <summary>
    /// Interaction logic for PasswordSetWin.xaml
    /// </summary>
    public partial class PasswordSetWin : Window
    {
        //PasswordSetEntitty _dataContext = new PasswordSetEntitty();

        public PasswordSetWin()
        {
            //this.DataContext = _dataContext;
            InitializeComponent();
            this.Loaded += delegate { txtOldPassword.Focus(); };
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            //Validation.GetHasError(txtOldPassword);
            string oldpwd = txtOldPassword.Password;
            string newpwd = txtNewPassword.Password;
            string surepwd = txtNewPasswordSure.Password;
            if (oldpwd.ToMD5String() != VMGlobal.CurrentUser.Password)
            {
                MessageBox.Show("当前密码输入错误.");
                return;
            }
            if (!SysUserBO.IsMatchPWDReg(newpwd))
            {
                MessageBox.Show("新密码格式不正确,必须为至少6位字母和数字组合字符串.");
                return;
            }
            if (newpwd != surepwd)
            {
                MessageBox.Show("两次输入的新密码不一致.");
                return;
            }
            var result = ((SysUserBO)VMGlobal.CurrentUser).ResetPassword(newpwd);            
            if (result.IsSucceed)
            {
                MessageBox.Show("修改成功,修改后密码为" + newpwd);
                this.Close();
            }
            else
                MessageBox.Show(result.Message);
        }
    }
}
