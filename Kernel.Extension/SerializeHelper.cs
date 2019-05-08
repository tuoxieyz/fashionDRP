using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Security;
using System.Xml.Serialization;
using System.Globalization;
using System.Xml;
using System.Collections.Specialized;

namespace Kernel
{
    public static class SerializeHelper
    {
        #region BinaryFormatter
        #region SerializeObject
        public static byte[] SerializeObject(object obj) //obj 可所以数组
        {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream memoryStream = new MemoryStream();//此种景象下,mem_stream的缓冲区大小是可变的

            formatter.Serialize(memoryStream, obj);

            byte[] buff = memoryStream.ToArray();
            memoryStream.Close();

            return buff;
        }

        public static void SerializeObject(object obj, ref byte[] buff, int offset) //obj 可所以数组
        {
            byte[] rude_buff = SerializeHelper.SerializeObject(obj);
            for (int i = 0; i < rude_buff.Length; i++)
            {
                buff[offset + i] = rude_buff[i];
            }
        }
        #endregion

        #region DeserializeBytes
        public static object DeserializeBytes(byte[] buff, int index, int count)
        {
            IFormatter formatter = new BinaryFormatter();

            MemoryStream stream = new MemoryStream(buff, index, count);
            object obj = formatter.Deserialize(stream);
            stream.Close();

            return obj;
        }
        #endregion
        #endregion

        #region SoapFormatter
        #region SerializeObjectToString
        /// <summary>
        /// SerializeObjectToString 将对象序列化为SOAP XML 格局。
        /// 若是要将对象转化为简洁的xml格局,请应用ESBasic.Persistence.SimpleXmlConverter类。
        /// </summary>        
        //public static string SerializeObjectToString(object obj)
        //{
        //    IFormatter formatter = new SoapFormatter();
        //    MemoryStream stream = new MemoryStream();
        //    formatter.Serialize(stream, obj);
        //    stream.Position = 0;
        //    StreamReader reader = new StreamReader(stream);
        //    string res = reader.ReadToEnd();
        //    stream.Close();

        //    return res;
        //}
        #endregion

        #region DeserializeString
        //public static object DeserializeString(string str, System.Text.Encoding ecnoding)
        //{
        //    byte[] buff = ecnoding.GetBytes(str);
        //    IFormatter formatter = new SoapFormatter();
        //    MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
        //    object obj = formatter.Deserialize(stream);
        //    stream.Close();

        //    return obj;
        //}
        #endregion
        #endregion

        #region XmlSerializer
        #region XmlObject
        public static string XmlObject(object obj)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            MemoryStream stream = new MemoryStream();
            xmlSerializer.Serialize(stream, obj);
            stream.Position = 0;
            StreamReader reader = new StreamReader(stream);
            string res = reader.ReadToEnd();
            stream.Close();

            return res;
        }
        #endregion

        #region ObjectXml
        public static T ObjectXml<T>(string str, System.Text.Encoding ecnoding)
        {
            return (T)SerializeHelper.ObjectXml(str, typeof(T), ecnoding);
        }

        public static object ObjectXml(string str, Type targetType, System.Text.Encoding ecnoding)
        {
            byte[] buff = ecnoding.GetBytes(str);
            XmlSerializer xmlSerializer = new XmlSerializer(targetType);
            MemoryStream stream = new MemoryStream(buff, 0, buff.Length);
            object obj = xmlSerializer.Deserialize(stream);
            stream.Close();

            return obj;
        }
        #endregion
        #endregion

        #region SaveToFile
        /// <summary>
        /// SaveToFile 将对象的二进制序列化后保存到文件。
        /// </summary>       
        public static void SaveToFile(object obj, string filePath)
        {
            FileStream stream = new FileStream(filePath, FileMode.CreateNew);
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);

