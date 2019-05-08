using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SysProcessViewModel;

namespace SysProcessView.Product
{
    public partial class ProStyleMatchManage : ProStylePictureBook
    {
        //默认会调用父类的无参构造器
        public ProStyleMatchManage()
            : base(true)
        {
            this.DataContext = new ProStyleMatchManageVM();
            //InitializeComponent();
        }
    }
}
