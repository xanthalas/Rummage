using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Threading;
using System.Threading.Tasks;
using RummageCore;
using RummageFilesystem;
using Snarl;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
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

        /// <summary>
        /// Main entry point to the program
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Snarl.SnarlConnector.RegisterConfig((IntPtr)snarlHandle, "Rummage", WindowsMessage.WM_NULL);

            normalBorderBrush = textBoxFolders.BorderBrush;

            this.Closed += new EventHandler(MainWindow_Closed);
        }

        void MainWindow_Closed(object sender, EventArgs e)
        {
            SnarlConnector.RevokeConfig((IntPtr) snarlHandle);
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

            #region Build search strings and search folders
            searchRequest = new SearchRequestFilesystem();
            for (int index = 0; index < textBoxSearchStrings.LineCount; index++)
            {
                string line = textBoxSearchStrings.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.SearchStrings.Add(line);
                }
            }
            for (int index = 0; index < textBoxFolders.LineCount; index++)
            {
                string line = textBoxFolders.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.SearchContainers.Add(line);
                }
            }
            #endregion

            #region Build includes
            for (int index = 0; index < textBoxIncludeFiles.LineCount; index++)
            {
                string line = textBoxIncludeFiles.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.IncludeItemStrings.Add(line);
                }
            }
            for (int index = 0; index < textBoxIncludeFolders.LineCount; index++)
            {
                string line = textBoxIncludeFolders.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.IncludeContainerStrings.Add(line);
                }
            }
            #endregion

            #region Build excludes
            for (int index = 0; index < textBoxExcludeFiles.LineCount; index++)
            {
                string line = textBoxExcludeFiles.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.ExcludeItemStrings.Add(line);
                }
            }
            for (int index = 0; index < textBoxExcludeFolders.LineCount; index++)
            {
                string line = textBoxExcludeFolders.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    searchRequest.ExcludeContainerStrings.Add(line);
                }
            }
            #endregion


            searchRequest.Recurse = chkRecurse.IsChecked.Value;
            searchRequest.CaseSensitive = chkCaseSensitive.IsChecked.Value;
            searchRequest.SearchBinaries = chkBinaries.IsChecked.Value;
            searchRequest.NotifyProgress += new NotifyProgressEventHandler(searchRequest_NotifyProgress);
                
            search = new SearchFilesystem();
            search.ItemSearched += new ItemSearchedEventHandler(search_ItemSearched);

            searchRunning = true;
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
            request.Prepare();
            string displayString = string.Format("Searching {0} files...", searchRequest.Urls.Count);
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

                string matchWord = search.Matches.Count == 1 ? "match" : "matches";
                string fileWord = matches.Count == 1 ? "file" : "files";
 
                string result = String.Format("Search complete. {0} {4} found in {1} {5} out of {2} files searched ({3} seconds)",
                                              search.Matches.Count, matches.Count, searchRequest.Urls.Count, String.Format("{0:0.00}", elapsed.TotalSeconds), matchWord, fileWord);
                searchingX.Text = "Searching file " + searchRequest.Urls.Count.ToString() + " ";
                searchingOfY.Text = "of " + searchRequest.Urls.Count.ToString();
                updateDocument(flowResults, result);
                updateStatus(result);

                string snarlResult = string.Format("\n    {0} {1} found", search.Matches.Count, matchWord);
                Int32 nReturnId = SnarlConnector.ShowMessage("Rummage - search complete", snarlResult, 10, "", (IntPtr) 0, 0);
            }
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

            if (textBoxFolders.Text.Trim().Length == 0)
            {
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

        private void updateProgressBar(int x, int ofY, string processName)
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
                    searchingX.Text = processName + " file " + x.ToString() + " ";
                }
                if (ofY == 0)
                {
                    searchingOfY.Text = string.Empty;
                }
                else
                {
                    searchingOfY.Text = "of " + ofY.ToString();
                }
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
                                     searchingX.Text = processName + " file " + x.ToString() + " ";
                                     if (ofY == 0)
                                     {
                                         searchingOfY.Text = string.Empty;
                                     }
                                     else
                                     {
                                         searchingOfY.Text = "of " + ofY.ToString();
                                     }
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
            string invalidRegex = string.Empty;

            for (int index = 0; index < textBoxSearchStrings.LineCount; index++)
            {
                string line = textBoxSearchStrings.GetLineText(index).Trim();
                if (line.Length > 0)
                {
                    try
                    {
                        Regex rx = new Regex(line);
                    }
                    catch (ArgumentException)
                    {
                        invalidRegex = line;
                        break;
                    }
                }
            }

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
            if (textBoxFolders == null)
            {
                return false;
            }

            string missingFolder = string.Empty;
            bool result = true;

            for (int index = 0; index < textBoxFolders.LineCount; index++)
            {
                string line = textBoxFolders.GetLineText(index).Trim();
                if (line.Length > 0 && !Directory.Exists(line))
                {
                    missingFolder = line;
                    break;
                }
            }

            if (missingFolder.Length > 0)
            {
                updateStatus(String.Format("Search Folder {0} doesn't exist", missingFolder));
                textBoxFolders.BorderBrush = Brushes.Red;
                textBoxFolders.BorderThickness = new Thickness(3, 3, 3, 3);
                result = false;
            }
            else
            {
                if (textBlockCurrentStatus.Text.StartsWith("Search Folder ") && textBlockCurrentStatus.Text.EndsWith("doesn't exist"))
                {
                    updateStatus(String.Empty);
                }
                textBoxFolders.BorderBrush = normalBorderBrush;
                textBoxFolders.BorderThickness = new Thickness(1, 1, 1, 1);
                result = true;
            }

            return result;
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
            updateProgressBar(0, 0, string.Empty);
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
                updateProgressBar(iea.FileX, iea.FileOfY, "Searching");

                foreach (IMatch match in iea.Matches)
                {
                    if (match.Successful)
                    {
                        addToMatches(match);
                        updateDocument(flowResults, formatOutputLine(match));

                    }
                    else
                    {
                        updateDocument(flowResults,
                                       string.Format("Couldn't search {0}:{1}", match.MatchItem, match.ErrorMessage));
                    }
                }
            }
        }
 
        /// <summary>
        /// Whenever the text changes we'll check whether the entered folders exist
        /// </summary>
        /// <param name="sender">Standard sender object</param>
        /// <param name="e"></param>
        private void textBoxFolders_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableDisableSearchButton();
        }

        void searchRequest_NotifyProgress(object sender, EventArgs e)
        {
            NotifyProgressEventArgs nea = e as NotifyProgressEventArgs;

            if (nea != null)
            {
                updateProgressBar(nea.NumberOfFilesScanned, 0, "Examining");
            }
        }


        private void textBoxSearchStrings_TextChanged(object sender, TextChangedEventArgs e)
        {
            enableDisableSearchButton();
        }


        private void listViewMatches_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (listViewMatches.SelectedItem != null)
            {
                MatchingItem selectedItem = listViewMatches.SelectedItem as MatchingItem;

                if (selectedItem != null)
                {
                    ProcessStartInfo procStartInfo = new ProcessStartInfo(@"c:\vim\vim73\gvim.exe");
                    procStartInfo.Arguments = @"""" + selectedItem.ItemKey + @"""";
                    procStartInfo.CreateNoWindow = true;
                    procStartInfo.UseShellExecute = true;
                    Process.Start(procStartInfo);
                }
            }

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
                textBoxFolders.Text += "\n" + fbd.SelectedPath;
            }
        }

        #endregion

    }
}
