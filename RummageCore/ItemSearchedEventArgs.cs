using System;
using System.Collections.Generic;

namespace RummageCore
{
    /// <summary>
    /// Event Args class for the ItemSearched event.
    /// </summary>
    public class ItemSearchedEventArgs : EventArgs
    {
        /// <summary>
        /// URL of the item which was searched
        /// </summary>
        public string Url { get; private set; }

        /// <summary>
        /// List of matches found in this item
        /// </summary>
        public List<IMatch> Matches { get; private set; }

        /// <summary>
        /// Create a new ItemSearchedEventArgs object
        /// </summary>
        /// <param name="url"></param>
        /// <param name="matches"></param>
        public ItemSearchedEventArgs(string url, List<IMatch> matches)
        {
            Url = url;
            Matches = matches;
        }
    }
}
