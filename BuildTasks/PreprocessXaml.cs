/*
 * 参照 Telerik.BuildTasks.dll
 */
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace BuildTasks
{
    public class PreprocessXaml : Task
    {
        // Fields
        private readonly string[] preprocessedLiterals = new string[] { "<?UniqueCode BEGIN?>", "<?UniqueCode END?>" };

        public override bool Execute()
        {
            try
            {
                this.RunXamlPreprocessor();
            }
            catch (Exception exception)
            {
                base.Log.LogErrorFromException(exception, false, true, null);
            }
            return !base.Log.HasLoggedErrors;
        }

        private void RunXamlPreprocessor()
        {
            string itemSpec = this.SourceFile.ItemSpec;
            string fullPath = Path.GetFullPath(itemSpec);
            
            base.Log.LogMessage("Processing: " + fullPath, new object[0]);
            Stopwatch stopwatch = Stopwatch.StartNew();
            XamlPreprocessor preprocessor = new XamlPreprocessor
            {
                UniqueCode = 
#if UniqueCode
                true
#else 
            false
#endif
            };
            Func<string, string> func = path => File.ReadAllText(path);
            string processedContents = null;
            try
            {
                processedContents = preprocessor.Process(func(fullPath));                
            }
            catch (Exception exception)
            {
                base.Log.LogErrorFromException(exception, false, true, itemSpec);
                return;
            }
            this.ValidatePreprocessedResult(processedContents, fullPath);
            File.WriteAllText(Path.GetFullPath(this.DestinationFile.ItemSpec), processedContents);
            base.Log.LogMessage(string.Concat(new object[] { "Completed Processing: ", fullPath, " in ", stopwatch.Elapsed }), new object[0]);
        }

        private void ValidatePreprocessedResult(string processedContents, string sourcePath)
        {
            foreach (string str in this.preprocessedLiterals)
            {
                if (processedContents.Contains(str))
                {
                    base.Log.LogError("File {0} contains unnecessary preprocessed literal {1}", new object[] { sourcePath, str });
                }
            }
        }

        // Properties
        [Required]
        public ITaskItem DestinationFile { get; set; }

        [Required]
        public ITaskItem SourceFile { get; set; }

        //public bool UniqueCode { get; set; }
    }
}
