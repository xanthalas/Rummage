using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XanWPFControls
{
    public class DirectoryScanner
    {
        public static DirectoryPathMatch GetMatchingPath(string initialisationString)
        {
            initialisationString =  initialisationString.Trim();
            var directoryPathMatch = new DirectoryPathMatch();

            if (Directory.Exists(initialisationString))
            {
                //If what was passed in is a complete directory then we don't need to do anything 
                directoryPathMatch.BaseDirectory = initialisationString;
                directoryPathMatch.BaseDirectoryExists = true;
                directoryPathMatch.FullMatchingPath = initialisationString;
                directoryPathMatch.PartialDirectoryCharacters = string.Empty;
                return directoryPathMatch;
            }

            //Strip everything from the last backslash to the end of the string.
            var lastBackslashPosition =  initialisationString.LastIndexOf(@"\");
            var completeDirectory = string.Empty;
            var partialPath = string.Empty;

            if (lastBackslashPosition >= 0)
            {
                completeDirectory = initialisationString.Substring(0, lastBackslashPosition);
                partialPath = initialisationString.Substring(completeDirectory.Length + 1).TrimEnd();

                string[] possibleDirectories = null;

                try
                {
                    string directoryToSearch = (completeDirectory.Length == 2 ? completeDirectory + "\\" : completeDirectory);
                    possibleDirectories = Directory.GetDirectories(directoryToSearch, partialPath + "*",
                                                                        SearchOption.TopDirectoryOnly);
                }
                catch (ArgumentException)
                {
                    //If what the user entered is not valid as part of a directory or filename then just drop out
                    return null;
                }
                if (possibleDirectories.Length > 0)
                {
                    foreach (string tail in possibleDirectories)
                    {
                        directoryPathMatch.MatchingTails.Add(tail.Substring(lastBackslashPosition + 1));
                    }
                    directoryPathMatch.MatchingPathTail = directoryPathMatch.MatchingTails[0];
                    directoryPathMatch.BaseDirectory = completeDirectory;
                    directoryPathMatch.PartialDirectoryCharacters = partialPath;
                }
            }

            return directoryPathMatch;
        }
    }
}
