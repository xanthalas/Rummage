﻿using System;
using System.Collections.Generic;
using System.Data.Linq;

namespace RummageCore
{
    /// <summary>
    /// Specifies all the information required to conduct a search
    /// </summary>
    public interface ISearchRequest
    {
        #region Member variables

        /// <summary>
        /// Identifier for this search request as provided by storage system.
        /// </summary>
        int Id { get; set; }

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
        bool Recurse { get; set; }


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

        /// <summary>
        /// Cancel the search request which is being prepared
        /// </summary>
        void CancelRequest();

        /// <summary>
        /// Raised at pre-determined times to notify progress
        /// </summary>
        /// <param name="e"></param>
        event NotifyProgressEventHandler NotifyProgress;

        /// <summary>
        ///  Add a container to search
        /// </summary>
        void AddSearchContainer(string container);

        /// <summary>
        /// Add a string to search for.
        /// </summary>
        void AddSearchString(string searchString);

        /// <summary>
        /// Add an include string used to match against item names to include those items in the search.
        /// </summary>
        void AddIncludeItemString(string includeItem);

        /// <summary>
        /// Add an exclude string used to match against item names to exclude those items.
        /// </summary>
        void AddExcludeItemString(string excludeItem);

        /// <summary>
        /// Add an include string used to match against container names to include those containers.
        /// </summary>
        void AddIncludeContainerString(string includeContainer);

        /// <summary>
        /// Add an exclude string used to match against container names to exclude those containers.
        /// </summary>
        void AddExcludeContainerString(string excludeContainer);

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        void SetCaseSensitive(bool caseSensitive);

        /// <summary>
        /// Indicates whether to search hidden items.
        /// </summary>
        void SetSearchHidden(bool searchHidden);

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        void SetSearchBinaries(bool searchBinaries);

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        void SetRecurse(bool recurse);

        /// <summary>
        /// Save the search request to the url specified
        /// </summary>
        /// <param name="url">The url to save the request to. </param>
        /// <returns>True if the save was successful, otherwise False</returns>
        bool SaveSearchRequest(string url);

        /// <summary>
        /// Load a search request from the url specified
        /// </summary>
        /// <param name="url">The url of the request to load</param>
        /// <returns>A loaded ISearchRequest object</returns>
        ISearchRequest LoadSearchRequest(string url);
    }
}
