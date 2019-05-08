using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Model.Extension;

namespace ViewModelBasic
{
    public class EnumVM
    {
        private Type enumType;

        public IEnumerable<IDNameImplementEntity> Values
        {
            get;
            private set;
        }

        //应用了该特性，在UI中就不需要显式指定转换实例
        //[TypeConverter(typeof(TypeTypeConverter))]//貌似这在wpf里不应该这么用，在silverlight中可以，因为wpf中的UI不可以以EnumType="DistributionModel.BillTypeEnum"指定类型
        public Type EnumType
        {
            get
            {
                return this.enumType;
            }
            set
            {
                this.enumType = value;
                this.InitValues();
            }
        }

        private void InitValues()
        {
            this.Values = this.EnumType.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                            .Select<FieldInfo, IDNameImplementEntity>(x =>
                            {
                                var obj = x.GetValue(this.EnumType);
                                return new IDNameImplementEntity { Name = obj.ToString(), ID = Convert.ToInt32(obj) };
                            });
        }

    }
}
