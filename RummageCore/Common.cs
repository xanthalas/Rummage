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
}
