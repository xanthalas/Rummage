using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace rmg
{
    class Options
    {
        [OptionArray("s", "search", Required = true, HelpText = "Regular Expression to search for.")]
        public string[] SearchStrings = null;

        [OptionArray("f", "folder", Required = false, HelpText = "The folder to search.")]
        public string[] Folders;

        [OptionArray("x", "excludefile", Required = false, HelpText = "Exclude files whose name matches this regex.")]
        public string[] ExcludeFiles;

        [OptionArray("i", "includefile", Required = false, HelpText = "Include only those files whose name matches this regex.")]
        public string[] IncludeFiles;

        [Option("c", "caseSensitive", Required = false, HelpText = "Make the search case-sensitive.")]
        public bool CaseSensitive = false;

        [Option("v", "verbose", Required = false, HelpText = "Show additional information during the search.")]
        public bool Verbose = false;

        /*
        [HelpOption("Display this help screen.", "help")]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("Rummage search tool v 0.1");
            Console.WriteLine(usage.ToString());
            return "";
        }
         * */

    }
}
