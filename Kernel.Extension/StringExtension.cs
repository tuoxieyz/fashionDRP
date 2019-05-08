using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.VisualBasic;
using System.Collections;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Security.Cryptography;

namespace Kernel
{
    public static class StringExtension
    {

        static public bool IsNullEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
        static public string IsNullEmpty(this string str, string defaultValue)
        {
            return str.IsNullEmpty() ? defaultValue : str;
        }

        static public string SafeSql(this string str)
        {
            str = str.IsNullEmpty() ? "" : str.Replace("'", "''");
            str = new Regex("exec", RegexOptions.IgnoreCase).Replace(str, "&#101;xec");
            str = new Regex("xp_cmdshell", RegexOptions.IgnoreCase).Replace(str, "&#120;p_cmdshell");
            str = new Regex("select", RegexOptions.IgnoreCase).Replace(str, "&#115;elect");
            str = new Regex("insert", RegexOptions.IgnoreCase).Replace(str, "&#105;nsert");
            str = new Regex("update", RegexOptions.IgnoreCase).Replace(str, "&#117;pdate");
            str = new Regex("delete", RegexOptions.IgnoreCase).Replace(str, "&#100;elete");

            str = new Regex("drop", RegexOptions.IgnoreCase).Replace(str, "&#100;rop");
            str = new Regex("create", RegexOptions.IgnoreCase).Replace(str, "&#99;reate");
            str = new Regex("rename", RegexOptions.IgnoreCase).Replace(str, "&#114;ename");
            str = new Regex("truncate", RegexOptions.IgnoreCase).Replace(str, "&#116;runcate");
            str = new Regex("alter", RegexOptions.IgnoreCase).Replace(str, "&#97;lter");
            str = new Regex("exists", RegexOptions.IgnoreCase).Replace(str, "&#101;xists");
            str = new Regex("master.", RegexOptions.IgnoreCase).Replace(str, "&#109;aster.");
            str = new Regex("restore", RegexOptions.IgnoreCase).Replace(str, "&#114;estore");
            return str;
        }
        static public string SafeSqlSimple(this string str)
        {
            str = str.IsNullEmpty() ? "" : str.Replace("'", "''");
            return str;
        }

        public static string UnHtml(this string htmlStr)
        {
            if (htmlStr.IsNullEmpty()) return string.Empty;
            return htmlStr.Replace("\"", "\\\"").ShowXmlHtml().Replace(" ", "&nbsp;").Replace("\n", "<br />");
        }
        public static string ShowXmlHtml(this string htmlStr)
        {
            if (htmlStr.IsNullEmpty()) return string.Empty;
            string str = htmlStr.Replace("&", "&amp;").Replace(">", "&gt;").Replace("<", "&lt;");
            return str;
        }

        public static string ShowHtml(this string htmlStr)
        {
            if (htmlStr.IsNullEmpty()) return string.Empty;
            string str = htmlStr;

            str = Regex.Replace(str, @"(script|frame|form|meta|behavior|style)([\s|:|>])+", "_$1.$2", RegexOptions.IgnoreCase);
            str = new Regex("<script", RegexOptions.IgnoreCase).Replace(str, "<_script");
            str = new Regex("<object", RegexOptions.IgnoreCase).Replace(str, "<_object");
            str = new Regex("javascript:", RegexOptions.IgnoreCase).Replace(str, "_javascript:");
            str = new Regex("vbscript:", RegexOptions.IgnoreCase).Replace(str, "_vbscript:");
            str = new Regex("expression", RegexOptions.IgnoreCase).Replace(str, "_expression");
            str = new Regex("@import", RegexOptions.IgnoreCase).Replace(str, "_@import");
            str = new Regex("<iframe", RegexOptions.IgnoreCase).Replace(str, "<_iframe");
            str = new Regex("<frameset", RegexOptions.IgnoreCase).Replace(str, "<_frameset");
            str = Regex.Replace(str, @"(\<|\s+)o([a-z]+\s?=)", "$1_o$2", RegexOptions.IgnoreCase);
            str = new Regex(@" (on[a-zA-Z ]+)=", RegexOptions.IgnoreCase).Replace(str, " _$1=");
            return str;
        }        

