namespace RummageCore
{
    /// <summary>
    /// Holds the result of a single match.
    /// </summary>
    public class Match : IMatch
    {
        #region Member variables
        /// <summary>
        /// Indicates whether this match was successful
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// String which matched.
        /// </summary>
        public string MatchString {get; set;}

        /// <summary>
        /// Line on which the match was found.
        /// </summary>
        public string MatchLine {get; set;}

        /// <summary>
        /// Number of the line on which the match was found.
        /// </summary>
        public int MatchLineNumber {get; set;}

        /// <summary>
        /// The Item in which the match was found.
        /// </summary>
        public string MatchItem {get; set;}

        /// <summary>
        /// If the match was not successful the reason will be held here
        /// </summary>
        public string ErrorMessage { get; set; }

        #endregion

        /// <summary>
        /// Create a new Match object
        /// </summary>
        /// <param name="matchString">The string which matched during the search</param>
        /// <param name="matchLine">The line containing the matching string</param>
        /// <param name="lineNumber">The line number of the line</param>
        /// <param name="file">The file in which the matching string was found</param>
        public Match(string matchString, string matchLine, int lineNumber, string file)
        {
            MatchString = matchString;
            MatchLine = matchLine;
            MatchLineNumber = lineNumber;
            MatchItem = file;
            Successful = true;
            ErrorMessage = string.Empty;
        }
    }
}
