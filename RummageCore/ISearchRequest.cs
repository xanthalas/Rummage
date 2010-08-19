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

        /// <summary>
        /// Unique identifier for this search request
        /// </summary>
        Guid SearchRequestId { get; }

        /// <summary>
        ///  Holds details of the containers to search.
        /// </summary>
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
        /// Holds the strings used to match against container names to include those containers.
        /// </summary>
        List<string> IncludeContainerStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against container names to exclude those containers.
        /// </summary>
        List<string> ExcludeContainerStrings { get; set; }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates whether to search hidden items.
        /// </summary>
        bool SearchHidden { get; set; }

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        bool SearchBinaries { get; set; }

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        bool NoRecurse { get; set; }


        /// <summary>
        /// Indicates whether this search request has been prepared. Until it is prepared it cannot be used in a search.
        /// </summary>
        bool IsPrepared { get; }

        /// <summary>
        /// Returns a list of all the URLs which are eligible to search
        /// </summary>
        List<string> Urls { get; }

        #endregion

        /// <summary>
        /// Prepares this Search Request. This must be called prior to initiating any search using this request.
        /// </summary>
        /// <returns>The number of items which will be searched if Search() is run using this request</returns>
        int Prepare();

    }
}
