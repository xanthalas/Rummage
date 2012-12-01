using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Permissions;
using System.Threading;
using System.Data.Linq;
using System.Data.Linq.Mapping;
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

        private static readonly string[] KNOWN_BINARY_FILENAME_REGEXES = new string[17]
                                                                   {
                                                                       @".*\.exe$", @".*\.dll$", @".*\.pdb$",
                                                                       @".*\.ico$",
                                                                       @".*\.jpg$", @".*\.jpeg$", @".*\.gif$",
                                                                       @".*\.png$", @".*\.mp3$", @".*\.wma$",
                                                                       @".*\.mpeg$", @".*\.mp4$", @".*\.mov$",
                                                                       @".*\.avi$", @".*\.zip$", @".*\.7z$", @".*\.msi$"
                                                                   };
        #region Member variables

        /// <summary>
        /// Holds details of the containers to search. For the filesystem these containers are directories.
        /// </summary>
        private List<string> _searchContainers;

        /// <summary>
        /// Holds the strings used to match against item names to include those items in the search. For the filesystem these items are files.
        /// </summary>
        private List<string> _includeItemStrings;

        /// <summary>
        /// Holds the strings used to match against item names to exclude those items. For the filesystem these items are files.
        /// </summary>
        private List<string> _excludeItemStrings;

        /// <summary>
        /// Holds the strings used to match against container names to include those containers. For the filesystem these containers are directories.
        /// </summary>
        private List<string> _includeContainerStrings;

        /// <summary>
        /// Holds the strings used to match against container names to exclude those containers. For the filesystem these containers are directories.
        /// </summary>
        private List<string> _excludeContainerStrings;

        /// <summary>
        /// Indicates whether to seach hidden files and folders.
        /// </summary>
        private bool _searchHidden = false;

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        private bool _searchBinaries = false;

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        private bool _searchRecursively = true;

        /// <summary>
        /// Identifier for this search request as provided by storage system.
        /// </summary>
        public int Id { get; set; }

        public Guid SearchRequestId { get; private set; }

        /// <summary>
        /// Holds details of the containers to search. For the filesystem these containers are directories.
        /// </summary>
        public List<string> SearchContainers
        {
            get { return _searchContainers;  }
 
            set 
            { 
                _searchContainers = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Holds the strings to search for.
        /// </summary>
        public List<string> SearchStrings { get; set; }

        /// <summary>
        /// Holds the strings used to match against item names to include those items in the search. For the filesystem these items are files.
        /// </summary>
        public List<string> IncludeItemStrings
        {
            get { return _includeItemStrings; }
            set 
            { 
                _includeItemStrings = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Holds the strings used to match against item names to exclude those items. For the filesystem these items are files.
        /// </summary>
        public List<string> ExcludeItemStrings
        {
            get { return _excludeItemStrings; }
            set
            {
                _excludeItemStrings = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Holds the strings used to match against container names to include those containers. For the filesystem these containers are directories.
        /// </summary>
        public List<string> IncludeContainerStrings
        {
            get { return _includeContainerStrings; }
            set
            {
                _includeContainerStrings = value;
                _isPrepared = false;
            }

        }

        /// <summary>
        /// Holds the strings used to match against container names to exclude those containers. For the filesystem these containers are directories.
        /// </summary>
        public List<string> ExcludeContainerStrings
        {
            get { return _excludeContainerStrings; }
            set
            {
                _excludeContainerStrings = value;
                _isPrepared = false;
            }

        }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        public bool CaseSensitive { get; set; }

        /// <summary>
        /// Indicates whether to seach hidden files and folders.
        /// </summary>
        public bool SearchHidden
        {
            get { return _searchHidden; }
            set
            {
                _searchHidden = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        public bool SearchBinaries
        {
            get { return _searchBinaries; }
            set
            {
                _searchBinaries = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        public bool Recurse
        {
            get { return _searchRecursively; }
            set
            {
                _searchRecursively = value;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// This list holds all the URLs (in this provider these are filenames (including paths)) to search
        /// </summary>
        private List<String> _urlToSearch;

        /// <summary>
        /// Indicates whether this search has been prepared - and can be used - or not.
        /// </summary>
        private bool _isPrepared = false;

        /// <summary>
        /// Used to indicate this search that it will be cancelled
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        private CancellationToken cancellationToken;

        /// <summary>
        /// Keeps tracked of the number of files which have been scanned
        /// </summary>
        private int filesScanned;

        /// <summary>
        /// Event raised when notifying progress
        /// </summary>
        public event NotifyProgressEventHandler NotifyProgress;

        #endregion

        #region Interface implementations

        /// <summary>
        ///  Add a container to search
        /// </summary>
        public void AddSearchContainer(string container)
        {
            if (!SearchContainers.Contains(container))
            {
                SearchContainers.Add(container);
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Add a string to search for.
        /// </summary>
        /// <remarks>Adding a search string does not change whether or not the search has been prepared</remarks>
        public void AddSearchString(string searchString)
        {
            if (!SearchStrings.Contains(searchString))
            {
                SearchStrings.Add(searchString);
            }
        }

        /// <summary>
        /// Add an include string used to match against item names to include those items in the search.
        /// </summary>
        public void AddIncludeItemString(string includeItem)
        {
            if (!IncludeItemStrings.Contains(includeItem))
            {
                IncludeItemStrings.Add(includeItem);
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Add an exclude string used to match against item names to exclude those items.
        /// </summary>
        public void AddExcludeItemString(string excludeItem)
        {
            if (!ExcludeItemStrings.Contains(excludeItem))
            {
                ExcludeItemStrings.Add(excludeItem);
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Add an include string used to match against container names to include those containers.
        /// </summary>
        public void AddIncludeContainerString(string includeContainer)
        {
            if (!IncludeContainerStrings.Contains(includeContainer))
            {
                IncludeContainerStrings.Add(includeContainer);
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Add an exclude string used to match against container names to exclude those containers.
        /// </summary>
        public void AddExcludeContainerString(string excludeContainer)
        {
            if (!ExcludeContainerStrings.Contains(excludeContainer))
            {
                ExcludeContainerStrings.Add(excludeContainer);
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether the search should be case sensitive.
        /// </summary>
        public void SetCaseSensitive(bool caseSensitive)
        {
            if (CaseSensitive != caseSensitive)
            {
                CaseSensitive = caseSensitive;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether to search hidden items.
        /// </summary>
        public void SetSearchHidden(bool searchHidden)
        {
            if (SearchHidden != searchHidden)
            {
                SearchHidden = searchHidden;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether to search binary items.
        /// </summary>
        public void SetSearchBinaries(bool searchBinaries)
        {
            if (SearchBinaries != searchBinaries)
            {
                SearchBinaries = searchBinaries;
                _isPrepared = false;
            }
        }

        /// <summary>
        /// Indicates whether to descend into subdirectories.
        /// </summary>
        public void SetRecurse(bool recurse)
        {
            if (Recurse != recurse)
            {
                Recurse = recurse;
                _isPrepared = false;
            }
        }

        #endregion

        /// <summary>
        /// Creates a new Search Request
        /// </summary>
        public SearchRequestFilesystem()
        {
            Id = -1;    //Until this request is stored this Id is not required and so is set to a default
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

            this.cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Cancel the search request which is being prepared
        /// </summary>
        public void CancelRequest()
        {
            cancellationTokenSource.Cancel();
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
            #region Logging information
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
            #endregion

            filesScanned = 0;

            if (SearchContainers.Count == 0 || SearchStrings.Count == 0)
            {
                return 0;
            }

            //If the user has chosen to ignore binary files then we will use a known list of filename extensions to
            //cut down the list of files to search. Every file which is not excluded by this list will still be
            //checked to see if it is binary or not. All this list does is speed things up considerably by discarding
            //files which we know are binary. Obviously files which the user deliberately renames to have a binary-type
            //extension will be skipped.

            if (!SearchBinaries)
            {
                ExcludeItemStrings.AddRange(KNOWN_BINARY_FILENAME_REGEXES);
            }

            cancellationToken = this.cancellationTokenSource.Token;

            //Now we must build up the list of files to search. This will then be handed off to the search routine via the enumerator
            if (_urlToSearch.Count > 0)
            {
                _urlToSearch = new List<string>();
            }

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
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }
                var files = from f in new DirectoryInfo(directory).GetFiles() select f;

                foreach (FileInfo fileInfo in files)
                {
                    filesScanned++;
                    if (filesScanned % 10 == 0)
                    {
                        OnNotifyProgress(new NotifyProgressEventArgs(filesScanned));
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (includeThisFile(fileInfo.Name, fileInfo.FullName))
                    {
                        _urlToSearch.Add(fileInfo.FullName);
                    }
                }

                //Next get all the directories in this directory and then recurse through them - if recurse is on
                if (Recurse)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var dirs = from d in new DirectoryInfo(directory).GetDirectories() select d;

                    foreach (DirectoryInfo dir in dirs)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

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
            catch (PathTooLongException)
            {
                log.Debug("Path too long for .Net to handle: " + directory);
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
            catch(IOException)
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

        protected virtual void OnNotifyProgress(NotifyProgressEventArgs eventArgs)
        {
            if (NotifyProgress != null)
            {
                NotifyProgress(this, eventArgs);
            }
        }


    }
}
