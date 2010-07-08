using System;
using System.Collections.Generic;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Encapsulates all the information required to perform a search.
    /// </summary>
    public class SearchRequest
    {
        #region Member variables
        // Holds the folders to search.
        public List<string> SearchFolders { get; set; }

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

        #endregion

        /// <summary>
        /// Creates a new Search Request
        /// </summary>
        public SearchRequest()
        {
            SearchFolders = new List<string>();
            SearchStrings = new List<string>();
            IncludeFileStrings = new List<string>();
            ExcludeFileStrings = new List<string>();
            ExcludeDirectoryStrings = new List<string>();
            CaseSensitive = false;
            SearchHidden = false;
        }
    }
}
