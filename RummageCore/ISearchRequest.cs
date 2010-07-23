using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Specifies all the information required to conduct a search
    /// </summary>
    public interface ISearchRequest
    {
        #region Member variables
        // Holds details of the containers to search.
        List<string> SearchContainers { get; set; }

        /// <summary>
        /// Holds the strings to search for.
        /// </summary>
        List<string> SearchStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against item names to include those items in the search.
        /// </summary>
        List<string> IncludeItemStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against item names to exclude those items.
        /// </summary>
        List<string> ExcludeItemStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against container names to exclude those containers.
        /// </summary>
        List<string> ExcludeContainerStrings { get; set; }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates whether to seach hidden items.
        /// </summary>
        bool SearchHidden { get; set; }

        #endregion

        /// <summary>
        /// Prepares this Search Request. This must be called prior to initiating any search using this request.
        /// </summary>
        /// <returns>True if prepare is successful, otherwise false</returns>
        bool Prepare();


        /// <summary>
        /// Contains a list of strings or Regexes to search for
        /// </summary>
        List<String> SearchString { get; set; }

        /// <summary>
        /// Returns a list of all the URLs which are eligible to search
        /// </summary>
        List<string> URL { get; }
    }
}