        public static int CnLength(this string str)
        {
            return Encoding.Default.GetBytes(str).Length;
        }

        public static string SubString(this string strInput, int len, string flg)
        {
            string myResult = string.Empty;
            if (len >= 0)
            {
                byte[] bsSrcString = Encoding.Default.GetBytes(strInput);
                if (bsSrcString.Length > len)
                {
                    int nRealLength = len;
                    int[] anResultFlag = new int[len];
                    byte[] bsResult = null;

                    int nFlag = 0;
                    for (int i = 0; i < len; i++)
                    {
                        if (bsSrcString[i] > 127)
                        {
                            nFlag++;
                            if (nFlag == 3) nFlag = 1;
                        }
                        else nFlag = 0;
                        anResultFlag[i] = nFlag;
                    }
                    if ((bsSrcString[len - 1] > 127) && (anResultFlag[len - 1] == 1))
                        nRealLength = len + 1;
                    bsResult = new byte[nRealLength];
                    Array.Copy(bsSrcString, bsResult, nRealLength);
                    myResult = Encoding.Default.GetString(bsResult);
                    myResult = myResult + (len >= strInput.CnLength() ? "" : flg);
                }
                else myResult = strInput;
            }
            return myResult;
        }

        public static string GetFileExtends(this string filename)
        {
            string ext = null;
            if (filename.IndexOf('.') > 0)
            {
                string[] fs = filename.Split('.');
                ext = fs[fs.Length - 1];
            }
            return ext;
        }
        public static string GetUrlFileName(this string url)
        {
            if (url == null) return "";
            string[] strs1 = url.Split(new char[] { '/' });
            return strs1[strs1.Length - 1].Split(new char[] { '?' })[0];
        }
        public static IList<string> GetHref(this string HtmlCode)
        {
            IList<string> MatchVale = new List<string>();
            string Reg = @"(h|H)(r|R)(e|E)(f|F) *= *('|"")?((\w|\\|\/|\.|:|-|_)+)('|""| *|>)?";
            foreach (Match m in Regex.Matches(HtmlCode, Reg))
            {
                MatchVale.Add((m.Value).ToLower().Replace("href=", "").Trim().TrimEnd('\'').TrimEnd('"').TrimStart('\'').TrimStart('"'));
            }
            return MatchVale;
        }
        public static IList<string> GetSrc(this string HtmlCode)
        {
            IList<string> MatchVale = new List<string>();
            string Reg = @"(s|S)(r|R)(c|C) *= *('|"")?((\w|\\|\/|\.|:|-|_)+)('|""| *|>)?";
            foreach (Match m in Regex.Matches(HtmlCode, Reg))
            {
                MatchVale.Add((m.Value).ToLower().Replace("src=", "").Trim().TrimEnd('\'').TrimEnd('"').TrimStart('\'').TrimStart('"'));
            }
            return MatchVale;
        }
        public static string GetEmailHostName(this string strEmail)
        {
            if (strEmail.IndexOf("@") < 0) return "";
            return strEmail.Substring(strEmail.LastIndexOf("@")).ToLower();
        }

        public static DateTime ToDateTime(this string DateTimeStr)
        {
            return DateTime.Parse(DateTimeStr);
        }
        public static string ToDateTime(this string fDateTime, string formatStr)
        {
            DateTime s = Convert.ToDateTime(fDateTime);
            return s.ToString(formatStr);
        }
        public static DateTime ToDateTime(this string DateTimeStr, DateTime defDate)
        {
            DateTime.TryParse(DateTimeStr, out defDate);
            return defDate;
        }
        public static DateTime? ToDateTime(this string DateTimeStr, DateTime? defDate)
        {
            DateTime dt = DateTime.Now;
            DateTime dt2 = dt;
            DateTime.TryParse(DateTimeStr, out dt);
            if (dt == dt2) return defDate;
            return dt;
        }

        public static byte[] ToBytes(this string value)
        {
            return value.ToBytes(null);
        }
        public static byte[] ToBytes(this string value, Encoding encoding)
        {
            encoding = (encoding ?? Encoding.Default);
            return encoding.GetBytes(value);
        }

