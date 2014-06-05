using System.Windows;
using System.Windows.Input;
using MahApps.Metro.Controls;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for HistoryPopup.xaml
    /// </summary>
    public partial class HistoryPopup : MetroWindow
    {
        /// <summary>
        /// Used for saving and restoring the contents of the list in this history popup
        /// </summary>
        private string controlName;

        /// <summary>
        /// Used when forcing the window to close - normally it just hides itself
        /// </summary>
        private bool forceClose = false;

        /// <summary>
        /// Gets the currently selected item or an empty string if nothing is selected
        /// </summary>
        public string SelectedItem { get; private set; }

        public HistoryPopup(string controlName)
        {
            InitializeComponent();

            this.controlName = controlName;

            chooser.RestoreList(this.controlName);
        }

        /// <summary>
        /// Adds a new item to the list. List is unique so add won't do anything if the item is already present.
        /// </summary>
        /// <param name="newItem"></param>
        public void AddItemToList(string newItem)
        {
            if (chooser != null)
            {
                chooser.AddItemToList(newItem);
            }
        }

        /// <summary>
        /// Saves the contents of this popup
        /// </summary>
        public void SaveList()
        {
            if (chooser != null)
            {
                chooser.SaveList(this.controlName);
            }
        }

        /// <summary>
        /// Show the dialog
        /// </summary>
        /// <returns></returns>
        public new bool? ShowDialog()
        {
            this.chooser.Focus();
            this.chooser.SetFocusToFilter();

            return base.ShowDialog();
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (chooser != null)
            {
                if (chooser.SelectedItem != null && chooser.SelectedItem.Length > 0)
                {
                    SelectedItem = chooser.SelectedItem;
                    this.chooser.Focus();
                    this.chooser.SetFocusToFilter();
                    this.Hide();
                }
                else
                {
                    if (chooser.IsEmpty)
                    {
                        cancel();
                    }
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            cancel();
        }

        /// <summary>
        /// Cancel out of the dialog and close it.
        /// </summary>
        private void cancel()
        {
            this.SelectedItem = string.Empty;
            this.chooser.Focus();
            this.chooser.SetFocusToFilter();
            this.Hide();
        }

        private void chooser_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            btnOk_Click(sender, null);
        }

        /// <summary>
        /// Overide the closing event to hide the window instead of closing it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!forceClose)
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        public void ForceClose()
        {
            forceClose = true;
            this.Close();
        }

    }
}
