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
        public SearchRequestFilesystem RequestFilesystem {get; set;}

        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        public List<Match> Matches {get; private set; }

        #endregion

        /// <summary>
        /// Creates a new Search
        /// </summary>
        /// <param name="searchRequestFilesystem"></param>
        public Search(SearchRequestFilesystem searchRequestFilesystem)
        {
            RequestFilesystem = searchRequestFilesystem;
            Matches = new List<Match>();
        }
    }
}
