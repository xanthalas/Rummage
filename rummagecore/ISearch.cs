using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    interface ISearch
    {
        /// <summary>
        /// Search Request to action.
        /// </summary>
        ISearchRequest SearchRequest { get; set; }


        /// <summary>
        /// Collection of matches from this search
        /// </summary>
        List<IMatch> Matches {get; set; }

    }
}
