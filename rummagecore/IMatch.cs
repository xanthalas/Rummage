namespace RummageCore
{
    public interface IMatch
    {
        /// <summary>
        /// Indicates whether this match was successful
        /// </summary>
        bool Successful { get; set; }

        /// <summary>
        /// String which matched.
        /// </summary>
        string MatchString { get; set; }

        /// <summary>
        /// Line on which the match was found.
        /// </summary>
        string MatchLine { get; set; }

        /// <summary>
        /// Number of the line on which the match was found.
        /// </summary>
        int MatchLineNumber { get; set; }

        /// <summary>
        /// The Item in which the match was found.
        /// </summary>
        string MatchItem { get; set; }

        /// <summary>
        /// If the match was not successful the reason will be held here
        /// </summary>
        string ErrorMessage { get; set; }
    }
}