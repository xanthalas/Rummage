using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace RummageCore
{
    /// <summary>
    /// Encapsulates all the information required to perform a search of the filesystem
    /// </summary>
    public class SearchRequestFilesystem : ISearchRequest
    {
        #region Member variables
        // Holds the directories to search.
        public List<string> SearchDirectories { get; set; }

        /// <summary>
        /// Holds the strings to search for.
        /// </summary>
        public List<string> SearchStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against filenames to include those files.
        /// </summary>
        public List<string> IncludeFileStrings { get; set; }

        /// <summary>
        /// Holds thee strings used to match against filenames to exclude those files.
        /// </summary>
        public List<string> ExcludeFileStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against directory names to exclude those directories.
        /// </summary>
        public List<string> ExcludeDirectoryStrings { get; set; }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates whether to seach hidden files and folders.
        /// </summary>
        public bool SearchHidden { get; set; }

        /// <summary>
        /// Indicates whether this Search Request is ready for the search process to begin
        /// </summary>
        private bool _isSearchReady = false;

        /// <summary>
        /// Has the Prepare method been called on this Search yet?
        /// </summary>
        private bool _prepareCalled = false;

        /// <summary>
        /// This list holds all the URLs (in this provider these are filenames (including paths)) to search
        /// </summary>
        private List<String> _urlToSearch;

        #endregion

        /// <summary>
        /// Creates a new Search Request
        /// </summary>
        public SearchRequestFilesystem()
        {
            SearchDirectories = new List<string>();
            SearchStrings = new List<string>();
            IncludeFileStrings = new List<string>();
            ExcludeFileStrings = new List<string>();
            ExcludeDirectoryStrings = new List<string>();
            CaseSensitive = false;
            SearchHidden = false;

            _urlToSearch = new List<string>();
        }

        /// <summary>
        /// Prepares this Search Request. This must be called prior to initiating any search using this request.
        /// </summary>
        /// <returns>True if prepare is successful, otherwise false</returns>
        public bool Prepare()
        {
           if (SearchDirectories.Count == 0 || SearchStrings.Count == 0)
           {
               return false;
           }

            //Now we must build up the list of files to search. This will then be handed off to the search routine via the enumerator
            foreach (string directory in SearchDirectories)
            {
                if (includeThisDirectory(directory))
                {
                    enumerateThisDirectory(directory);
                }
            }

            _prepareCalled = true;

            return true;
        }

        /// <summary>
        /// Gets the contents of the directory passed in and checks them for inclusion/exclusion from the search
        /// </summary>
        /// <param name="directory"></param>
        private void enumerateThisDirectory(string directory)
        {
            // First get all the files in this directory
            var files = from f in new DirectoryInfo(directory).GetFiles() select f;
            foreach (FileInfo fileInfo in files)
            {
                if (includeThisFile(fileInfo.Name))
                {
                    _urlToSearch.Add(fileInfo.FullName);
                }
            }

            //Next get all the directories in this directory and then recurse through them
            var dirs = from d in new DirectoryInfo(directory).GetDirectories() select d;

            foreach (DirectoryInfo dir in dirs)
            {
                if (includeThisDirectory(dir.Name))
                {
                    enumerateThisDirectory(dir.FullName);
                }
            }

            
        }

        /// <summary>
        /// Checks the filename and other attributes to determine whether it should be included in the search list or not
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <returns>True if the file should be included in the search, otherwise false</returns>
        private bool includeThisFile(string filename)
        {
            //First check for includes. Excludes take priority so we'll work out what is included first and then drop
            //out anything which should be excluded.
            bool includeFound = false;

            if (IncludeFileStrings.Count > 0)
            {
                var result = IncludeFileStrings.Select(inclString => Regex.Match(filename, inclString)).Where(m => m.Success);
                

                foreach (System.Text.RegularExpressions.Match m in
                    IncludeFileStrings.Select(inclString => Regex.Match(filename, inclString)).Where(m => m.Success))
                {
                    includeFound = true;
                }
                
                if (!includeFound)
                {
                    return false;
                }
            }

            if (ExcludeFileStrings.Count > 0)
            {
                bool excludeFound = false;

                foreach (string exclString in ExcludeFileStrings)
                {
                    System.Text.RegularExpressions.Match m = Regex.Match(filename, exclString);

                    if (m.Success)
                    {
                        return false;       //This file matches an exclude regex so we won't include it.
                    }
                }

                return ExcludeFileStrings.Select(exclString => Regex.Match(filename, exclString))
                    .All(match => !match.Success);

                /*    Below is the original non-LINQ version of the clause above.
                                foreach (string exclString in ExcludeFileStrings)
                                {
                                    System.Text.RegularExpressions.Match m = Regex.Match(filename, exclString);

                                    if (m.Success)
                                    {
                                        return false;       //This file matches an exclude regex so we won't include it.
                                    }
                                }
                */

            }

            return true;
        }

        /// <summary>
        /// See if this directory should be included or not based on a check of the exclusion regex strings
        /// </summary>
        /// <param name="folder">The name of the directory</param>
        /// <returns>true if this directory should be included in the search, otherwise false</returns>
        private bool includeThisDirectory(string folder)
        {
            if (ExcludeDirectoryStrings.Count == 0)
            {
                return true;
            }

            return ExcludeDirectoryStrings.Select(
                excludeDirectoryString => Regex.Match(folder, excludeDirectoryString))
                .All(match => !match.Success);

            /*    Below is the original non-LINQ version of the clause above.
             
            foreach (string excludeDirectoryString in ExcludeDirectoryStrings)
            {
                System.Text.RegularExpressions.Match match = Regex.Match(folder, excludeDirectoryString);
                if (match.Success)
                {
                    return false;       //This directory matches an exclude regex so we won't include it.
                }
            }
             * 
             */
        }


        /// <summary>
        /// Gets/Sets the set of strings to search for
        /// </summary>
        public List<string> SearchString
        {
            get
            {
                return SearchStrings;
            }
            set
            {
                SearchStrings = value;
            }
        }

        /// <summary>
        /// Returns the list of URLs which will be searched. If the Prepare() method has not been called first 
        /// then this will return null.
        /// </summary>
        public List<string> URL
        {
            get
            {
                if (_prepareCalled)
                {
                    return _urlToSearch;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
