using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Kernel
{
    public class SystemDateTimeManager
    {
        //[DllImportAttribute("Kernel32.dll")]
        //public static extern void GetLocalTime(SystemTime st);
        [DllImportAttribute("Kernel32.dll")]
        private static extern void SetLocalTime(ref SystemTime st);

        /// <summary>
        /// 设置本机时间
        /// </summary>
        public void SetLocalDateTime(DateTime dateTime)
        {
            SystemTime systemTime = new SystemTime();
            systemTime.vYear = (ushort)dateTime.Year; 
            systemTime.vMonth = (ushort)dateTime.Month;
            systemTime.vDay = (ushort)dateTime.Day; 
            systemTime.vHour = (ushort)dateTime.Hour; 
            systemTime.vMinute = (ushort)dateTime.Minute;
            systemTime.vSecond = (ushort)dateTime.Second;
            SystemDateTimeManager.SetLocalTime(ref systemTime);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SystemTime
        {
            public ushort vYear;
            public ushort vMonth; 
            public ushort vDayOfWeek; 
            public ushort vDay; 
            public ushort vHour; 
            public ushort vMinute;
            public ushort vSecond;
        }
    }
}
