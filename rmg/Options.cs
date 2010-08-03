using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;

namespace rmg
{
    class Options
    {
        [OptionArray("s", "search", Required = false, HelpText = "Regular Expression to search for.")]
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

        [Option("n", "norecurse", Required = false, HelpText = "Don't descend into sub-directories.")]
        public bool NoRecurse = false;

        [Option("b", "binaries", Required = false, HelpText = "Search in binary files as well as text files.")]
        public bool SearchBinaries = false;

        [Option("h", "help", Required = false, HelpText = "Show this help text.")]
        public bool ShowHelp = false;

        [Option("o", "output", Required = false, HelpText = "Format of output.")]
        public string OutputFormat = "{MatchItem}:{MatchLineNumber} {MatchLine}";

    }
}
