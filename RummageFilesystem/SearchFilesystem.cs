using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RummageCore;
using System.IO;
using Match = System.Text.RegularExpressions.Match;
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

        #endregion


        /// <summary>
        /// Executes the search
        /// </summary>
        /// <param name="searchRequestFilesystem">The search request to action</param>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        public List<IMatch> Search(ISearchRequest searchRequestFilesystem)
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

            foreach (string url in searchRequestFilesystem.URL)
            {
                if (File.Exists(url))
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
                                Matches.Add(new RummageCore.Match(result.ToString(), line, lineNumber, url));
                            }
                        }
                    }
                }
            }

            return Matches;
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
        public List<IMatch> Search()
        {
            if (SearchRequest == null)
            {
                throw new ArgumentNullException("SearchRequest", "Search requested but no SearchRequest has been specified");
            }

            if (!SearchRequest.IsPrepared)
            {
                throw new ArgumentException("Prepare() has not yet been called on this search request");
            }

            return Search(SearchRequest);

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