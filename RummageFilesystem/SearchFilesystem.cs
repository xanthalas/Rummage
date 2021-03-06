﻿using System;
using System.Collections.Generic;
using System.Data.Linq;
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
        /// Unique identifier for this search
        /// </summary>
        public Guid SearchId { get; private set; }

        /// <summary>
        /// Search Request to action.
        /// </summary>
        public ISearchRequest SearchRequest { get; set; }

        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        public List<IMatch> Matches { get; set; }

        /// <summary>
        /// The number of the file (within the collection of all the files) which is being searched
        /// </summary>
        public int ItemNumber { get; set; }

        /// <summary>
        /// Event raised when the searching of a given item is complete
        /// </summary>
        public event ItemSearchedEventHandler ItemSearched;

        #endregion

        /// <summary>
        /// Used to indicate this search that it will be cancelled
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;
        private CancellationToken cancellationToken;
        private ProgressReporter progressReporter;


        /// <summary>
        /// Create a new SearchFilesystem object
        /// </summary>
        public SearchFilesystem()
        {
            ItemNumber = 0;
            SearchId = Guid.NewGuid();
            this.cancellationTokenSource = new CancellationTokenSource();
        }


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

            //Before kicking off the search we will save it to the database
            SaveSearchRequest(SearchRequest, SearchContainerType.Filesystem);

            cancellationToken = this.cancellationTokenSource.Token;
            progressReporter = new ProgressReporter();
            List<Task> tasks = new List<Task>();
            bool allTasksCompleted = false;

            if (searchRequestFilesystem.Urls.Count > 0)
            {
                foreach (string url in searchRequestFilesystem.Urls)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }
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
                                                     OnFileSearched(new ItemSearchedEventArgs(url, matchesInThisFile, ItemNumber, SearchRequest.Urls.Count));
                                                 }, cancellationToken);


            tasksArray.Add(task);
        }

        private List<RummageCore.IMatch> searchFile(List<RE.Regex> regexes, string url)
        {
            ItemNumber++;

            List<RummageCore.IMatch> matchesInThisFile = new List<IMatch>();

            if (!File.Exists(url))
            {
                RummageCore.Match failedMatch = new RummageCore.Match("", "", 0, url);
                failedMatch.Successful = false;
                failedMatch.ErrorMessage = String.Format("File {0} cannot be searched as it no longer exists.", url);
                //matchesInThisFile.Add(failedMatch);  
                matchesInThisFile.AddWithNullcheck(failedMatch);
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
                                //matchesInThisFile.Add(new RummageCore.Match(result.ToString(), line, lineNumber, url));
                                matchesInThisFile.AddWithNullcheck(new RummageCore.Match(result.ToString(), line, lineNumber, url));
                            }
                        }
                    }
                }
                catch (IOException ioe)
                {
                    RummageCore.Match failedMatch = new RummageCore.Match("", "", 0, url);
                    failedMatch.Successful = false;
                    failedMatch.ErrorMessage = string.Format("Error: Couldn't search file {0}. Reason: {1} ", url, ioe.Message);
                    //matchesInThisFile.Add(failedMatch);
                    matchesInThisFile.AddWithNullcheck(failedMatch);
                }
                catch (Exception ex)
                {
                    RummageCore.Match failedMatch = new RummageCore.Match("", "", 0, url);
                    failedMatch.Successful = false;
                    failedMatch.ErrorMessage = string.Format("Error: Couldn't search file {0}. Reason: {1} ", url, ex.Message);
                    //matchesInThisFile.Add(failedMatch);
                    matchesInThisFile.AddWithNullcheck(failedMatch);
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

        protected virtual void OnFileSearched(EventArgs e)
        {
            if (ItemSearched != null)
            {
                ItemSearched(this, e);
            }
        }

        /// <summary>
        /// Cancel the search which is running
        /// </summary>
        public void CancelSearch()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Save the details of the Search Request.
        /// </summary>
        /// <param name="searchRequest">The Search Request to save</param>
        /// <param name="searchContainerType">The type of container being searched</param>
        /// <returns>The id of the request which was just stored</returns>
        public int SaveSearchRequest(ISearchRequest searchRequest, SearchContainerType searchContainerType)
        {

            return 0;
        }

    }

    /// <summary>
    /// Added in order to help trap an issue with matches occasionally being set to Null.
    /// </summary>
    public static class ListTExtensions
    {
        public static void AddWithNullcheck<T>(this List<T> set, T item) where T : class
        {
            if (item == null)
            {
                throw new NullReferenceException("Attempting to add null item");
            }
            else
            {
                set.Add(item);
            }
        }
    }
}