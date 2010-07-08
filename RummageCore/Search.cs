using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Performs a search
    /// </summary>
    class Search
    {
        #region Member variables
        /// <summary>
        /// Search Request to action.
        /// </summary>
        public SearchRequest Request {get; set;}

        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        public List<Match> Matches {public get; private set; }

        #endregion

        /// <summary>
        /// Creates a new Search
        /// </summary>
        /// <param name="searchRequest"></param>
        public Search(SearchRequest searchRequest)
        {
            Request = searchRequest;
            Matches = new List<Match>();
        }
    }
}
