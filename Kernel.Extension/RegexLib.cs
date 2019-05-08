using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kernel
{
    /// <summary>
    /// 提供常用的正则表达式
    /// </summary>
    public static class RegexLib
    {
        /// <summary>
        /// 只能输入数字
        /// </summary>
        public static string Num = "^[0-9]*$";

        /// <summary>
        /// 只能输入中文
        /// </summary>
        public static string Chs = @"^[\u4e00-\u9fa5]{0,}$";

        /// <summary>
        /// Eamil
        /// </summary>
        public static string Email = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

        /// <summary>
        /// 电话号码
        /// </summary>
        public static string Tel = @"^[+]{0,1}(\d){1,3}[ ]?([-]?((\d)|[ ]){1,12})+$";

        /// <summary>
        /// 手机
        /// </summary>
        public static string Mobile = @"^0{0,1}(13[0-9]|15[5-9]|15[0-3]|18[0-3]|18[5-9]|147)[0-9]{8}$";
    }
}
