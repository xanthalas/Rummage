using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    public interface ISearch
    {
        /// <summary>
        /// Search Request to action.
        /// </summary>
        ISearchRequest SearchRequest { get; set; }


        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        List<IMatch> Matches {get; set; }

        /// <summary>
        /// Executes the search
        /// </summary>
        /// <param name="searchRequest">The search request to action</param>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        List<IMatch> Search(ISearchRequest searchRequest);

        /// <summary>
        /// Executes the search using the search request previously assigned
        /// </summary>
        /// <returns>A collection of IMatch objects holding the result of the search</returns>
        List<IMatch> Search();

    }
}
