using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq.Expressions;

namespace Kernel
{
    public static class DataExtension
    {
        public static List<TResult> ToList<TResult>(this DataTable dt) where TResult : class, new()
        {
            List<PropertyInfo> prlist = new List<PropertyInfo>();
            Type t = typeof(TResult);
            Array.ForEach<PropertyInfo>(t.GetProperties(), p =>
            {
                if (dt.Columns.IndexOf(p.Name) != -1)
                    prlist.Add(p);
            });

            List<TResult> oblist = new List<TResult>();

            foreach (DataRow row in dt.Rows)
            {

                TResult ob = new TResult();
                prlist.ForEach(p =>
                {
                    if (row[p.Name] != DBNull.Value)
                        p.SetValue(ob, row[p.Name], null);
                });
                oblist.Add(ob);
            }

            return oblist;
        }

        public static DataTable ToTable<T>(this IEnumerable<T> collection)
        {
            var props = typeof(T).GetProperties();
            var func = GetGetDelegate<T>(props);
            var dt = new DataTable();
            dt.Columns.AddRange(props.Select(p => new DataColumn(p.Name, p.PropertyType)).ToArray());
            collection.ToList().ForEach(i => dt.Rows.Add(func(i)));
            return dt;
        }

        static Func<T, object[]> GetGetDelegate<T>(PropertyInfo[] ps)
        {
            var param_obj = Expression.Parameter(typeof(T), "obj");
            Expression newArrayExpression = Expression.NewArrayInit(typeof(object), ps.Select(p => Expression.Property(param_obj, p)));
            return Expression.Lambda<Func<T, object[]>>(newArrayExpression, param_obj).Compile();
        }

        public static T FirstOrEmpty<T>(this IEnumerable<T> collection, Func<T, bool> predicate) where T : new()
        {
            T t = collection.FirstOrDefault(predicate);
            if (t == null)
                t = new T();
            return t;
        }

        public static Object RetrieveDataDecompress(byte[] binaryData)
        {
            MemoryStream memStream = new MemoryStream(Decompress(binaryData));
            IFormatter brFormatter = new BinaryFormatter();
            Object obj = brFormatter.Deserialize(memStream);
            memStream.Close();
            memStream.Dispose();
            return obj;
        }

        public static byte[] GetBinaryFormatDataCompress(object data)
        {
            byte[] binaryDataResult = null;
            MemoryStream memStream = new MemoryStream();
            IFormatter brFormatter = new BinaryFormatter();
            brFormatter.Serialize(memStream, data);
            binaryDataResult = memStream.ToArray();
            memStream.Close();
            memStream.Dispose();
            return Compress(binaryDataResult);
        }

        public static byte[] Decompress(byte[] data)
        {
            byte[] bData;
            MemoryStream ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            ms.Position = 0;
            GZipStream stream = new GZipStream(ms, CompressionMode.Decompress, true);
            byte[] buffer = new byte[1024];
            MemoryStream temp = new MemoryStream();
            int read = stream.Read(buffer, 0, buffer.Length);
            while (read > 0)
            {
                temp.Write(buffer, 0, read);
                read = stream.Read(buffer, 0, buffer.Length);
            }
            //必须把stream流关闭才能返回ms流数据,不然数据会不完整
            stream.Close();
            stream.Dispose();
            ms.Close();
            ms.Dispose();
            bData = temp.ToArray();
            temp.Close();
            temp.Dispose();
            return bData;
        }

        public static byte[] Compress(byte[] data)
        {
            byte[] bData;
            MemoryStream ms = new MemoryStream();
            GZipStream stream = new GZipStream(ms, CompressionMode.Compress, true);
            stream.Write(data, 0, data.Length);
            stream.Close();
            stream.Dispose();
            //必须把stream流关闭才能返回ms流数据,不然数据会不完整
            //并且解压缩方法stream.Read(buffer, 0, buffer.Length)时会返回0
            bData = ms.ToArray();
            ms.Close();
            ms.Dispose();
            return bData;
        }

        #region 属性快速Set相关

        public static Action<T, MethodInfo, object> GetSetDelegate<T>(MethodInfo m, Type type)
        {
            var param_obj = Expression.Parameter(typeof(T), "obj");
            var param_val = Expression.Parameter(typeof(object), "val");
            var param_m = Expression.Parameter(typeof(MethodInfo), "m");
            //var body_val = Expression.Convert(param_val, type);//这种方式在string到int类型转换中会出错((int)"string"本身就不正确)，因此改用下面显式指定转换方法（注意生成的lamda树的ToString()返回的字符串是一样的，都是Convert(val)）
            //关于何时需要指定转换方法，可参考 Expression.Convert 方法 (Expression, Type, MethodInfo) http://msdn.microsoft.com/zh-cn/library/bb353516.aspx
            var body_val = Expression.Convert(param_val, type, typeof(Convert).GetMethod("To" + type.Name, new[] { typeof(object) }));
            var body = Expression.Call(param_obj, m, body_val);
            Action<T, MethodInfo, object> set = Expression.Lambda<Action<T, MethodInfo, object>>(body, param_obj, param_m, param_val).Compile();
            return set;
        }

        public static void FastSetValue<T>(this PropertyInfo property, T t, object value)
        {
            MethodInfo m = property.GetSetMethod();
            GetSetDelegate<T>(m, property.PropertyType)(t, m, value);
        }

        #endregion

        #region 属性快速Get相关

        //构造委托类似Func<User, int> getAge = u => u.Age; 
        public static Func<T, object> GetGetDelegate<T>(PropertyInfo p)
        {
            var param_obj = Expression.Parameter(typeof(T), "obj");
            //lambda的方法体 u.Age
            var pGetter = Expression.Property(param_obj, p);
            //编译lambda
            return Expression.Lambda<Func<T, object>>(pGetter, param_obj).Compile();
        }

        public static object FastGetValue<T>(this PropertyInfo property, T t)
        {
            return GetGetDelegate<T>(property)(t);
        }

        #endregion
    }
}
