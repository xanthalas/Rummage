namespace RummageCore
{
    public interface IMatch
    {
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
    }
}