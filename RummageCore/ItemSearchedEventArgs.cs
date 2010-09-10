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
        /// The number of the file which has been found (X of Y)
        /// </summary>
        public int FileX { get; set; }

        /// <summary>
        /// The number of the files which will be searched
        /// </summary>
        public int FileOfY { get; set; }


        /// <summary>
        /// Create a new ItemSearchedEventArgs object
        /// </summary>
        /// <param name="url"></param>
        /// <param name="matches"></param>
        public ItemSearchedEventArgs(string url, List<IMatch> matches, int fileX, int ofY)
        {
            Url = url;
            Matches = matches;
            FileX = fileX;
            FileOfY = ofY;
        }
    }
}
