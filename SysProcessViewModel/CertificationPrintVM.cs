using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using Kernel;
using SysProcessModel;

namespace SysProcessViewModel
{
    public class CertificationPrintVM
    {
        private string[] _charArray = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public IEnumerable<string> GenerateBarCodes(int num)
        {
            List<string> barCodes = new List<string>();

            Random random = new Random();
            string[] codechars = new string[10];
            for (int i = 0; i < num; i++)
            {
                barCodes.Add(string.Join("", codechars.Select(o => _charArray[random.Next(0, _charArray.Length - 1)])));
            }
            return barCodes;
        }

        public Product GetProduct(int styleID, string colorCode, string sizeName)
        {
            var cid = VMGlobal.Colors.Find(o => o.Code == colorCode).ID;
            var sid = VMGlobal.Sizes.Find(o => o.Name == sizeName).ID;
            return VMGlobal.SysProcessQuery.LinqOP.Search<Product>(o => o.StyleID == styleID && o.ColorID == cid && o.SizeID == sid).FirstOrDefault();
        }
    }
}
