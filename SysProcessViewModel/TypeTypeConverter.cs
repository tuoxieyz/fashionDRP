using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace SysProcessViewModel
{
    //public class TypeTypeConverter : TypeConverter 
    //{
    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    //    {
    //        return sourceType.IsAssignableFrom(typeof(string));
    //    }

    //    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
    //    {
    //        // Try to load the type from the current assembly (EnumValuesInCombo.dll)
    //        Type t = Type.GetType((string)value, false);
    //        // If the type is from a different known assembly, try to load it from there           
    //        if (t == null)
    //        {
    //            // Try to load the type from System.Windows.dll
    //            t = this.GetTypeFromAssembly(value.ToString(), typeof(BillTypeEnum));
    //        }
    //        // You can also try with other known assemblies.
    //        //if (t == null)
    //        //{
    //        //    t = GetTypeFromAssembly(value.ToString(), typeof(a type that is in the assembly, containing the enum));
    //        //}
    //        return t;
    //    }

    //    private Type GetTypeFromAssembly(string typeName, Type knownType)
    //    {
    //        //AssemblyQualifiedName:类名称+程序集完全限定名
    //        string assemblyName = knownType.AssemblyQualifiedName;
    //        return Type.GetType(assemblyName.Replace(knownType.FullName, typeName), false);
    //    }

    //}
}
