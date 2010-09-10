using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RummageCore
{
    public interface ISearch
    {
        /// <summary>
        /// Unique identifier for this search
        /// </summary>
        Guid SearchId { get; }

        /// <summary>
        /// Search Request to action.
        /// </summary>
        ISearchRequest SearchRequest { get; set; }


        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        List<IMatch> Matches {get; set; }

        /// <summary>
        /// The number of the file (within the collection of all the files) which is being searched
        /// </summary>
        int ItemNumber { get; set; }

        /// <summary>
        /// Executes the search
        /// </summary>
        /// <param name="searchRequest">The search request to action</param>
        /// <param name="waitForCompletion">If true then the Search method will not return until all searches are complete</param>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        List<IMatch> Search(ISearchRequest searchRequest, bool waitForCompletion);

        /// <summary>
        /// Executes the search using the search request previously assigned
        /// </summary>
        /// <param name="waitForCompletion">If true then the Search method will not return until all searches are complete</param>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        List<IMatch> Search(bool waitForCompletion);

        /// <summary>
        /// Raised when an item search is completed
        /// </summary>
        /// <param name="e"></param>
        event ItemSearchedEventHandler ItemSearched;

        /// <summary>
        /// Cancel the search which is running
        /// </summary>
        void CancelSearch();
    }
}