        public static string RemoveHTML(this string HtmlCode)
        {
            string MatchVale = HtmlCode;
            MatchVale = new Regex("<br>", RegexOptions.IgnoreCase).Replace(MatchVale, "\n");
            foreach (Match s in Regex.Matches(HtmlCode, "<[^{><}]*>")) { MatchVale = MatchVale.Replace(s.Value, ""); }//"(<[^{><}]*>)"//@"<[\s\S-! ]*?>"//"<.+?>"//<(.*)>.*<\/\1>|<(.*) \/>//<[^>]+>//<(.|\n)+?>
            MatchVale = new Regex("\n", RegexOptions.IgnoreCase).Replace(MatchVale, "<br>");
            return MatchVale;
        }
        public static string RemoveAllHTML(this string content)
        {
            string pattern = "<[^>]*>";
            return Regex.Replace(content, pattern, string.Empty, RegexOptions.IgnoreCase);
        }

        public static string Reverse(this string value)
        {
            if (value.IsNullEmpty()) return string.Empty;

            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string FormatWith(this string str, params object[] args)
        {
            return string.Format(str, args);
        }
        public static string FormatWith(this string text, object arg0)
        {
            return string.Format(text, arg0);
        }
        public static string FormatWith(this string text, object arg0, object arg1)
        {
            return string.Format(text, arg0, arg1);
        }
        public static string FormatWith(this string text, object arg0, object arg1, object arg2)
        {
            return string.Format(text, arg0, arg1, arg2);
        }
        public static string FormatWith(this string text, IFormatProvider provider, params object[] args)
        {
            return string.Format(provider, text, args);
        }

        public static string ReplaceWith(this string value, string regexPattern, string replaceValue)
        {
            return ReplaceWith(value, regexPattern, replaceValue, RegexOptions.None);
        }
        public static string ReplaceWith(this string value, string regexPattern, string replaceValue, RegexOptions options)
        {
            return Regex.Replace(value, regexPattern, replaceValue, options);
        }
        public static string ReplaceWith(this string value, string regexPattern, MatchEvaluator evaluator)
        {
            return ReplaceWith(value, regexPattern, RegexOptions.None, evaluator);
        }
        public static string ReplaceWith(this string value, string regexPattern, RegexOptions options, MatchEvaluator evaluator)
        {
            return Regex.Replace(value, regexPattern, evaluator, options);
        }
        public static string ReplaceWith(this string value, string regexPattern, string ReplaceString, bool IsCaseInsensetive)
        {
            return Regex.Replace(value, regexPattern, ReplaceString, IsCaseInsensetive ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
        public static string Replace(this string RegValue, string regStart, string regEnd)
        {
            string s = RegValue;
            if (RegValue != "" && RegValue != null)
            {
                if (regStart != "" && regStart != null) { s = s.Replace(regStart, ""); }
                if (regEnd != "" && regEnd != null) { s = s.Replace(regEnd, ""); }
            }
            return s;
        }

        public static MatchCollection GetMatches(this string value, string regexPattern)
        {
            return GetMatches(value, regexPattern, RegexOptions.None);
        }
        public static MatchCollection GetMatches(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.Matches(value, regexPattern, options);
        }
        public static MatchCollection FindBetween(this string s, string startString, string endString)
        {
            return s.FindBetween(startString, endString, true);
        }
        public static MatchCollection FindBetween(this string s, string startString, string endString, bool recursive)
        {
            MatchCollection matches;

            startString = Regex.Escape(startString);
            endString = Regex.Escape(endString);

            Regex regex = new Regex("(?<=" + startString + ").*(?=" + endString + ")");

            matches = regex.Matches(s);

            if (!recursive) return matches;

            if (matches.Count > 0)
            {
                if (matches[0].ToString().IndexOf(Regex.Unescape(startString)) > -1)
                {
                    s = matches[0].ToString() + Regex.Unescape(endString);
                    return s.FindBetween(Regex.Unescape(startString), Regex.Unescape(endString));
                }
                else
                {
                    return matches;
                }
            }
            else
            {
                return matches;
            }
        }

        public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern)
        {
            return GetMatchingValues(value, regexPattern, RegexOptions.None);
        }
        public static IEnumerable<string> GetMatchingValues(this string value, string regexPattern, RegexOptions options)
        {
            foreach (Match match in GetMatches(value, regexPattern, options))
            {
                if (match.Success) yield return match.Value;
            }
        }
        public static IList<string> GetMatchingValues(this string value, string regexPattern, string rep1, string rep2)
        {
            IList<string> txtTextArr = new List<string>();
            string MatchVale = "";
            foreach (Match m in Regex.Matches(value, regexPattern))
            {
                MatchVale = m.Value.Trim().Replace(rep1, "").Replace(rep2, "");
                txtTextArr.Add(MatchVale);
            }
            return txtTextArr;
        }

        public static string[] Split(this string value, string regexPattern, RegexOptions options)
        {
            return Regex.Split(value, regexPattern, options);
        }
        public static string[] Split(this string value, string regexPattern)
        {
            return value.Split(regexPattern, RegexOptions.None);
        }

        public static XDocument ToXDocument(this string xml)
        {
            return XDocument.Parse(xml);
        }
        public static XmlDocument ToXmlDOM(this string xml)
        {
            var document = new XmlDocument();
            document.LoadXml(xml);
            return document;
        }
        public static XPathNavigator ToXPath(this string xml)
        {
            var document = new XPathDocument(new StringReader(xml));
            return document.CreateNavigator();
        }

        public static string Left(this string @string, int length)
        {
            if (length <= 0 || @string.Length == 0) return string.Empty;
            if (@string.Length <= length) return @string;
            return @string.Substring(0, length);
        }
        public static string Right(this string @string, int length)
        {
            if (length <= 0 || @string.Length == 0) return string.Empty;
            if (@string.Length <= length) return @string;
            return @string.Substring(@string.Length - length, length);
        }

        public static T CreateType<T>(this string typeName, params object[] args)
        {
            Type type = Type.GetType(typeName, true, true);
            return (T)Activator.CreateInstance(type, args);
        }
        public static T ToEnum<T>(this string value)
        {
            return ToEnum<T>(value, false);
        }
        public static T ToEnum<T>(this string value, bool ignorecase)
        {
            if (value == null) throw new ArgumentNullException("Value");
            value = value.Trim();
            if (value.Length == 0) throw new ArgumentNullException("Must specify valid information for parsing in the string.", "value");
            Type t = typeof(T);
            if (!t.IsEnum) throw new ArgumentException("Type provided must be an Enum.", "T");
            return (T)Enum.Parse(t, value, ignorecase);
        }

        public static int CharacterCount(this string value, char character)
        {
            int intReturnValue = 0;

            for (int intCharacter = 0; intCharacter <= (value.Length - 1); intCharacter++)
            {
                if (value.Substring(intCharacter, 1) == character.ToString()) intReturnValue += 1;
            }

            return intReturnValue;
        }

        public static string ForcePrefix(this string s, string prefix)
        {
            string result = s;
            if (!result.StartsWith(prefix)) result = prefix + result;
            return result;
        }
        public static string ForceSuffix(this string s, string suffix)
        {
            string result = s;
            if (!result.EndsWith(suffix)) result += suffix;
            return result;
        }
        public static string RemovePrefix(this string s, string prefix)
        {
            return Regex.Replace(s, "^" + prefix, System.String.Empty, RegexOptions.IgnoreCase);
        }
        public static string RemoveSuffix(this string s, string suffix)
        {
            return Regex.Replace(s, suffix + "$", System.String.Empty, RegexOptions.IgnoreCase);
        }

        public static string PadLeft(this string s, string pad)
        {
            return s.PadLeft(pad, s.Length + pad.Length, false);
        }
        public static string PadLeft(this string s, string pad, int totalWidth, bool cutOff)
        {
            if (s.Length >= totalWidth) return s;

            int padCount = pad.Length;

            string paddedString = s;

            while (paddedString.Length < totalWidth) paddedString += pad;

            if (cutOff) paddedString = paddedString.Substring(0, totalWidth);

            return paddedString;
        }
        public static string PadRight(this string s, string pad)
        {
            return PadRight(s, pad, s.Length + pad.Length, false);
        }
        public static string PadRight(this string s, string pad, int length, bool cutOff)
        {
            if (s.Length >= length) return s;

            string paddedString = string.Empty;

            while (paddedString.Length < length - s.Length) paddedString += pad;

            if (cutOff) paddedString = paddedString.Substring(0, length - s.Length);

            paddedString += s;

            return paddedString;
        }
        public static Color ToColor(this string s)
        {
            s = s.Replace("#", string.Empty);

            byte a = System.Convert.ToByte("ff", 16);

            byte pos = 0;

            if (s.Length == 8)
            {
                a = System.Convert.ToByte(s.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte g = System.Convert.ToByte(s.Substring(pos, 2), 16);

            pos += 2;

            byte b = System.Convert.ToByte(s.Substring(pos, 2), 16);

            return Color.FromArgb(a, r, g, b);
        }

        public static bool ContainsArray(this string value, params string[] keywords)
        {
            return keywords.All((s) => value.Contains(s));
        }
        public static Nullable<T> ToNullable<T>(this string s) where T : struct
        {
            T? result = null;
            if (!s.Trim().IsNullEmpty())
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T?));
                result = (T?)converter.ConvertFrom(s);
            }
            return result;
        }

        public static List<string> GetLines(this string text)
        {
            return text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        public static bool IsMatch(this string str, string op)
        {
            if (str.Equals(String.Empty) || str == null) return false;
            Regex re = new Regex(op, RegexOptions.IgnoreCase);
            return re.IsMatch(str);
        }
        public static bool IsIP(this string input)
        {
            return input.IsMatch(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$"); //@"^(([01]?[\d]{1,2})|(2[0-4][\d])|(25[0-5]))(\.(([01]?[\d]{1,2})|(2[0-4][\d])|(25[0-5]))){3}$";
        }
        public static bool IsIPSect(this string ip)
        {
            return ip.IsMatch(@"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){2}((2[0-4]\d|25[0-5]|[01]?\d\d?|\*)\.)(2[0-4]\d|25[0-5]|[01]?\d\d?|\*)$");
        }
        public static bool IsNumber(this string strNumber)
        {
            string pet = @"^([0-9])[0-9]*(\.\w*)?$";
            return strNumber.IsMatch(pet);
        }
        public static bool IsDouble(this string input)
        {
            string pet = @"^[0-9]*[1-9][0-9]*$";//@"^\d{1,}$"//整数校验常量//@"^-?(0|\d+)(\.\d+)?$"//数值校验常量 
            return input.IsMatch(pet);
        }
        public static bool IsInt(this string input)
        {
            string pet = @"^[0-9]*$"; //@"^([0-9])[0-9]*(\.\w*)?$";
            return input.IsMatch(pet);
        }
        public static bool IsNumberArray(this string[] strNumber)
        {
            if (strNumber == null) return false;
            if (strNumber.Length < 1) return false;
            foreach (string id in strNumber)
                if (!id.IsNumber()) return false;
            return true;
        }
        public static bool IsEmail(this string input)
        {
            string pet = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";//@"^\w+((-\w+)|(\.\w+))*\@\w+((\.|-)\w+)*\.\w+$";
            return input.IsMatch(pet);
        }
        public static bool IsUrl(this string input)
        {
            string pet = @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|60.191.40.5|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$";//@"^http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            return input.IsMatch(pet);
        }
        public static bool IsZip(this string input)
        {
            return input.IsMatch(@"\d{6}");
        }
        public static bool IsSSN(this string input)
        {
            string pet = @"\d{18}|\d{15}";
            return input.IsMatch(pet);
        }
        public static bool IsSafeSqlString(this string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }
        public static bool IsDateTime(this string input)
        {
            //string pet = @"^(?:(?:(?:(?:1[6-9]|[2-9]\d)?(?:0[48]|[2468][048]|[13579][26])|(?:(?:16|[2468][048]|[3579][26])00)))(\/|-|\.)(?:0?2\1(?:29))$)|(?:(?:1[6-9]|[2-9]\d)?\d{2})(\/|-|\.)(?:(?:(?:0?[13578]|1[02])\2(?:31))|(?:(?:0?[1,3-9]|1[0-2])\2(29|30))|(?:(?:0?[1-9])|(?:1[0-2]))\2(?:0?[1-9]|1\d|2[0-8]))$";
            string pet = @"^(?=\d)(?:(?!(?:1582(?:\.|-|\/)10(?:\.|-|\/)(?:0?[5-9]|1[0-4]))|(?:1752(?:\.|-|\/)0?9(?:\.|-|\/)(?:0?[3-9]|1[0-3])))(?=(?:(?!000[04]|(?:(?:1[^0-6]|[2468][^048]|[3579][^26])00))(?:(?:\d\d)(?:[02468][048]|[13579][26]))\D0?2\D29)|(?:\d{4}\D(?!(?:0?[2469]|11)\D31)(?!0?2(?:\.|-|\/)(?:29|30))))(\d{4})([-\/.])(0?\d|1[012])\2((?!00)[012]?\d|3[01])(?:$|(?=\x20\d)\x20))?((?:(?:0?[1-9]|1[012])(?::[0-5]\d){0,2}(?:\x20[aApP][mM]))|(?:[01]?\d|2[0-3])(?::[0-5]\d){1,2})?$";
            return input.IsMatch(pet);
        }
        public static bool IsDateTime2(this string DateTimeStr)
        {
            try { DateTime _dt = DateTime.Parse(DateTimeStr); return true; }
            catch { return false; }
        }
        public static bool IsDate(this string DateStr)
        {
            try { DateTime _dt = DateTime.Parse(DateStr); return true; }
            catch { return false; }
        }
        public static bool IsTime(this string TimeStr)
        {
            return TimeStr.IsMatch(@"^([0-1]\\d|2[0-3]):[0-5]\\d:[0-5]\\d$");//^((([0-1]?[0-9])|(2[0-3])):([0-5]?[0-9])(:[0-5]?[0-9])?)$
        }
        public static bool IsAlphaNumeric(this string input)
        {
            return input.IsMatch(@"[^a-zA-Z0-9]");
        }
        public static bool IsTelepone(this string input)
        {
            return input.IsMatch(RegexLib.Tel);//："^(\(\d{3,4}-)|\d{3.4}-)?\d{7,8}$
        }
        public static bool IsMobile(this string input)
        {
            return input.IsMatch(RegexLib.Mobile);
        }

        public static bool IsBase64String(this string str)
        {
            return Regex.IsMatch(str, @"[A-Za-z0-9\+\/\=]");
        }
        public static bool IsYear(this string input)
        {
            return Regex.IsMatch(input, @"^(19\d\d)|(200\d)$");
        }
        public static bool IsImgFileName(this string filename)
        {
            filename = filename.Trim();
            if (filename.EndsWith(".") || (filename.IndexOf(".") == -1)) return false;
            string str = filename.Substring(filename.LastIndexOf(".") + 1).ToLower();
            if (((str != "jpg") && (str != "jpeg")) && ((str != "png") && (str != "bmp"))) return (str == "gif");
            return true;
        }
        public static bool IsGuid(this string s)
        {
            if (s.IsNullEmpty()) return false;
            Regex format = new Regex("^[A-Fa-f0-9]{32}$|" +
                "^({|\\()?[A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12}(}|\\))?$|" +
                "^({)?[0xA-Fa-f0-9]{3,10}(, {0,1}[0xA-Fa-f0-9]{3,6}){2},{0,1}({)([0xA-Fa-f0-9]{3,4}, {0,1}){7}[0xA-Fa-f0-9]{3,4}(}})$");
            Match match = format.Match(s);
            return match.Success;
        }       

        public static Guid ToGuid(this string target)
        {
            if ((!target.IsNullEmpty()) && (target.Trim().Length == 22))
            {
                string encoded = string.Concat(target.Trim().Replace("-", "+").Replace("_", "/"), "==");
                byte[] base64 = Convert.FromBase64String(encoded);
                return new Guid(base64);
            }
            return Guid.Empty;
        }

        public static string ToMD5String(this string str)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] fromData = Encoding.Unicode.GetBytes(str);
            byte[] toData = md5.ComputeHash(fromData);//结果为长度为16的字节数组(128bit)
            string byteStr = null;
            for (int i = 0; i < toData.Length; i++)
            {
                byteStr += toData[i].ToString("x");
            }
            return byteStr;
        }
    }
}