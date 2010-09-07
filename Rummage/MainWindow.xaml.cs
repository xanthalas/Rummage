using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using System.Threading;
using System.Threading.Tasks;
using RummageCore;
using RummageFilesystem;

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

        private ISearch search;
        private ISearchRequest searchRequest;
        private DateTime startTime;

        /// <summary>
        /// Holds all the matches to be displayed to the user
        /// </summary>
        private Dictionary<String, List<IMatch>> matches;

        /// <summary>
        /// Indicates whether a search is currently running
        /// </summary>
        private bool searchRunning = false;

        private bool cancellingSearch = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!searchRunning)
            {
                if (checkParms())
                {
                    btnStart.Content = "Cancel Search";
                    cancellingSearch = false;
                    doSearch();
                }
            }
            else
            {
                updateResults("Search cancelled.");
                if (searchRequest != null)
                {
                    searchRequest.CancelRequest();
                }
                if (search != null)
                {
                    search.CancelSearch();
                }

                cancellingSearch = true;
                btnStart.Content = "Start Search";
            }
        }

        private void doSearch()
        {
            startTime = DateTime.Now;
            //textBlockResults.Text = string.Empty;
            flowResults.Blocks.Clear();
            listViewMatches.Items.Clear();
            matches = new Dictionary<string, List<IMatch>>();

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
                
            search = new SearchFilesystem();
            search.ItemSearched += new ItemSearchedEventHandler(search_ItemSearched);
            searchRunning = true;
            performSearchOnSeparateThread(search, searchRequest);

        }


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

            /*
            t.ContinueWith(task =>
                                this.Dispatcher.BeginInvoke(new Action(() =>
                                                                        updateStatus("")
                                                           )
                                                            , null));
            */
            tasks.Add(t);


            Task.Factory.ContinueWhenAll(tasks.ToArray(),
                                            result =>
                                            {
                                                this.Dispatcher.BeginInvoke(new Action(() =>
                                                                                    searchComplete())
                                                                            , null);
                                            });

        }

        void search_ItemSearched(object sender, EventArgs e)
        {
            var iea = e as ItemSearchedEventArgs;

            foreach (IMatch match in iea.Matches)
            {
                if (match.Successful)
                {
                    if (addToMatches(match))
                    {
                        updateList(match);      //Only add it to the list if it is a new match    
                    }
                    updateResults(formatOutputLine(match));
                    
                }
                else
                {
                    updateResults(string.Format("Couldn't search {0}:{1}", match.MatchItem, match.ErrorMessage));
                }
            }

        }

        private void updateList(IMatch match)
        {
            // Checking if this thread has access to the object.
            if (listViewMatches.Dispatcher.CheckAccess())
            {
                ListViewItem newItem = new ListViewItem();
                newItem.Content = match.MatchItem + ": Line " + match.MatchLineNumber;
                newItem.Tag = match;

                listViewMatches.Items.Add(newItem);
            }
            else
            {
                // Place the update method on the Dispatcher of the UI thread.
                flowResults.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                                 {
                                     ListViewItem newItem = new ListViewItem();
                                     newItem.Content = match.MatchItem + ": Line " + match.MatchLineNumber;
                                     newItem.Tag = match;

                                     listViewMatches.Items.Add(newItem);
                                 }
                                 ));
            }
        }

        private void prepareAndSearch(ISearchRequest request, ISearch search)
        {
            updateStatus("");
            updateResults("Determining which files to search...");
            request.Prepare();
            updateResults(string.Format("Searching {0} files...", searchRequest.Urls.Count));

            if (!cancellingSearch)
            {
                search.Search(request, true);
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
                string result = String.Format("Search complete. {0} matches found in {1} files out of {2} files searched ({3} seconds)",
                                              search.Matches.Count, matches.Count ,searchRequest.Urls.Count, elapsed.TotalSeconds);
                updateResults(result);
                updateStatus(result);
            }
            btnStart.Content = "Start Search";
            searchRunning = false;
            cancellingSearch = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void updateResults(string message)
        {
            // Checking if this thread has access to the object.
            if (flowResults.Dispatcher.CheckAccess())
            {
                Paragraph p = new Paragraph();
                p.FontSize = 10;
                p.Inlines.Add(message);

                // This thread has access so it can update the UI thread.      
                flowResults.Blocks.Add(p);
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
                flowResults.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                                 {
                                     Paragraph p = new Paragraph();
                                     p.FontSize = 10;
                                     p.Inlines.Add(message);
                                     flowResults.Blocks.Add(p);
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
        /// 
        /// </summary>
        /// <param name="message"></param>
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
        private bool addToMatches(IMatch match)
        {
            bool isANewMatch = false;

            if (matches.ContainsKey(match.MatchItem))
            {
                List<IMatch> matchesForThisItem = matches[match.MatchItem];
                matchesForThisItem.Add(match);
                isANewMatch = false;
            }
            else
            {
                List<IMatch> newMatch = new List<IMatch>();
                matches.Add(match.MatchItem, newMatch);
                isANewMatch = true;
            }

            return isANewMatch;
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

    }
}
