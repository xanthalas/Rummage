﻿using System;
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

        [OptionArray("d", "excludedirectory", Required = false, HelpText = "Exclude directories whose name matches this regex.")]
        public string[] ExcludeDirectories;

        [OptionArray("n", "includedirectory", Required = false, HelpText = "Include directories whose name matches this regex.")]
        public string[] IncludeDirectories;

        [Option("c", "caseSensitive", Required = false, HelpText = "Make the search case-sensitive.")]
        public bool CaseSensitive = false;

        [Option("v", "verbose", Required = false, HelpText = "Show additional information during the search.")]
        public bool Verbose = false;

        [Option("r", "recurse", Required = false, HelpText = "Descend into sub-directories.")]
        public bool Recurse = false;

        [Option("b", "binaries", Required = false, HelpText = "Search in binary files as well as text files.")]
        public bool SearchBinaries = false;

        [Option("y", "confirm", Required = false, HelpText = "Confirm search should continue once number of files to search has been shown.")]
        public bool ConfirmSearch = false;

        [Option("h", "help", Required = false, HelpText = "Show this help text.")]
        public bool ShowHelp = false;

        [Option("o", "output", Required = false, HelpText = "Format of output.")]
        public string OutputFormat = "{MatchItem} : {MatchLineNumber} {MatchLine}";

        [Option("g", "cygwin", Required = false, HelpText = "Output filenames in Cygwin format")]
        public bool CygwinFormat = false;

        [Option("q", "quotes", Required = false, HelpText = "Output filenames surrounded by quotes")]
        public bool Quotes = false;

    }
}
