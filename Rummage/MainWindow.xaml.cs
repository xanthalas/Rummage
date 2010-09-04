using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using RummageCore;
using RummageFilesystem;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (checkParms())
            {
                doSearch();
            }
        }

        private void doSearch()
        {
            textBlockResults.Text = string.Empty;

            #region Build search strings and search folders
            ISearchRequest searchRequest = new SearchRequestFilesystem();
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
                
            ISearch search = new SearchFilesystem();
            search.ItemSearched += new ItemSearchedEventHandler(search_ItemSearched);
            //CurrentStatus.Text = "Preparing search...";
            searchRequest.Prepare();
            //CurrentStatus.Text = "Searching...";
            search.Search(searchRequest, true);
            CurrentStatus.Text = "Search complete.";

        }

        void search_ItemSearched(object sender, EventArgs e)
        {
            var iea = e as ItemSearchedEventArgs;

            foreach (IMatch match in iea.Matches)
            {
                if (match.Successful)
                {
                   updateResults(formatOutputLine(match) + "\n");
                }
                else
                {
                    updateResults(string.Format("Couldn't search {0}:{1}", match.MatchItem, match.ErrorMessage));
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void updateResults(string message)
        {
            // Checking if this thread has access to the object.
            if (textBlockResults.Dispatcher.CheckAccess())
            {
                // This thread has access so it can update the UI thread.      
                textBlockResults.Text += message;
            }
            else
            {
                // This thread does not have access to the UI thread.
                // Place the update method on the Dispatcher of the UI thread.
                textBlockResults.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() => { textBlockResults.Text += message; }));
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
    }
}
