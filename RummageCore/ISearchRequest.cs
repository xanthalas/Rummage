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
    interface ISearchRequest
    {
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
