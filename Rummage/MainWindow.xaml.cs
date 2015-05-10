using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Linq;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;
using RummageCore;
using RummageFilesystem;
using Snarl;
using Application = System.Windows.Application;
using MahApps.Metro.Controls;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        /// <summary>
        /// The name of the ini file
        /// </summary>
        private const string INI_FILE = "Rummage.ini";

        /// <summary>
        /// The default file extension to use for saving search requests
        /// </summary>
        private const string DEFAULT_REQUEST_EXTENSION = "rmgreq";

        /// <summary>
        /// The text to show in the "Save as Type" dropdown of the Save dialog
        /// </summary>
        private const string DEFAULT_REQUEST_FILTER = "Rummage Search Request (*.rmgreq)|*.rmgreq|All files (*.*)|*.*";

        /// <summary>
        /// The name of the settings file
        /// </summary>
        public const string SETTINGS_FILE = "Rummage_Settings.prf";

        /// <summary>
        /// Default preferences file to use when running Rummage for the first time
        /// </summary>
        public const string DEFAULT_SETTINGS_FILE = "DefaultPrefs";

        /// <summary>
        /// Used to indicate to the running tasks that a cancellation has been requested
        /// </summary>
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        /// <summary>
        /// The search to action
        /// </summary>
        private ISearch search;

        /// <summary>
        /// The search request to action.
        /// </summary>
        private ISearchRequest searchRequest;

        /// <summary>
        /// Holds the time the search was started so we can track how long it took.
        /// </summary>
        private DateTime startTime;

        /// <summary>
        /// Collection of matches found by the search which can be bound to the UI
        /// </summary>
        ObservableCollection<MatchingItem> matches = new ObservableCollection<MatchingItem>();


        /// <summary>
        /// Collection of matching lines in the currently selected match
        /// </summary>
        ObservableCollection<IMatch> matchingLinesForCurrentSelection = new ObservableCollection<IMatch>();

        /// <summary>
        /// Indicates whether a search is currently running
        /// </summary>
        private bool searchRunning = false;

        /// <summary>
        /// Used to cancel a running search
        /// </summary>
        private bool cancellingSearch = false;

        private Brush normalBorderBrush;

        private int snarlHandle = 1846493;      //Number picked at random

        private bool _searchStringsChanged = false;
        private bool _foldersChanged = false;
        private bool _includeFilesChanged = true;
        private bool _includeFoldersChanged = true;
        private bool _excludeFilesChanged = true;
        private bool _excludeFoldersChanged = true;
        private bool _recurseChanged = false;
        private bool _binariesChanged = false;
        private bool _caseSensitivityChanged = false;

        private HistoryPopup searchHistoryWindow;

        private HistoryPopup folderHistoryWindow;

        private string editor = "notepad.exe";

        private string editorArguments = string.Empty;

        private string searchRequestUrl = string.Empty;

        private Settings settings;

        public static RoutedCommand DoSearchRoutedCommand = new RoutedCommand();

        /// <summary>
        /// Main entry point to the program
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            Snarl.SnarlConnector.RegisterConfig((IntPtr)snarlHandle, "Rummage", WindowsMessage.WM_NULL);

            normalBorderBrush = dirChooser.InternalTextBox.BorderBrush;

            this.Closed += new EventHandler(MainWindow_Closed);

            setChangedStatus(true);     //Initially set the changed status to true to ensure that all initial selections are captured

            readIniFile();

            createHistoryWindows();

            wireUpCommands();

            resetUI();
