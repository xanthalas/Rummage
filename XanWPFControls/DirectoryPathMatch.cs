using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace XanWPFControls
{
    /// <summary>
    /// Holds details of a directory path match
    /// </summary>
    public class DirectoryPathMatch
    {
        /// <summary>
        /// Get/Set to part of the path which is a complete directory
        /// </summary>
        public string BaseDirectory { get; set; }

        /// <summary>
        /// Get the complete matching path. If there is no match this will be empty.
        /// </summary>
        public string FullMatchingPath { get; set; }

        /// <summary>
        /// Returns only the last part of the path - the bit which matches with the characters passed in
        /// </summary>
        public string MatchingPathTail { get; set; }

        /// <summary>
        /// If this is false then all other information in this object cannot be relied upon
        /// </summary>
        public bool BaseDirectoryExists { get; set; }

        /// <summary>
        /// The partial characters entered by the user and used to find matches
        /// </summary>
        public string PartialDirectoryCharacters { get; set; }

        /// <summary>
        /// All the matching tails for the the characters passed in
        /// </summary>
        public List<string> MatchingTails { get; set; }

        public DirectoryPathMatch()
        {
            BaseDirectory = string.Empty;
            FullMatchingPath = string.Empty;
            MatchingPathTail = string.Empty;
            BaseDirectoryExists = false;
            MatchingTails = new List<string>();
        }


        public DirectoryPathMatch(string fullMatchingPath, string matchingPathTail, int numberOfMatchingPaths)
        {
            FullMatchingPath = fullMatchingPath;
            MatchingPathTail = matchingPathTail;
            BaseDirectoryExists = Directory.Exists(fullMatchingPath);
        }
    }
}
