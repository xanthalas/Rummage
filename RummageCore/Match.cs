using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RummageCore
{
    /// <summary>
    /// Holds the result of a single match.
    /// </summary>
    public class Match
    {
        #region Member variables
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
        /// File in which the match was found
        /// </summary>
        public string MatchFile {get; set;}

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
            MatchFile = file;
        }
    }
}
