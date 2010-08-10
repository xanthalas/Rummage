using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using RummageCore;
using System.IO;
using RE = System.Text.RegularExpressions;

namespace RummageFilesystem
{
    /// <summary>
    /// Performs a search
    /// </summary>
    public class SearchFilesystem : ISearch
    {
        #region Member variables
        /// <summary>
        /// Search Request to action.
        /// </summary>
        public ISearchRequest SearchRequest { get; set; }

        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        public List<IMatch> Matches { get; set; }

        /// <summary>
        /// Event raised when the searching of a given item is complete
        /// </summary>
        public event ItemSearchedEventHandler ItemSearched;

        protected virtual void OnFileSearched(EventArgs e)
        {
            if (ItemSearched != null)
            {
                ItemSearched(this, e);
            }
        }


        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private ProgressReporter progressReporter;

        #endregion


        /// <summary>
        /// Executes the search
        /// </summary>
        /// <param name="searchRequestFilesystem">The search request to action</param>
        /// <param name="waitForCompletion">If true then the Search method will not return until all searches are complete</param>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        public List<IMatch> Search(ISearchRequest searchRequestFilesystem, bool waitForCompletion)
        {
            SearchRequest = searchRequestFilesystem;
            Matches = new List<IMatch>();
            
            //Build a list of Regexes corresponding to the regex strings to be searched for
            List<RE.Regex> regexes = new List<RE.Regex>(searchRequestFilesystem.SearchStrings.Count);
            if (SearchRequest.CaseSensitive)
            {
                regexes.AddRange(searchRequestFilesystem.SearchStrings.Select(searchString => new RE.Regex(searchString)));
            }
            else
            {
                regexes.AddRange(searchRequestFilesystem.SearchStrings.Select(searchString => new RE.Regex(searchString, RegexOptions.IgnoreCase)));                
            }

            this.cancellationTokenSource = new CancellationTokenSource();
            cancellationToken = this.cancellationTokenSource.Token;
            progressReporter = new ProgressReporter();
            List<Task> tasks = new List<Task>();
            bool allTasksCompleted = false;

            foreach (string url in searchRequestFilesystem.Urls)
            {
                searchThisUrlInBackground(regexes, url, tasks);
            }

            Task.Factory.ContinueWhenAll(tasks.ToArray(),
                  result =>
                      {
                          allTasksCompleted = true;
                      });

            while (waitForCompletion && !allTasksCompleted)
            {
                //Do nothing - just wait
            }
            return Matches;
        }

        private void searchThisUrlInBackground(List<Regex> regexes, string url, List<Task> tasksArray)
        {
            var task = Task.Factory.StartNew(() =>
                                                 {
                                                     List<RummageCore.IMatch> matchesInThisFile = new List<IMatch>();

                                                     matchesInThisFile = searchFile(regexes, url);
                                                     Matches.AddRange(matchesInThisFile);
                                                     OnFileSearched(new ItemSearchedEventArgs(url, matchesInThisFile));
                                                 }, cancellationToken);


            tasksArray.Add(task);
        }

        private List<RummageCore.IMatch> searchFile(List<RE.Regex> regexes, string url)
        {
            List<RummageCore.IMatch> matchesInThisFile = new List<IMatch>();

            if (!File.Exists(url))
            {
                RummageCore.Match failedMatch = new RummageCore.Match("", "", 0, url);
                failedMatch.Successful = false;
                failedMatch.ErrorMessage = String.Format("File {0} cannot be searched as it no longer exists.", url);
                matchesInThisFile.Add(failedMatch);                
            }
            else
            {
                try
                {
                    using (StreamReader reader = new StreamReader(url))
                    {
                        int lineNumber = 0;

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            lineNumber++;

                            var result = searchLine(line, regexes);
                            if (result != null)
                            {
                                matchesInThisFile.Add(new RummageCore.Match(result.ToString(), line, lineNumber, url));
                            }
                        }
                    }
                }
                catch (IOException ioe)
                {
                    RummageCore.Match failedMatch = new RummageCore.Match("", "", 0, url);
                    failedMatch.Successful = false;
                    failedMatch.ErrorMessage = ioe.Message;
                    matchesInThisFile.Add(failedMatch);
                }
            }
            return matchesInThisFile;
        }

        /// <summary>
        /// Searches the line passed in to see if it matches with any of the regular expressions in the list
        /// </summary>
        /// <param name="line">The line to search</param>
        /// <param name="regexes">The list of regular expressions to look for</param>
        /// <returns>The Regex which matched or null if no match is found</returns>
        private RE.Regex searchLine(string line, List<RE.Regex> regexes)
        {
            foreach (RE.Regex regex in regexes)
            {
                RE.Match match = regex.Match(line);
                if (match.Success)
                {
                    return regex;
                }
            }

            return null;
        }


        /// <summary>
        /// Executes the search
        /// </summary>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        /// <param name="waitForCompletion">If true then the Search method will not return until all searches are complete</param>
        public List<IMatch> Search(bool waitForCompletion)
        {
            if (SearchRequest == null)
            {
                throw new ArgumentNullException("SearchRequest", "Search requested but no SearchRequest has been specified");
            }

            if (!SearchRequest.IsPrepared)
            {
                throw new ArgumentException("Prepare() has not yet been called on this search request");
            }

            return Search(SearchRequest, waitForCompletion);

        }

        /// <summary>
        /// Check if the URL passed in contains any of the filter values in the Include list
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>true if it includes one of the filter strings, otherwise false</returns>
        private bool itemIncludeStrings(string url)
        {
            foreach (string s in SearchRequest.IncludeItemStrings)
            {
                if (url.Contains(s))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Check if the URL passed in contains any of the filter values in the Exclude list
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>true if it includes one of the filter strings, otherwise false</returns>
        private bool itemExcludeStrings(string url)
        {
            foreach (string s in SearchRequest.ExcludeItemStrings)
            {
                if (url.Contains(s))
                {
                    return true;
                }
            }

            return false;
        }

    }
}