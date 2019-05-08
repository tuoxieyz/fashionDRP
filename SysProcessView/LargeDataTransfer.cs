using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IWCFService;
using Kernel;
using System.ServiceModel;

namespace SysProcessView
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    public class LargeDataTransfer : ILargeDataTransfer
    {
        private int _lenatime = 10240; //每次读取字节的数量
        private int _iti = 0;  //初始化循环次数
        byte[] _byte_All;//获取要上传的字节流

        /// <summary>
        /// 每次回调触发该事件
        /// </summary>
        public event Action<int> CallbackEvent;

        /// <summary>
        /// 需要传递的数据对象
        /// </summary>
        private object _data;
        public object Data
        {
            get
            {
                return _data;
            }
            set
            {
                if (value != null)
                    _byte_All = DataExtension.GetBinaryFormatDataCompress(value);
                _data = value;
            }
        }

        #region ILargeDataTransfer 成员
        
        public byte[] GetBytes(int intStep)
        {
            int i = _lenatime;
            if (intStep >= _iti - 1)//最后一次
            {
                i = _byte_All.Length - ((_iti - 1) * _lenatime);
            }
            int iold = _lenatime * intStep;  //记录上一次的字节位置
            if (CallbackEvent != null)
                CallbackEvent((intStep + 1) * 100 / _iti);
            return _byte_All.Skip(iold).Take(i).ToArray();
        }
        
        public int GetTimes()  //将数据流分为多少部分
        {
            int temp = _byte_All.Length / _lenatime;
            int intStep = _byte_All.Length % _lenatime != 0 ? temp + 1 : temp;
            _iti = intStep;
            return intStep;
        }
        #endregion
    }
}
