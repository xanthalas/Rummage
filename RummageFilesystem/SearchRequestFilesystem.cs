using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Permissions;
using RummageCore;

namespace RummageFilesystem
{
    /// <summary>
    /// Encapsulates all the information required to perform a search of the filesystem
    /// </summary>
    public class SearchRequestFilesystem : ISearchRequest
    {
        //Declare the logger
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Member variables

        public Guid SearchRequestId { get; private set; }

        /// <summary>
        /// Holds details of the containers to search. For the filesystem these containers are directories.
        /// </summary>
        public List<string> SearchContainers { get; set; }

        /// <summary>
        /// Holds the strings to search for.
        /// </summary>
        public List<string> SearchStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against item names to include those items in the search. For the filesystem these items are files.
        /// </summary>
        public List<string> IncludeItemStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against item names to exclude those items. For the filesystem these items are files.
        /// </summary>
        public List<string> ExcludeItemStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against container names to include those containers. For the filesystem these containers are directories.
        /// </summary>
        public List<string> IncludeContainerStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against container names to exclude those containers. For the filesystem these containers are directories.
        /// </summary>
        public List<string> ExcludeContainerStrings { get; set; }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates whether to seach hidden files and folders.
        /// </summary>
        public bool SearchHidden { get; set; }

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        public bool SearchBinaries { get; set; }

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        public bool Recurse { get; set; }

        /// <summary>
        /// Indicates whether this Search Request is ready for the search process to begin
        /// </summary>
        private bool _isSearchReady = false;

        /// <summary>
        /// This list holds all the URLs (in this provider these are filenames (including paths)) to search
        /// </summary>
        private List<String> _urlToSearch;

        /// <summary>
        /// Indicates whether this search has been prepared - and can be used - or not.
        /// </summary>
        private bool _isPrepared = false;

        #endregion

        /// <summary>
        /// Creates a new Search Request
        /// </summary>
        public SearchRequestFilesystem()
        {
            SearchRequestId = Guid.NewGuid();
            SearchContainers = new List<string>();
            SearchStrings = new List<string>();
            IncludeItemStrings = new List<string>();
            ExcludeItemStrings = new List<string>();
            IncludeContainerStrings = new List<string>();
            ExcludeContainerStrings = new List<string>();
            CaseSensitive = false;
            SearchHidden = false;
            Recurse = false;        //Don't recurse unless told to

            _urlToSearch = new List<string>();
        }

        /// <summary>
        /// Indicates whether this search request has been prepared. Until it is prepared it cannot be used in a search.
        /// </summary>
        public bool IsPrepared
        {
            get { return _isPrepared; }
        }

        /// <summary>
        /// Prepares this Search Request. This must be called prior to initiating any search using this request.
        /// </summary>
        /// <returns>The number of files which will be searched if Search() is run using this request</returns>
        public int Prepare()
        {
           if (log.IsDebugEnabled)
           {
               log.Debug("Searching for:");
               foreach (string s in SearchStrings)
               {
                   log.Debug("    " + s);
               }
               log.Debug("Searching the following folders:");
               foreach (string s in SearchContainers)
               {
                   log.Debug("    " + s);
               }
               log.Debug("Using the following file include strings:");
               foreach (string s in IncludeItemStrings)
               {
                   log.Debug("    " + s);
               }
               log.Debug("Using the following file exclude strings:");
               foreach (string s in ExcludeItemStrings)
               {
                   log.Debug("    " + s);
               }
               log.Debug("Using the following directory include strings:");
               foreach (string s in IncludeContainerStrings)
               {
                   log.Debug("    " + s);
               }
               log.Debug("Using the following directory exclude strings:");
               foreach (string s in ExcludeContainerStrings)
               {
                   log.Debug("    " + s);
               }
           }

           if (SearchContainers.Count == 0 || SearchStrings.Count == 0)
           {
               return 0;
           }

            //Now we must build up the list of files to search. This will then be handed off to the search routine via the enumerator
            foreach (string directory in SearchContainers)
            {
                enumerateThisDirectory(directory);
            }

            _isPrepared = true;

            return Urls.Count;
        }

