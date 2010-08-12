using System;
using System.IO;
using System.Text;
using CommandLine;
using CommandLine.Text;
using RummageCore;
using RummageFilesystem;

/**********************************************************************************************************************
 * Uses the command line parser by Giacomo Stelluti Scala available here: http://commandline.codeplex.com/            *
 *********************************************************************************************************************/
namespace rmg
{
    class Program
    {
        private const string DEFAULT_FILE = "rmg-defaults.txt";

        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static bool _verbose = false;

        private static string _outputFormat = string.Empty;

        private static bool _confirmSearch = false;

        static void Main(string[] args)
        {
            ISearchRequest searchRequest = new SearchRequestFilesystem();

            string[] defaultOptions = loadDefaults();
            if (log.IsDebugEnabled)
            {
                log.Debug("Default options read: ");
                foreach (string opt in defaultOptions)
                {
                    log.Debug("    " + opt);
                }
            }

            string[] allArgs = new string[(defaultOptions.Length + args.Length)];
            defaultOptions.CopyTo(allArgs, 0);
            args.CopyTo(allArgs, defaultOptions.Length);

            if (!parseOptions(searchRequest, allArgs))
            {
                return;     //If option parsing fails (including if help is requested) then drop out
            }

            if (_verbose) {Console.WriteLine("Starting search.");}

            int numberOfFilesToSearch = searchRequest.Prepare();

            if (numberOfFilesToSearch == 0)
            {
                Console.WriteLine("No files will be searched. Check your filter conditions.");
                return;
            }

            if (searchRequest.IsPrepared && numberOfFilesToSearch > 0)
            {
                if (_confirmSearch)
                {
                    Console.WriteLine("{0} files will be searched. Continue (Y/n)", numberOfFilesToSearch);
                    string response = Console.ReadLine();
                    if (response.Length == 0)
                    {
                        response = "Y";
                    }

                    if (response.ToUpper().Substring(0, 1) != "Y")
                    {
                        Console.WriteLine("Search aborted");
                        return;
                    }
                }
                else
                {
                    Console.WriteLine("Searching {0} files...", numberOfFilesToSearch);
                }
                if (_verbose) { Console.WriteLine("{0} files will be searched", searchRequest.Urls.Count); }
                ISearch search = new SearchFilesystem();
                search.ItemSearched += new ItemSearchedEventHandler(search_ItemSearched); 
                search.Search(searchRequest, true);
                
                if (_verbose) { Console.WriteLine("Search complete. Found {0} matches.", search.Matches.Count); }
            }
            else
            {
                Console.WriteLine("Not enough information has been supplied to perform the search.");
            }
        }

        /// <summary>
        /// Event handler invoked when a file search has been completed.
        /// </summary>
        /// <param name="sender">Standard argument for the sender of the event</param>
        /// <param name="e">Standard Event Args argument - ItemSearchedEventArgs</param>
        static void search_ItemSearched(object sender, EventArgs e)
        {
            var iea = e as ItemSearchedEventArgs;

            foreach (IMatch match in iea.Matches)
            {
                if (match.Successful)
                {
                    Console.WriteLine(formatOutputLine(match));
                }
                else
                {
                    Console.WriteLine("Couldn't search {0}:{1}", match.MatchItem, match.ErrorMessage);
                }
            }
        }

        /// <summary>
        /// Formats the output according to the output format given
        /// </summary>
        /// <param name="match">The match object to format</param>
        /// <returns>Output line ready for writing to the display</returns>
        private static string formatOutputLine(IMatch match)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(_outputFormat);
            builder.Replace("{MatchItem}", match.MatchItem);
            builder.Replace("{MatchLineNumber}", match.MatchLineNumber.ToString());
            builder.Replace("{MatchLine}", match.MatchLine);
            builder.Replace("{MatchString}", match.MatchString);

            return builder.ToString();
        }

        /// <summary>
        /// Parse the options passed in.
        /// </summary>
        /// <param name="searchRequest">The Search Request to populate with the options</param>
        /// <param name="args">The options to parse</param>
        /// <returns>True if all is ok and the program can continue, otherwise false</returns>
        private static bool parseOptions(ISearchRequest searchRequest, string[] args)
        {
            var options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            if (parser.ParseArguments(args, options))
            {
                if (options.ShowHelp)
                {
                    CommandLine.Text.HelpText ht = new HelpText("Rummage version 0.1");
                    ht.AddOptions(options);
                    Console.WriteLine(ht.ToString());
                    Console.WriteLine("");
                    return false;
                }

                if (options.SearchStrings != null)
                {
                    foreach (var searchString in options.SearchStrings)
                    {
                        searchRequest.SearchStrings.Add(searchString);
                    }
                }

                if (options.Folders != null)
                {
                    foreach (var folder in options.Folders)
                    {
                        searchRequest.SearchContainers.Add(folder);
                    }

                }

                if (options.IncludeFiles != null)
                {
                    foreach (var inc in options.IncludeFiles)
                    {
                        searchRequest.IncludeItemStrings.Add(inc);
                    }

                }

                if (options.ExcludeFiles != null)
                {
                    foreach (var exc in options.ExcludeFiles)
                    {
                        searchRequest.ExcludeItemStrings.Add(exc);
                    }

                }

                if (options.ExcludeDirectories != null)
                {
                    foreach (var exc in options.ExcludeDirectories)
                    {
                        searchRequest.ExcludeContainerStrings.Add(exc);
                    }

                }

                if (options.IncludeDirectories != null)
                {
                    foreach (var inc in options.IncludeDirectories)
                    {
                        searchRequest.IncludeContainerStrings.Add(inc);
                    }

                }

                searchRequest.CaseSensitive = options.CaseSensitive;
                _verbose = options.Verbose;
                searchRequest.SearchBinaries = options.SearchBinaries;
                _outputFormat = options.OutputFormat;
                _confirmSearch = options.ConfirmSearch;
                return true;
            }

            return true;
        }

        /// <summary>
        /// Load the default values from the default file if it exists
        /// </summary>
        /// <returns></returns>
        private static string[] loadDefaults()
        {
            string defaultOptions = string.Empty;

            if (File.Exists(DEFAULT_FILE))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(DEFAULT_FILE))
                    {
                        defaultOptions = sr.ReadLine();
                        sr.Close();
                    }
                }
                catch (IOException)
                {
                    //Do nothing. If we can't read any defaults it doesn't matter
                }
            }

            defaultOptions = defaultOptions.Trim();
            if (defaultOptions.Length == 0)
            {
                return new string[0];
            }
            else
            {
                return defaultOptions.Split(' ');
            }
        }
    }
}
