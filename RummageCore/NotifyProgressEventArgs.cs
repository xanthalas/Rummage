using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Event Args used when notifying progress
    /// </summary>
    public class NotifyProgressEventArgs : EventArgs
    {
        /// <summary>
        /// The number of the files which have been scanned
        /// </summary>
        public int NumberOfFilesScanned { get; set; }

                /// <summary>
        /// Create a new ItemSearchedEventArgs object
        /// </summary>
        /// <param name="url"></param>
        /// <param name="matches"></param>
        public NotifyProgressEventArgs(int fileCount)
        {
            NumberOfFilesScanned = fileCount;
        }

    }
}