//            loadTheme("XXX");
        }
        
        private void wireUpCommands()
        {
            // Create the binding.
            CommandBinding binding = new CommandBinding(ApplicationCommands.Find);

            // Attach the event handler.
            binding.Executed += ExecutedDoSearchCommand;
            binding.CanExecute += CanExecuteDoSearchCommand;

            KeyGesture doSearchKeyGesture = new KeyGesture(Key.F5, ModifierKeys.None);
            ApplicationCommands.Find.InputGestures.Add(doSearchKeyGesture);

            // Register the binding.
            this.CommandBindings.Add(binding);
        }

        /// <summary>
        /// Creates instances of the search history windows
        /// </summary>
        private void createHistoryWindows()
        {
            searchHistoryWindow = new HistoryPopup(Application.Current.MainWindow.GetType().Assembly.FullName + "." + "searchHistoryWindow");


            folderHistoryWindow = new HistoryPopup(Application.Current.MainWindow.GetType().Assembly.FullName + "." + "folderHistoryWindow");
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            searchHistoryWindow.ForceClose();
            searchHistoryWindow = null;

            folderHistoryWindow.ForceClose();
            folderHistoryWindow = null;

            SnarlConnector.RevokeConfig((IntPtr) snarlHandle);
        }

        /// <summary>
        /// Read startup parameters from the ini file
        /// </summary>
        private void readIniFile()
        {
            Setting.AllowSerialize = true;
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.DoNotVerify), SETTINGS_FILE);
            if (!File.Exists(settingsPath))
            {
                File.Copy(Path.Combine(Environment.CurrentDirectory, DEFAULT_SETTINGS_FILE), settingsPath);
            }
            settings = Settings.LoadSettings(settingsPath);
        }

        /// <summary>
        /// Builds the Search Request, prepares it and then executes it
        /// </summary>
        private void doSearch()
        {
            startTime = DateTime.Now;
            flowResults.Blocks.Clear();
            matches = new System.Collections.ObjectModel.ObservableCollection<MatchingItem>();
            matchingLinesForCurrentSelection.Clear();
            
            listViewMatches.ItemsSource = matches;
            listViewMatchesForSelection.ItemsSource = matchingLinesForCurrentSelection;
            runningProgress.Visibility = System.Windows.Visibility.Visible;

            #region Build search strings and search folders
            if (searchRequest == null)
            {
                searchRequest = new SearchRequestFilesystem();
            }

            if (_searchStringsChanged)
            {
                searchRequest.SearchStrings.Clear();

                for (int index = 0; index < textBoxSearchStrings.LineCount; index++)
                {
                    string line = textBoxSearchStrings.GetLineText(index).Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddSearchString(line);
                    }
                }
            }
            if (_foldersChanged)
            {
                searchRequest.SearchContainers.Clear();

                for (int index = 0; index < dirChooser.InternalTextBox.LineCount; index++)
                {
                    string line = dirChooser.InternalTextBox.GetLineText(index).Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddSearchContainer(line);
                    }
                }
            }

            #endregion

            #region Build includes
            if (_includeFilesChanged)
            {
                searchRequest.IncludeItemStrings.Clear();

                string[] lines = textBoxIncludeFiles.Text.Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index].Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddIncludeItemString(line);
                    }
                }
            }
            if (_includeFoldersChanged)
            {
                searchRequest.IncludeContainerStrings.Clear();

                string[] lines = textBoxIncludeFolders.Text.Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index].Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddIncludeContainerString(line);
                    }
                }
            }

            #endregion

            #region Build excludes
            if (_excludeFilesChanged)
            {
                searchRequest.ExcludeItemStrings.Clear();

                string[] lines = textBoxExcludeFiles.Text.Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index].Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddExcludeItemString(line);
                    }
                }
            }
            if (_excludeFoldersChanged)
            {
                searchRequest.ExcludeContainerStrings.Clear();

                string[] lines = textBoxExcludeFolders.Text.Split('\n');

                for (int index = 0; index < lines.Length; index++)
                {
                    string line = lines[index].Trim();
                    if (line.Length > 0)
                    {
                        searchRequest.AddExcludeContainerString(line);
                    }
                }
            }

            #endregion

            //Update the history popups with the values from this search
            updateHistoryPopups();

            searchRequest.SetRecurse(chkRecurse.IsChecked.Value);
            searchRequest.SetCaseSensitive(chkCaseSensitive.IsChecked.Value);
            searchRequest.SetSearchBinaries(chkBinaries.IsChecked.Value);
            searchRequest.NotifyProgress += new NotifyProgressEventHandler(searchRequest_NotifyProgress);
                
            search = new SearchFilesystem();
            search.ItemSearched += new ItemSearchedEventHandler(search_ItemSearched);

            searchRunning = true;
            setChangedStatus(false);        //Reset the changed status now that the search is about to run
            performSearchOnSeparateThread(search, searchRequest);

        }


        /// <summary>
        /// Prepare and execute the search on a separate thread.
        /// </summary>
        /// <param name="search">The search to run</param>
        /// <param name="request">The search request to action</param>
        private void performSearchOnSeparateThread(ISearch search, ISearchRequest request)
        {
            List<Task> tasks = new List<Task>();

            var token = tokenSource.Token;

            var t = Task.Factory.StartNew(() =>
            {
                prepareAndSearch(request, search);

                this.Dispatcher.BeginInvoke(new Action(() =>
                                                        updateStatus("")
                                                        )
                                            , null);
            }, token);


            tasks.Add(t);


            Task.Factory.ContinueWhenAll(tasks.ToArray(),
                                            result =>
                                            {
                                                this.Dispatcher.BeginInvoke(new Action(() =>
                                                                                    searchComplete())
                                                                            , null);
                                            });

        }


        /// <summary>
        /// Prepares the search and then runs it.
        /// </summary>
        /// <param name="request">The search request to action</param>
        /// <param name="search">The search to run</param>
        private void prepareAndSearch(ISearchRequest request, ISearch search)
        {
            updateStatus("Determining which files to search...");
            updateDocument(flowResults, "Determining which files to search...");
            setChangedStatus(false);

            if (!request.IsPrepared)
            {
                request.Prepare();                
            }
            string displayString = string.Format("Searching {0} {1} ...", searchRequest.Urls.Count, searchRequest.Urls.Count == 1 ? "file" : "files");
            updateDocument(flowResults, displayString);
            updateStatus(displayString);
            if (!cancellingSearch)
            {
                try
                {
                    search.Search(request, true);
                }
                catch (Exception exception)
                {
                    cancelRunningSearch();
                    updateDocument(flowResults, "Search aborted due to error: " + exception.Message);
                    updateStatus("Search aborted due to error: " + exception.Message);
                }
            }

        }

        /// <summary>
        /// Method run when the search has completed
        /// </summary>
        void searchComplete()
        {
            if (cancellingSearch)
            {
                updateStatus("Search cancelled. Ready.");
            }
            else
            {
                DateTime endTime = DateTime.Now;
                TimeSpan elapsed = endTime.Subtract(startTime);

                var successCount = (from m in search.Matches where m.Successful != null && m.Successful select m).Count();
                var failCount = search.Matches.Count - successCount;

                string matchWord = search.Matches.Count == 1 ? "match" : "matches";
                string fileWord = matches.Count == 1 ? "file" : "files";
                string searchedFilesWord = searchRequest.Urls.Count == 1 ? "file" : "files";

                string result;

                if (search.Matches.Count == 0)
                {
                    result = String.Format("Search complete. No matches found. {0} {1} searched ({2} seconds)",
                                                  searchRequest.Urls.Count, searchedFilesWord, String.Format("{0:0.00}", elapsed.TotalSeconds));
                }
                else
                {
                    if (failCount == 0)
                    {
                        result = String.Format("Search complete. {0} {4} found in {1} {5} out of {2} {6} searched ({3} seconds)",
                                                      search.Matches.Count, matches.Count, searchRequest.Urls.Count, String.Format("{0:0.00}", elapsed.TotalSeconds), matchWord, fileWord, searchedFilesWord);
                    }
                    else
                    {
                        result = String.Format("Search complete. {0} {4} found in {1} {5} out of {2} {6} searched ({3} seconds). {7} files could not be searched",
                                                      successCount, matches.Count, searchRequest.Urls.Count - failCount, String.Format("{0:0.00}", elapsed.TotalSeconds), matchWord, fileWord, searchedFilesWord, failCount);
                    }
                }
                searchingX.Text = string.Empty;
                searchingOfY.Text = string.Empty;
                updateDocument(flowResults, result);
                updateStatus(result);

                string snarlResult = string.Format("\n    {0} {1} found", successCount, matchWord);
                if (failCount > 0)
                {
                    snarlResult = snarlResult + String.Format("\n    {0} files couldn't be searched", failCount); 
                }
                Int32 nReturnId = SnarlConnector.ShowMessage("Rummage - search complete", snarlResult, 10, "", (IntPtr) 0, 0);
            }
            runningProgress.Visibility = System.Windows.Visibility.Hidden;
            btnStart.Content = "_Start Search";
            searchRunning = false;
            cancellingSearch = false;
            
        }

        /// <summary>
        /// Writes the message passed in to the document passed in.
        /// </summary>
        /// <param name="document">The document to update</param>
        /// <param name="message">The message to write to the document</param>
        private void updateDocument(FlowDocument document, string message)
        {
            // Checking if this thread has access to the object.
            if (document.Dispatcher.CheckAccess())
            {
                Paragraph p = new Paragraph();
                p.FontSize = 10;
                p.Inlines.Add(message);

                // This thread has access so it can update the UI thread.      
                document.Blocks.Add(p);
                ScrollViewer sv = FindScrollViewer(ScrollableViewer);
                if (sv != null)
                {
                    sv.ScrollToEnd();
                }
            }
            else
            {
                // This thread does not have access to the UI thread.
                // Place the update method on the Dispatcher of the UI thread.
                document.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                                 {
                                     Paragraph p = new Paragraph();
                                     p.FontSize = 10;
                                     p.Inlines.Add(message);
                                     document.Blocks.Add(p);
                                     ScrollViewer sv = FindScrollViewer(ScrollableViewer);
                                     if (sv != null)
                                     {
                                         sv.ScrollToEnd();
                                     }
                                 }
                                 ));
            }
        }


        /// <summary>
        /// Updates the Status Bar message area with the message passed in
        /// </summary>
        /// <param name="message">Message to display in the status bar</param>
        private void updateStatus(string message)
        {
            // Checking if this thread has access to the object.
            if (textBlockCurrentStatus.Dispatcher.CheckAccess())
            {
                // This thread has access so it can update the UI thread.      
                textBlockCurrentStatus.Text = message;
            }
            else
            {
                // This thread does not have access to the UI thread.
                // Place the update method on the Dispatcher of the UI thread.
                textBlockCurrentStatus.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() => { textBlockCurrentStatus.Text = message; }));
            }
        }


        /// <summary>
        /// Formats the output according to the output format given
        /// </summary>
        /// <param name="match">The match object to format</param>
        /// <returns>Output line ready for writing to the display</returns>
        private static string formatOutputLine(IMatch match)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{MatchItem}:{MatchLineNumber} {MatchLine}");
            builder.Replace("{MatchItem}", match.MatchItem);
            builder.Replace("{MatchLineNumber}", match.MatchLineNumber.ToString());
            builder.Replace("{MatchLine}", match.MatchLine);
            builder.Replace("{MatchString}", match.MatchString);

            return builder.ToString();
        }

        /// <summary>
        /// Check whether there are enough valid parameters present to be able to search
        /// </summary>
        /// <returns>True if the parameters being checked are valid</returns>
        private bool checkParms()
        {
            if (textBoxSearchStrings.Text.Trim().Length == 0)
            {
                return false;
            }

            if (dirChooser.InternalTextBox.Text.Trim().Length == 0)
            {
                return false;
            }

            var invalidregex = RummageHelper.AreRegexesValid(textBoxIncludeFiles.Text);
            if (invalidregex.Length > 0)
            {
                updateStatus("Cannot search - the following regex in the \"Include Files\" filter is invalid: " + invalidregex);
                return false;
            }

            invalidregex = RummageHelper.AreRegexesValid(textBoxIncludeFolders.Text);
            if (invalidregex.Length > 0)
            {
                updateStatus("Cannot search - the following regex in the \"Include Folders\" filter is invalid: " + invalidregex);
                return false;
            }

            invalidregex = RummageHelper.AreRegexesValid(textBoxExcludeFiles.Text);
            if (invalidregex.Length > 0)
            {
                updateStatus("Cannot search - the following regex in the \"Exclude Files\" filter is invalid: " + invalidregex);
                return false;
            }

            invalidregex = RummageHelper.AreRegexesValid(textBoxExcludeFolders.Text);
            if (invalidregex.Length > 0)
            {
                updateStatus("Cannot search - the following regex in the \"Exclude Folders\" filter is invalid: " + invalidregex);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Adds the match to the collection of matches to be displayed
        /// </summary>
        /// <param name="match">The match to add</param>
        /// <returns>True if this is a new item which has been matched or False if this match has been added to an existing item</returns>
        private void addToMatches(IMatch match)
        {
            MatchingItem mi = new MatchingItem(match.MatchItem);
            textBlockCurrentStatus.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    if (matches.Contains(mi))
                    {
                        int index = matches.IndexOf(mi);
                        MatchingItem miToUpdate = matches[index] as MatchingItem;
                        miToUpdate.matches.Add(match);
                    }
                    else
                    {
                        matches.Add(new MatchingItem(match.MatchItem, match));
                    }

                }));

        }

        private void updateProgressBar(int x, int ofY, string processName, string item, bool showProgressBar)
        {
            // Checking if this thread has access to the object.
            if (runningProgress.Dispatcher.CheckAccess())
            {
                // This thread has access so it can update the UI thread.      
                runningProgress.Value = x;
                runningProgress.Maximum = ofY;
                if (processName.Trim().Length == 0)
                {
                    searchingX.Text = string.Empty;
                }
                else
                {
                    searchingX.Text = String.Format("{0} {1} {2}", processName, item, x.ToString());
                    //searchingX.Text = processName + " file " + x.ToString() + " ";
                }
                if (ofY == 0)
                {
                    searchingOfY.Text = string.Empty;
                }
                else
                {
                    searchingOfY.Text = "of " + ofY.ToString();
                }

                runningProgress.Visibility = (showProgressBar ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden);
            }
            else
            {
                // This thread does not have access to the UI thread.
                // Place the update method on the Dispatcher of the UI thread.
                runningProgress.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                                 {
                                     runningProgress.Value = x;
                                     runningProgress.Maximum = ofY;
                                     searchingX.Text = String.Format("{0} {1} {2}", processName, item, x.ToString());
                                     //searchingX.Text = processName + " file " + x.ToString() + " ";
                                     if (ofY == 0)
                                     {
                                         searchingOfY.Text = string.Empty;
                                     }
                                     else
                                     {
                                         searchingOfY.Text = "of " + ofY.ToString();
                                     }

                                     runningProgress.Visibility = (showProgressBar ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden);
                                 }));
            }

        }

        /// <summary>
        /// Check all the regexes entered in the search box and highlight it if any are invalid
        /// </summary>
        /// <returns>true if all the searches are valid, otherwise false</returns>
        private bool checkSearchesAreValid()
        {
            if (textBoxSearchStrings == null)
            {
                return false;
            }

            bool result = true;

            string invalidRegex = RummageHelper.AreRegexesValid(textBoxSearchStrings.Text);


            if (invalidRegex.Length > 0)
            {
                updateStatus(String.Format("Search term {0} is not a valid regular expression", invalidRegex));
                textBoxSearchStrings.BorderBrush = Brushes.Red;
                textBoxSearchStrings.BorderThickness = new Thickness(3, 3, 3, 3);
                result = false;
            }
            else
            {
                if (textBlockCurrentStatus.Text.StartsWith("Search term ") && textBlockCurrentStatus.Text.EndsWith("is not a valid regular expression"))
                {
                    updateStatus(String.Empty);
                }
                textBoxSearchStrings.BorderBrush = normalBorderBrush;
                textBoxSearchStrings.BorderThickness = new Thickness(1, 1, 1, 1);
                result = true;
            }

            return result;
        }


        private void enableDisableSearchButton()
        {
            if (btnStart != null)
            {
                btnStart.IsEnabled = checkSearchesAreValid() && checkFoldersExist();
            }
        }

        /// <summary>
        /// Check all the folders specified in the Folders textbox to ensure they all exist
        /// </summary>
        /// <returns>true if all the folders specified exist, otherwise false</returns>
        private bool checkFoldersExist()
        {
            if (dirChooser.InternalTextBox == null)
            {
                return false;
            }

            string missingFolder = string.Empty;
            bool result = true;

            for (int index = 0; index < dirChooser.InternalTextBox.LineCount; index++)
            {
                string line = dirChooser.InternalTextBox.GetLineText(index).Trim();
                if (line.Length > 0 && !Directory.Exists(line))
                {
                    missingFolder = line;
                    break;
                }
            }

            if (missingFolder.Length > 0)
            {
                updateStatus(String.Format("Search Folder {0} doesn't exist", missingFolder));
                dirChooser.InternalTextBox.BorderBrush = Brushes.Red;
                dirChooser.InternalTextBox.BorderThickness = new Thickness(3, 3, 3, 3);
                result = false;
            }
            else
            {
                if (textBlockCurrentStatus.Text.StartsWith("Search Folder ") && textBlockCurrentStatus.Text.EndsWith("doesn't exist"))
                {
                    updateStatus(String.Empty);
                }
                dirChooser.InternalTextBox.BorderBrush = normalBorderBrush;
                dirChooser.InternalTextBox.BorderThickness = new Thickness(1, 1, 1, 1);
                result = true;
            }

            return result;
        }

        private void loadTheme(string themePath)
        {
            this.Background = Brushes.Black;
            this.Foreground = Brushes.Green;
            this.FontFamily = new FontFamily("Courier New");

            textBoxSearchStrings.Background = Brushes.Black;
            textBoxSearchStrings.Foreground = Brushes.Green;
            textBoxSearchStrings.FontFamily = new FontFamily("Courier New");

            dirChooser.InternalTextBox.Background = Brushes.Black;
            dirChooser.InternalTextBox.Foreground = Brushes.Green;
            dirChooser.InternalTextBox.FontFamily = new FontFamily("Courier New");

            listViewMatches.Background = Brushes.Black;
            listViewMatches.Foreground = Brushes.Green;

            listViewMatchesForSelection.Background = Brushes.Black;
            listViewMatchesForSelection.Foreground = Brushes.Green;

            chkRecurse.Background = Brushes.Black;
            chkRecurse.Foreground = Brushes.Green;

            chkCaseSensitive.Background = Brushes.Black;
            chkCaseSensitive.Foreground = Brushes.Green;

            chkBinaries.Background = Brushes.Black;
            chkBinaries.Foreground = Brushes.Green;

            textBoxIncludeFiles.Background = Brushes.Black;
            textBoxIncludeFiles.Foreground = Brushes.Green;

            textBoxIncludeFolders.Background = Brushes.Black;
            textBoxIncludeFolders.Foreground = Brushes.Green;

            textBoxExcludeFiles.Background = Brushes.Black;
            textBoxExcludeFiles.Foreground = Brushes.Green;

            textBoxExcludeFolders.Background = Brushes.Black;
            textBoxExcludeFolders.Foreground = Brushes.Green;

            textBlockCurrentStatus.Background = Brushes.Black;
            textBlockCurrentStatus.Foreground = Brushes.Green;

            statusBarMain.Background = Brushes.Black;
            statusBarMain.Foreground = Brushes.Green;

            tabResults.Background = Brushes.Black;
            tabResults.Foreground = Brushes.Green;

            tabFilters.Background = Brushes.Black;
            tabFilters.Foreground = Brushes.Green;

            tabItemExcludes.Background = Brushes.Black;
            tabItemExcludes.Foreground = Brushes.Green;

            tabItemIncludes.Background = Brushes.Black;
            tabItemIncludes.Foreground = Brushes.Green;

            labelExcludeTabHeader.Background = Brushes.Black;
            labelExcludeTabHeader.Foreground = Brushes.Green;

            labelIncludeTabHeader.Background = Brushes.Black;
            labelIncludeTabHeader.Foreground = Brushes.Green;

        }

        /// <summary>
        /// Set the "Changed" status
        /// </summary>
        /// <param name="status"></param>
        private void setChangedStatus(bool status)
        {
            _searchStringsChanged = status;
            _foldersChanged = status;
            _includeFilesChanged = status;
            _includeFoldersChanged = status;
            _excludeFilesChanged = status;
            _excludeFoldersChanged = status;
            _recurseChanged = status;
            _binariesChanged = status;
            _caseSensitivityChanged = status;
            
        }

        #region Helper methods
        public static ScrollViewer FindScrollViewer(FlowDocumentScrollViewer flowDocumentScrollViewer)
        {
            if (VisualTreeHelper.GetChildrenCount(flowDocumentScrollViewer) == 0)
            {
                return null;
            }

            // Border is the first child of first child of a ScrolldocumentViewer
            DependencyObject firstChild = VisualTreeHelper.GetChild(flowDocumentScrollViewer, 0);
            if (firstChild == null)
            {
                return null;
            }

            Decorator border = VisualTreeHelper.GetChild(firstChild, 0) as Decorator;

            if (border == null)
            {
                return null;
            }

            return border.Child as ScrollViewer;
        }
        /// <summary>
        /// Backing store for the <see cref="ScrollViewer"/> property.
        /// </summary>
        private ScrollViewer scrollViewer;

        /// <summary>
        /// Gets the scroll viewer contained within the FlowDocumentScrollViewer control
        /// </summary>
        public ScrollViewer ScrollViewer
        {
            get
            {
                if (this.scrollViewer == null)
                {
                    DependencyObject obj = this;

                    do
                    {
                        if (VisualTreeHelper.GetChildrenCount(obj) > 0)
                            obj = VisualTreeHelper.GetChild(obj as Visual, 0);
                        else
                            return null;
                    }
                    while (!(obj is ScrollViewer));

                    this.scrollViewer = obj as ScrollViewer;
                }

                return this.scrollViewer;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Starts of Stops the search
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e">Not used</param>
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!searchRunning)
            {
                if (checkParms())
                {
                    btnStart.Content = "_Cancel Search";
                    cancellingSearch = false;
                    doSearch();
                }
            }
            else
            {
                updateDocument(flowResults, "Search cancelled.");
                cancelRunningSearch();
            }
        }

        /// <summary>
        /// Sends cancel messages to the search process to stop it running
        /// </summary>
        private void cancelRunningSearch()
        {
            if (searchRequest != null)
            {
                searchRequest.CancelRequest();
            }
            if (search != null)
            {
                search.CancelSearch();
            }

            cancellingSearch = true;
            btnStart.Content = "_Start Search";
            updateProgressBar(0, 0, string.Empty, string.Empty, false);
        }


        private void updateHistoryPopups()
        {
            foreach (var searchString in searchRequest.SearchStrings)
            {
                searchHistoryWindow.AddItemToList(searchString);
            }

            searchHistoryWindow.SaveList();

            foreach (var containerString in searchRequest.SearchContainers)
            {
                folderHistoryWindow.AddItemToList(containerString);
            }

            folderHistoryWindow.SaveList();

        }

        /// <summary>
        /// When the selected item changes reload the bottom display with the matches from the selected item
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e">Argument holding the selected item</param>
        private void listViewMatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listViewMatches.SelectedItem != null)
            {
                matchingLinesForCurrentSelection = new ObservableCollection<IMatch>();
                MatchingItem itemToSearchFor = listViewMatches.SelectedItem as MatchingItem;

                if (itemToSearchFor != null && itemToSearchFor.matches != null)
                {
                    foreach (IMatch match in itemToSearchFor.matches)
                    {
                        matchingLinesForCurrentSelection.Add(match);
                    }
                }
                listViewMatchesForSelection.ItemsSource = matchingLinesForCurrentSelection;
            }

        }

        /// <summary>
        /// Triggered when a match is found. Updates the collection of matches for display to the user.
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e">ItemSearchedEventArgs object holding details of the match</param>
        void search_ItemSearched(object sender, EventArgs e)
        {
            var iea = e as ItemSearchedEventArgs;
            if (iea != null)
            {
                updateProgressBar(iea.FileX, iea.FileOfY, "Searching", "file", true);

                foreach (IMatch match in iea.Matches)
                {
                    if (match.Successful)
                    {
                        addToMatches(match);
                        updateDocument(flowResults, formatOutputLine(match));

                    }
                    else
                    {
                        updateDocument(flowResults, match.ErrorMessage);
                    }
                }
            }
        }
 
        /// <summary>
        /// Whenever the text changes we'll check whether the entered folders exist
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e"></param>
        private void dirChooser_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableDisableSearchButton();
            _foldersChanged = true;

            if (dirChooser.InternalTextBox.LineCount > 2)
            {
                lblSearchHere.Content = string.Empty;
            }
            else
            {
                lblSearchHere.Content = "Search where ...";
            }
        }

        void searchRequest_NotifyProgress(object sender, EventArgs e)
        {
            NotifyProgressEventArgs nea = e as NotifyProgressEventArgs;

            if (nea != null)
            {
                updateProgressBar(nea.NumberOfFilesScanned, 0, "Examining", "filenames", false);
            }
        }


        private void textBoxSearchStrings_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableDisableSearchButton();
            _searchStringsChanged = true;

            if (textBoxSearchStrings.LineCount > 2)
            {
                lblSearchFor.Content = string.Empty;
            }
            else
            {
                lblSearchFor.Content = "Search for ...";
            }
        }


        private void listViewMatches_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listViewMatches.SelectedItem != null)
            {
                MatchingItem selectedItem = listViewMatches.SelectedItem as MatchingItem;

                if (selectedItem != null)
                {
                    StartEditor(buildArguments(selectedItem.ItemKey, 1));
                }
            }

        }

        /// <summary>
        /// Combine the selected file with the arguments for the editor
        /// </summary>
        /// <param name="itemPath">File to open</param>
        /// <param name="lineNumber">Line number to jump to</param>
        /// <returns></returns>
        private string buildArguments(string itemPath, int lineNumber)
        {
            string arguments = @"""" + itemPath + @"""";
            if (editorArguments.Length > 0)
            {
                arguments = editorArguments + lineNumber + " " + arguments;
            }
            return arguments;
        }

        private void StartEditor(string arguments)
        {

            editor = settings.GetSettingByName("Editor").ValueAsText;
            editorArguments = settings.GetSettingByName("Editor Args").ValueAsText;

            if (editor.Trim().Length == 0)
            {
                editor = "notepad.exe";
            }

            ProcessStartInfo procStartInfo = new ProcessStartInfo(editor);
            procStartInfo.Arguments = arguments;

            procStartInfo.CreateNoWindow = true;
            procStartInfo.UseShellExecute = true;
            Process.Start(procStartInfo);
        }

        /// <summary>
        /// Open a Folder-Browse dialog and add the user's selection to the Folders box
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e">Standard EventArgs object</param>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (dirChooser.InternalTextBox.Text.Length > 1)
                {
                    dirChooser.InternalTextBox.Text += "\n";
                }
                dirChooser.InternalTextBox.Text += fbd.SelectedPath;
            }
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainMenuHelpAbout_Click(object sender, RoutedEventArgs e)
        {
            RummageAbout about = new RummageAbout
                                     {
                                         Owner = this,
                                         WindowStartupLocation = WindowStartupLocation.CenterOwner
                                     };

            about.ShowDialog();
        }

        private void chkRecurse_Checked(object sender, RoutedEventArgs e)
        {
            _recurseChanged = true;
        }

        private void chkBinaries_Checked(object sender, RoutedEventArgs e)
        {
            _binariesChanged = true;
        }

        private void chkCaseSensitive_Checked(object sender, RoutedEventArgs e)
        {
            _caseSensitivityChanged = true;
        }

        private void textBoxIncludeFiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            _includeFilesChanged = true;
        }

        private void textBoxIncludeFolders_TextChanged(object sender, TextChangedEventArgs e)
        {
            _includeFoldersChanged = true;
        }

        private void textBoxExcludeFiles_TextChanged(object sender, TextChangedEventArgs e)
        {
            _excludeFilesChanged = true;
        }

        private void textBoxExcludeFolders_TextChanged(object sender, TextChangedEventArgs e)
        {
            _excludeFoldersChanged = true;
        }
        #endregion

        private void MainMenuFileSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            

        }

        private void btnFoldersHistory_Click(object sender, RoutedEventArgs e)
        {
            folderHistoryWindow.ShowDialog();

            if (folderHistoryWindow.SelectedItem != null && folderHistoryWindow.SelectedItem.Length > 0)
            {
                dirChooser.InternalTextBox.Text += "\n" + folderHistoryWindow.SelectedItem;
                dirChooser.InternalTextBox.Text = dirChooser.InternalTextBox.Text.Trim();
            }
        }

        private void btnSearchHistory_Click(object sender, RoutedEventArgs e)
        {
            searchHistoryWindow.ShowDialog();
            if (searchHistoryWindow.SelectedItem != null && searchHistoryWindow.SelectedItem.Length > 0)
            {
                textBoxSearchStrings.Text += "\n" + searchHistoryWindow.SelectedItem;
                textBoxSearchStrings.Text = textBoxSearchStrings.Text.Trim();
            }

        }

        private void listViewMatchesForSelection_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listViewMatches.SelectedItem != null)
            {
                MatchingItem selectedItem = listViewMatches.SelectedItem as MatchingItem;
                if (listViewMatchesForSelection.SelectedItem != null)
                {
                    var match = listViewMatchesForSelection.SelectedItem as RummageCore.Match;
                    if (match != null)
                    {
                        if (selectedItem != null)
                        {
                            StartEditor(buildArguments(selectedItem.ItemKey, match.MatchLineNumber));
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Allow Ctrl+Up/Down to change the selected match in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Down || e.Key == Key.Up) && ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control))
            {
                NavigateResultRows(e.Key);
            }
        }


        /// <summary>
        /// Move the selection up/down the list of matches
        /// </summary>
        /// <param name="key">The key the user pressed to initiate the selection</param>
        private void NavigateResultRows(Key key)
        {
            if (listViewMatches != null && listViewMatches.Items.Count > 1)
            {
                if (listViewMatches.SelectedItems.Count == 0)
                {
                    listViewMatches.SelectedIndex = 0;
                    return;
                }

                if (key == Key.Down)
                {
                    if (listViewMatches.SelectedIndex == listViewMatches.Items.Count - 1)
                    {
                        listViewMatches.SelectedIndex = 0;
                    }
                    else
                    {
                        if (listViewMatches.SelectedIndex < listViewMatches.Items.Count - 1)
                        {
                            listViewMatches.SelectedIndex++;
                        }
                    }
                }
                else
                {
                    if (listViewMatches.SelectedIndex == 0)
                    {
                        listViewMatches.SelectedIndex = listViewMatches.Items.Count - 1;
                    }
                    else
                    {
                        if (listViewMatches.SelectedIndex > 0)
                        {
                            listViewMatches.SelectedIndex--;
                        }
                    }                
                }
            }
        }

        private void listViewMatches_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (listViewMatches.SelectedItems.Count > 0)
            {
                MatchingItem matchingItem = listViewMatches.SelectedItem as MatchingItem;

                if (matchingItem != null)
                {
                    if (File.Exists(matchingItem.ItemKey))
                    {
                        FileInfo[] singleFile = new FileInfo[1];
                        singleFile[0] = new FileInfo(matchingItem.ItemKey);
                        ShellContextMenu shellContextMenu = new ShellContextMenu();
                        
                        System.Drawing.Point pt = new System.Drawing.Point(Convert.ToInt32(Mouse.GetPosition(this).X), Convert.ToInt32(Mouse.GetPosition(this).Y));

                        shellContextMenu.ShowContextMenu(singleFile, pt);
                    }
                }
            }
            
        }

        private void MainMenuHelpRegex_OnClick(object sender, RoutedEventArgs e)
        {
            var doc = new RegexHelpWindow();
            doc.Show();
        }

        private void SaveSearchRequest_OnClick(object sender, RoutedEventArgs e)
        {
            if (searchRequestUrl.Length == 0)
            {
                SaveSearchRequestAs_OnClick(sender, e);
            }
            else
            {
                searchRequest.SaveSearchRequest(searchRequestUrl);
            }
        }

        private void SaveSearchRequestAs_OnClick(object sender, RoutedEventArgs e)
        {
            if (searchRequest == null || ! searchRequest.IsPrepared)
            {
                System.Windows.MessageBox.Show("Please run the search before saving it", "Rummage - cannot save Search Request");
                return;
            }

            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.AddExtension = true;
            saveDialog.CheckPathExists = true;
            saveDialog.DefaultExt = DEFAULT_REQUEST_EXTENSION;
            saveDialog.Filter = DEFAULT_REQUEST_FILTER;
            saveDialog.OverwritePrompt = true;

            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                bool result = searchRequest.SaveSearchRequest(saveDialog.FileName);
                
                if (result)
                {
                    setWindowTitle(saveDialog.FileName);
                }

                updateStatus((result ? "Search request saved" : "Save failed"));
            }
        }

        private void LoadSearchRequest_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opendialog = new OpenFileDialog();
            opendialog.DefaultExt = DEFAULT_REQUEST_EXTENSION;
            opendialog.Filter = DEFAULT_REQUEST_FILTER;
            opendialog.CheckFileExists = true;
            opendialog.Multiselect = false;

            if (opendialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (searchRequest == null)
                {
                    searchRequest = new SearchRequestFilesystem();
                }
                searchRequest = searchRequest.LoadSearchRequest(opendialog.FileName);

                if (searchRequest != null)
                {
                    searchRequestUrl = opendialog.FileName;
                    populateScreenFromSearchRequest(searchRequest, searchRequestUrl);
                }
            }
        }

        /// <summary>
        /// Get the date this request was saved so we can work out how old it is
        /// </summary>
        /// <param name="filename">The search request file</param>
        /// <returns>The last write date</returns>
        private DateTime getSearchRequestSavedDate(string filename)
        {
            FileInfo fi = new FileInfo(filename);

            return fi.LastWriteTime;
        }

        /// <summary>
        /// Update the screen based on the contents of the Search Request which has just been loaded
        /// </summary>
        /// <param name="loadSearchRequest"></param>
        /// <param name="filename"></param>
        private void populateScreenFromSearchRequest(ISearchRequest loadSearchRequest, string filename)
        {
            loadUIContainer(textBoxSearchStrings, loadSearchRequest.SearchStrings);
            loadUIContainer(dirChooser.InternalTextBox, loadSearchRequest.SearchContainers);
            loadUIContainer(textBoxIncludeFiles, loadSearchRequest.IncludeItemStrings);
            loadUIContainer(textBoxIncludeFolders, loadSearchRequest.IncludeContainerStrings);
            loadUIContainer(textBoxExcludeFiles, loadSearchRequest.ExcludeItemStrings);
            loadUIContainer(textBoxExcludeFolders, loadSearchRequest.ExcludeContainerStrings);
            chkBinaries.IsChecked = loadSearchRequest.SearchBinaries;
            chkCaseSensitive.IsChecked = loadSearchRequest.CaseSensitive;
            chkRecurse.IsChecked = loadSearchRequest.Recurse;

            setWindowTitle(filename);

            //Mark all the change tracking variables as false
            _searchStringsChanged = false;
            _foldersChanged = false;
            _includeFilesChanged = false;
            _includeFoldersChanged = false;
            _excludeFilesChanged = false;
            _excludeFoldersChanged = false;
        }

        /// <summary>
        /// Sets the window title based on the name and age of the saved Search Request
        /// </summary>
        /// <param name="filename">The filename for the saved Search Request</param>
        private void setWindowTitle(string filename)
        {
            DateTime searchRequestLastSaved = getSearchRequestSavedDate(filename);

            int daysOld = (DateTime.Now - searchRequestLastSaved).Days;

            string filenameOnly = Path.GetFileNameWithoutExtension(filename);

            string age;
            string ageDescription;

            switch (daysOld)
            {
                case 0:
                    age = "(today)";
                    ageDescription = string.Format("This search request is new today (0 days old)", daysOld);
                    break;
                case 1:
                    age = "(1 day old)";
                    ageDescription = string.Format("This search request is 1 day old", daysOld);
                    break;
                default:
                    age = string.Format("({0} days old)", daysOld);
                    ageDescription = string.Format("This search request is {0} days old", daysOld);
                    break;
            }

            updateStatus("Loaded. " + ageDescription + ".");

            this.Title = string.Format("Rummage - {0} {1}", filenameOnly, age);

            //Write details of what has been loaded to the text pane
            updateDocument(flowResults, string.Format("Loaded Search Request: {0}", filenameOnly));
            updateDocument(flowResults, string.Format("    {0} and will search {1} {2}", ageDescription, searchRequest.Urls.Count, searchRequest.Urls.Count == 1 ? "file." : "files."));

        }

        /// <summary>
        /// Moves the strings from a List to a textbox control
        /// </summary>
        /// <param name="container">The TextBox container to load</param>
        /// <param name="source">The source List</param>
        private void loadUIContainer(System.Windows.Controls.TextBox container, List<string> source)
        {
            container.Clear();

            foreach (var str in source)
            {
                container.Text += str + Environment.NewLine;
            }   
        }

        /// <summary>
        /// Copies the selected filenames to the clipboard
        /// </summary>
        /// <param name="sender">The menu item which triggered this call</param>
        /// <param name="e">Standard event args</param>
        private void EditCopyFilenames_OnClick(object sender, RoutedEventArgs e)
        {
            if (listViewMatches.SelectedItems.Count == 0)
            {
                return;
            }

            StringBuilder filenames = new StringBuilder();
            foreach (MatchingItem item in listViewMatches.SelectedItems)
            {
                filenames.AppendLine(item.ItemKey);
            }

            if (filenames.Length > 0)
            {
                System.Windows.Clipboard.SetText(filenames.ToString().TrimEnd());
            }
        }

        /// <summary>
        /// Clears everything out ready to perform a new search
        /// </summary>
        /// <param name="sender">Component triggering this event</param>
        /// <param name="e">Standard event args</param>
        private void NewSearchRequest_OnClick(object sender, RoutedEventArgs e)
        {
            matchingLinesForCurrentSelection.Clear();
            matches.Clear();
            searchRequest = new SearchRequestFilesystem();

            resetUI();
        }

        /// <summary>
        /// Clears the UI completely 
        /// </summary>
        private void resetUI()
        {
            textBoxSearchStrings.Clear();
            dirChooser.InternalTextBox.Clear();
            textBoxIncludeFiles.Clear();
            textBoxIncludeFiles.Text = turnCollectionIntoString("Include Files");
            textBoxIncludeFolders.Clear();
            textBoxIncludeFolders.Text = turnCollectionIntoString("Include Folders");
            textBoxExcludeFiles.Clear();
            textBoxExcludeFiles.Text = turnCollectionIntoString("Exclude Files");
            textBoxExcludeFolders.Clear();
            textBoxExcludeFolders.Text = turnCollectionIntoString("Exclude Folders");
            chkBinaries.IsChecked = settings.GetSettingByName("Search Binaries").ValueAsBoolean;
            chkCaseSensitive.IsChecked = settings.GetSettingByName("Case Sensitive").ValueAsBoolean;
            chkRecurse.IsChecked = settings.GetSettingByName("Sub Folders").ValueAsBoolean;
            flowResults.Blocks.Clear();

            this.Title = "Rummage";
            updateStatus("Ready.");
            searchingX.Text = string.Empty;
            searchingOfY.Text = string.Empty;
            runningProgress.Value = 0;
        }

        private string turnCollectionIntoString(string settingName)
        {
            List<string> collection = settings.GetSettingByName(settingName).ValueAsCollection;
            string returnValue;

            if (collection.Count == 0)
            {
                returnValue = string.Empty;
            }
            else
            {
                returnValue = collection.Aggregate((current, next) => current + Environment.NewLine + next);
            }

            return returnValue;
        }

        /// <summary>
        /// Initiate the Search when the command is executed
        /// </summary>
        /// <param name="sender">Control which sends this command</param>
        /// <param name="e">Event arguments</param>
        private void ExecutedDoSearchCommand(object sender, ExecutedRoutedEventArgs e)
        {
            doSearch();
        }

        /// <summary>
        /// Used by routed command to indicate whether Search can be initiated
        /// </summary>
        /// <param name="sender">Standard sender</param>
        /// <param name="e">Event Args for this event</param>
        private void CanExecuteDoSearchCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = checkSearchesAreValid() && checkFoldersExist() && textBoxSearchStrings.Text.Trim().Length > 0 && dirChooser.InternalTextBox.Text.Trim().Length > 0;
        }

        private void EditSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsScreen = new SettingScreen(settings);

            settingsScreen.WindowStyle = System.Windows.WindowStyle.ToolWindow;
            settingsScreen.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            settingsScreen.ShowDialog();
            settings = settingsScreen.settings;
        }
    }
}

