/*
 * 参照 Telerik.BuildTasks.dll
 */
using Microsoft.Build.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BuildTasks
{
    public class XamlPreprocessor
    {
        // Fields
        private static readonly Regex uniqueCodeConditionalReplacer = new Regex(@"<\s*\?UniqueCode BEGIN\s*\?>(?<content>(.|\n)*?)<\s*\?UniqueCode END\s*\?>", RegexOptions.Compiled | RegexOptions.Multiline);

        public string Process(string input)
        {
            return this.ReplaceUniqueCodeSpecificConditionals(input);
        }

        private string ReplaceUniqueCodeSpecificConditionals(string input)
        {
            if (this.UniqueCode)
            {
                return uniqueCodeConditionalReplacer.Replace(input, "${content}");
            }
            return uniqueCodeConditionalReplacer.Replace(input, "");
        }

        public bool UniqueCode { get; set; }
    }


}