        /// <summary>
        /// Gets the contents of the directory passed in and checks them for inclusion/exclusion from the search
        /// </summary>
        /// <param name="directory"></param>
        private void enumerateThisDirectory(string directory)
        {
            // First get all the files in this directory
            try
            {
                var files = from f in new DirectoryInfo(directory).GetFiles() select f;

                foreach (FileInfo fileInfo in files)
                {
                    if (includeThisFile(fileInfo.Name, fileInfo.FullName))
                    {
                        _urlToSearch.Add(fileInfo.FullName);
                    }
                }

                //Next get all the directories in this directory and then recurse through them - if recurse is on
                if (Recurse)
                {
                    var dirs = from d in new DirectoryInfo(directory).GetDirectories() select d;

                    foreach (DirectoryInfo dir in dirs)
                    {
                        if (includeThisDirectory(dir.Name))
                        {
                            enumerateThisDirectory(dir.FullName);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                log.Debug("Unauthorized access exception on " + directory);
                return;     //If we can't access this directory then we'll just ignore it
            }
            catch (DirectoryNotFoundException)
            {
                log.Debug("Cannot find directory: " + directory);
                return;     //If we can't access this directory then we'll just ignore it
            }
        }

        /// <summary>
        /// Checks the filename and other attributes to determine whether it should be included in the search list or not
        /// </summary>
        /// <param name="filename">The name of the file to check</param>
        /// <param name="fullFilename">The path and filename of the file to check</param>
        /// <returns>True if the file should be included in the search, otherwise false</returns>
        private bool includeThisFile(string filename, string fullFilename)
        {
            //First check for includes. Excludes take priority so we'll work out what is included first and then drop
            //out anything which should be excluded.
            bool includeFound = false;

            if (IncludeItemStrings.Count > 0)
            {
                var result = IncludeItemStrings.Select(inclString => Regex.Match(filename, inclString, RegexOptions.IgnoreCase)).Where(m => m.Success);
                

                foreach (System.Text.RegularExpressions.Match m in
                    IncludeItemStrings.Select(inclString => Regex.Match(filename, inclString, RegexOptions.IgnoreCase)).Where(m => m.Success))
                {
                    log.Debug("Including file " + filename);
                    includeFound = true;
                }
                
                if (!includeFound)
                {
                    return false;
                }
            }

            if (ExcludeItemStrings.Count > 0)
            {
                bool excludeFound = false;

                foreach (string exclString in ExcludeItemStrings)
                {
                    System.Text.RegularExpressions.Match m = Regex.Match(filename, exclString);

                    if (m.Success)
                    {
                        log.Debug("Excluding file " + filename);
                        return false;       //This file matches an exclude regex so we won't include it.
                    }
                }
            }

            //See if we are searching binary files and if not, check whether this file is binary
            if (!SearchBinaries)
            {
                if (isFileBinary(fullFilename))
                {
                    log.Debug("Excluding binary file " + filename);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// See if this directory should be included or not based on a check of the exclusion regex strings
        /// </summary>
        /// <param name="folder">The name of the directory</param>
        /// <returns>true if this directory should be included in the search, otherwise false</returns>
        private bool includeThisDirectory(string folder)
        {
            if (ExcludeContainerStrings.Count == 0 && IncludeContainerStrings.Count == 0)
            {
                return true;
            }

            bool result = ExcludeContainerStrings.Select(
                excludeDirectoryString => Regex.Match(folder, excludeDirectoryString,RegexOptions.IgnoreCase))
                .All(match => !match.Success);

            if (!result)
            {
                return false;
            }

            result = IncludeContainerStrings.Select(
                includeDirectoryString => Regex.Match(folder, includeDirectoryString, RegexOptions.IgnoreCase))
                .All(match => match.Success);

            return result;
        }


        /// <summary>
        /// Determine if file contains binary data (i.e. a jpeg or mp3 file)
        /// </summary>
        /// <remarks>This is pretty crude. It counts the number of null characters found in the first 64 bytes
        /// and figures the file is binary if it finds more than 4.</remarks>
        /// <param name="file">File to check</param>
        private bool isFileBinary(string file)
        {
            int nullCount = 0;

            try
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    char[] buffer = new char[64];
                    int count = reader.ReadBlock(buffer, 0, 64);
                    for (int index = 0; index < count; index++)
                    {
                        if (buffer[index] == (char) 00)
                        {
                            nullCount++;
                        }
                    }
                }
                if (nullCount > 4) //Arbitrary value. Might need to be adjusted
                {
                    return true;
                }
            }
            catch(IOException ioe)
            {
                //This is a bit crude but we'll assume that if there is a problem reading the file then we 
                //can't search it anyway so we'll set it to not-binary and let it fail during the main search
                return false;
            }
            return false;
        }


        /// <summary>
        /// Returns the list of URLs which will be searched. If the Prepare() method has not been called first 
        /// then this will return null.
        /// </summary>
        public List<string> Urls
        {
            get
            {
                if (_isPrepared)
                {
                    return _urlToSearch;
                }
                else
                {
                    return null;
                }
            }
        }

    }
}
