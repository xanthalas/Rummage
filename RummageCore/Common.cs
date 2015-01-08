using System;

/******************************************************************************************************************
 * Contains common constructs required throughout the the system.                                                 *
 *****************************************************************************************************************/
 namespace RummageCore
{
    /// <summary>
    /// Item Searched Delegate
    /// </summary>
    public delegate void ItemSearchedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Notify Progress Delegate
    /// </summary>
    public delegate void NotifyProgressEventHandler(object sender, EventArgs e);

    /// <summary>
    /// Indicates the type of container to be searched
    /// </summary>
    public enum SearchContainerType
    {
        /// <summary>
        /// Search of a filesystem
        /// </summary>
        Filesystem,

        /// <summary>
        /// Search of a database
        /// </summary>
        Database
    }

}