            stream.Flush();
            stream.Close();
        }
        #endregion

        #region ReadFromFile
        /// <summary>
        /// ReadFromFile 从文件读取二进制反序列化为对象。
        /// </summary> 
        //public static object ReadFromFile(string filePath)
        //{
        //    byte[] buff = FileHelper.ReadFileReturnBytes(filePath);
        //    return SerializeHelper.DeserializeBytes(buff, 0, buff.Length);
        //}
        #endregion
    }

    /// <summary>
    /// 序列化类
    /// </summary>
    public static class Serializer
    {


        /// <summary>
        /// Static Constructor is used to set the CanBinarySerialize value only once for the given security policy
        /// </summary>
        static Serializer()
        {
            SecurityPermission sp = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
            try
            {
                sp.Demand();
                CanBinarySerialize = true;
            }
            catch (SecurityException)
            {
                CanBinarySerialize = false;
            }
        }

        /// <summary>
        /// Readonly value indicating if Binary Serialization (using BinaryFormatter) is allowed
        /// </summary>
        public static readonly bool CanBinarySerialize;

        /// <summary>
        /// Converts a .NET object to a byte array. Before the conversion happens, a check with 
        /// Serializer.CanBinarySerialize will be made
        /// </summary>
        /// <param name="objectToConvert">Object to convert</param>
        /// <returns>A byte arry representing the object paramter. Null will be return if CanBinarySerialize is false</returns>
        public static byte[] ConvertToBytes(object objectToConvert)
        {
            byte[] byteArray = null;

            if (CanBinarySerialize)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {

                    binaryFormatter.Serialize(ms, objectToConvert);


                    // Set the position of the MemoryStream back to 0
                    //
                    ms.Position = 0;

                    // Read in the byte array
                    //
                    byteArray = new Byte[ms.Length];
                    ms.Read(byteArray, 0, byteArray.Length);
                    ms.Close();
                }
            }
            return byteArray;
        }

        /// <summary>
        /// Saves an object to disk as a binary file. 
        /// </summary>
        /// <param name="objectToSave">Object to Save</param>
        /// <param name="path">Location of the file</param>
        /// <returns>true if the save was succesful.</returns>
        public static bool SaveAsBinary(object objectToSave, string path)
        {
            if (objectToSave != null && CanBinarySerialize)
            {
                byte[] ba = ConvertToBytes(objectToSave);
                if (ba != null)
                {
                    using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            bw.Write(ba);
                            return true;
                        }
                    }
                }
            }
            return false;
        }


        /// <summary>
        /// Converts a .NET object to a string of XML. The object must be marked as Serializable or an exception
        /// will be thrown.
        /// </summary>
        /// <param name="objectToConvert">Object to convert</param>
        /// <returns>A xml string represting the object parameter. The return value will be null of the object is null</returns>
        public static string ConvertToString(object objectToConvert)
        {
            string xml = null;

            if (objectToConvert != null)
            {
                //we need the type to serialize
                Type t = objectToConvert.GetType();

                XmlSerializer ser = new XmlSerializer(t);
                //will hold the xml
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
                {
                    ser.Serialize(writer, objectToConvert);
                    xml = writer.ToString();
                    writer.Close();
                }
            }

            return xml;
        }

        public static void SaveAsXML(object objectToConvert, string path)
        {
            if (objectToConvert != null)
            {
                //we need the type to serialize
                Type t = objectToConvert.GetType();

                XmlSerializer ser = new XmlSerializer(t);
                //will hold the xml
                using (StreamWriter writer = new StreamWriter(path))
                {
                    ser.Serialize(writer, objectToConvert);
                    writer.Close();
                }
            }

        }


        /// <summary>
        /// Converts a byte array to a .NET object. You will need to cast this object back to its expected type. 
        /// If the array is null or empty, it will return null.
        /// </summary>
        /// <param name="byteArray">An array of bytes represeting a .NET object</param>
        /// <returns>The byte array converted to an object or null if the value of byteArray is null or empty</returns>
        public static object ConvertToObject(byte[] byteArray)
        {
            object convertedObject = null;
            if (CanBinarySerialize && byteArray != null && byteArray.Length > 0)
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(byteArray, 0, byteArray.Length);

                    // Set the memory stream position to the beginning of the stream
                    //
                    ms.Position = 0;

                    if (byteArray.Length > 4)
                        convertedObject = binaryFormatter.Deserialize(ms);

                    ms.Close();
                }
            }
            return convertedObject;
        }

        /// <summary>
        /// Converts a string of xml to the supplied object type. 
        /// </summary>
        /// <param name="xml">Xml representing a .NET object</param>
        /// <param name="objectType">The type of object which the xml represents</param>
        /// <returns>A instance of object or null if the value of xml is null or empty</returns>
        public static object ConvertToObject(XmlNode node, Type objectType)
        {
            object convertedObject = null;

            if (node != null)
            {
                using (StringReader reader = new StringReader(node.OuterXml))
                {

                    XmlSerializer ser = new XmlSerializer(objectType);

                    convertedObject = ser.Deserialize(reader);

                    reader.Close();
                }
            }
            return convertedObject;
        }


        public static object ConvertFileToObject(string path, Type objectType)
        {
            object convertedObject = null;

            if (path != null && path.Length > 0)
            {
                using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    XmlSerializer ser = new XmlSerializer(objectType);
                    convertedObject = ser.Deserialize(fs);
                    fs.Close();
                }
            }
            return convertedObject;
        }

        /// <summary>
        /// Converts a string of xml to the supplied object type. 
        /// </summary>
        /// <param name="xml">Xml representing a .NET object</param>
        /// <param name="objectType">The type of object which the xml represents</param>
        /// <returns>A instance of object or null if the value of xml is null or empty</returns>
        public static object ConvertToObject(string xml, Type objectType)
        {
            object convertedObject = null;

            if (!string.IsNullOrEmpty(xml))
            {
                using (StringReader reader = new StringReader(xml))
                {
                    XmlSerializer ser = new XmlSerializer(objectType);
                    convertedObject = ser.Deserialize(reader);
                    reader.Close();
                }
            }
            return convertedObject;
        }

        public static object LoadBinaryFile(string path)
        {
            if (!File.Exists(path))
                return null;

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                BinaryReader br = new BinaryReader(fs);
                byte[] ba = new byte[fs.Length];
                br.Read(ba, 0, (int)fs.Length);
                return ConvertToObject(ba);
            }
        }

        /// <summary>
        /// Creates a NameValueCollection  two string. The first contains the key pattern and the second contains the values
        /// spaced according to the kys
        /// </summary>
        /// <param name="keys">Keys for the namevalue collection</param>
        /// <param name="values">Values for the namevalue collection</param>
        /// <returns>A NVC populated based on the keys and vaules</returns>
        /// <example>
        /// string keys = "key1:S:0:3:key2:S:3:2:";
        /// string values = "12345";
        /// This would result in a NameValueCollection with two keys (Key1 and Key2) with the values 123 and 45
        /// </example>
        public static NameValueCollection ConvertToNameValueCollection(string keys, string values)
        {
            NameValueCollection nvc = new NameValueCollection();

            if (keys != null && values != null && keys.Length > 0 && values.Length > 0)
            {
                char[] splitter = new char[1] { ':' };
                string[] keyNames = keys.Split(splitter);

                for (int i = 0; i < (keyNames.Length / 4); i++)
                {
                    int start = int.Parse(keyNames[(i * 4) + 2], CultureInfo.InvariantCulture);
                    int len = int.Parse(keyNames[(i * 4) + 3], CultureInfo.InvariantCulture);
                    string key = keyNames[i * 4];

                    //Future version will support more complex types 
                    if (((keyNames[(i * 4) + 1] == "S") && (start >= 0)) && (len > 0) && (values.Length >= (start + len)))
                    {
                        nvc[key] = values.Substring(start, len);
                    }
                }
            }

            return nvc;
        }

        /// <summary>
        /// Creates a the keys and values strings for the simple serialization based on a NameValueCollection
        /// </summary>
        /// <param name="nvc">NameValueCollection to convert</param>
        /// <param name="keys">the ref string will contain the keys based on the key format</param>
        /// <param name="values">the ref string will contain all the values of the namevaluecollection</param>
        public static void ConvertFromNameValueCollection(NameValueCollection nvc, ref string keys, ref string values)
        {
            if (nvc == null || nvc.Count == 0)
                return;

            StringBuilder sbKey = new StringBuilder();
            StringBuilder sbValue = new StringBuilder();

            int index = 0;
            foreach (string key in nvc.AllKeys)
            {
                if (key.IndexOf(":") != -1)
                    throw new ArgumentException("ExtendedAttributes Key can not contain the character \":\"");

                string v = nvc[key];
                if (!string.IsNullOrEmpty(v))
                {
                    sbKey.AppendFormat("{0}:S:{1}:{2}:", key, index, v.Length);
                    sbValue.Append(v);
                    index += v.Length;
                }
            }
            keys = sbKey.ToString();
            values = sbValue.ToString();
        }
    }
}
