using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommandLine;
using RummageCore;
using RummageFilesystem;

namespace rmg
{
    class Program
    {
        private static bool _verbose = false;

        static void Main(string[] args)
        {
            ISearchRequest searchRequest = new SearchRequestFilesystem();

            parseOptions(searchRequest, args);

            if (_verbose) {Console.WriteLine("Starting search.");}

            searchRequest.Prepare();
            if (searchRequest.IsPrepared)
            {
                if (_verbose) { Console.WriteLine("{0} files will be searched", searchRequest.Urls.Count); }
                ISearch search = new SearchFilesystem();
                search.Search(searchRequest);
                
                foreach (IMatch match in search.Matches)
                {
                    Console.WriteLine("{0}:{1} {2}", match.MatchItem, match.MatchLineNumber, match.MatchLine);
                }
                if (_verbose) { Console.WriteLine("Search complete. Found {0} matches.", search.Matches.Count); }
            }
            else
            {
                Console.WriteLine("Not enough information has been supplied to perform the search.");
            }
        }

        private static void parseOptions(ISearchRequest searchRequest, string[] args)
        {

            var options = new Options();
            ICommandLineParser parser = new CommandLineParser();
            if (parser.ParseArguments(args, options))
            {
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
                        searchRequest.IncludeItemStrings.Add(exc);
                    }

                }

                searchRequest.CaseSensitive = options.CaseSensitive;
                _verbose = options.Verbose;
                searchRequest.SearchBinaries = options.SearchBinaries;
            }
        }
    }
}
