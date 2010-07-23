using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Performs a search
    /// </summary>
    class Search: ISearch
    {
        #region Member variables
        /// <summary>
        /// Search Request to action.
        /// </summary>
        public ISearchRequest SearchRequest {get; set;}

        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        public List<IMatch> Matches {get; set; }

        #endregion

        /// <summary>
        /// Creates a new Search
        /// </summary>
        /// <param name="searchRequestFilesystem"></param>
        public Search(ISearchRequest searchRequestFilesystem)
        {
            SearchRequest = searchRequestFilesystem;
            Matches = new List<IMatch>();
        }
    }

}
