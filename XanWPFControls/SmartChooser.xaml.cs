using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace XanWPFControls
{
    /// <summary>
    /// Interaction logic for SmartChooser.xaml
    /// </summary>
    public partial class SmartChooser : UserControl
    {
        private ObservableCollection<string> baseItems;
        private ObservableCollection<string> filteredItems;
        private Brush defaultForegroundBrush = Brushes.Black;
        private string fullyQualifiedSaveName = string.Empty;

        public int MaximumEntries = 20;

        /// <summary>
        /// Gets the currently selected item or an empty string if nothing is selected
        /// </summary>
        public string SelectedItem { get; private set; }

        public SmartChooser()
        {
            InitializeComponent();

            defaultForegroundBrush = filterLabel.Foreground;

            baseItems = new ObservableCollection<string>();

            SetFocusToFilter();
        }

        /// <summary>
        /// Loads the chooser from the initialList of strings passed in
        /// </summary>
        /// <param name="initialList"></param>
        public void LoadChooser(List<string> initialList)
        {
            baseItems = new ObservableCollection<string>();

            foreach (var item in initialList)
            {
                ListItem li = new ListItem();
                baseItems.Insert(0, item);
            }

            generateFilteredList(filter.Text.Trim());

            this.list.ItemsSource = filteredItems;
        }


        /// <summary>
        /// Saves the contents of the list with the name specified to the user's default Application Data directory
        /// </summary>
        /// <param name="controlName"></param>
        public void SaveList(string controlName)
        {
            SaveList(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), controlName);

        }

        /// <summary>
        /// Saves the contents of the list with the name specified to the specified directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public bool SaveList(string directory, string controlName)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            string outputFile = Path.Combine(directory, controlName);

            using (StreamWriter sw = new StreamWriter(outputFile))
            {
                string line = Newtonsoft.Json.JsonConvert.SerializeObject(controlName);
                sw.WriteLine(line);

                line = Newtonsoft.Json.JsonConvert.SerializeObject(MaximumEntries);
                sw.WriteLine(line);

                line = Newtonsoft.Json.JsonConvert.SerializeObject(this.MinWidth);
                sw.WriteLine(line);

                line = Newtonsoft.Json.JsonConvert.SerializeObject(this.MinHeight);
                sw.WriteLine(line);

                line = Newtonsoft.Json.JsonConvert.SerializeObject(baseItems);
                sw.WriteLine(line);

                sw.Close();
            }

            return true;
        }


        /// <summary>
        /// Restore the list from a saved file with the name specified in the directory specified
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public bool RestoreList(string controlName)
        {
            return RestoreList(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), controlName);

        }

        /// <summary>
        /// Restore the list from a saved file with the name specified in the user's default Application Data directory
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public bool RestoreList(string directory, string controlName)
        {
            if (!Directory.Exists(directory))
            {
                return false;
            }

            string inputFile = Path.Combine(directory, controlName);

            if (!File.Exists(inputFile))
            {
                return false;
            }
            using (StreamReader reader = new StreamReader(inputFile))
            {
                try
                {
                    string line;

                    line = reader.ReadLine();           //Line 1 (Control name)

                    if (line != null)
                    {
                        fullyQualifiedSaveName = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(line);
                    }

                    line = reader.ReadLine();           //Line 2 (Maximum entries)

                    if (line != null)
                    {
                        MaximumEntries = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(line);
                    }

                    line = reader.ReadLine();           //Line 3 (Control Width)

                    if (line != null)
                    {
                        this.MinWidth = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(line);
                    }

                    line = reader.ReadLine();           //Line 4 (Control height)
                    if (line != null)
                    {
                        this.MinHeight = Newtonsoft.Json.JsonConvert.DeserializeObject<int>(line);
                    }
                    line = reader.ReadLine();           //Line 5 (list contents)

                    if (line != null)
                    {
                        baseItems = Newtonsoft.Json.JsonConvert.DeserializeObject<ObservableCollection<string>>(line);
                    }
                }
                catch (Newtonsoft.Json.JsonReaderException)
                {
                    return false;
                }
                catch (IOException)
                {
                    return false;
                }
            }

            generateFilteredList(filter.Text.Trim());

            return true;
        }


        /// <summary>
        /// Adds a new item to the list. List is unique so add won't do anything if the item is already present.
        /// </summary>
        /// <param name="newItem"></param>
        public void AddItemToList(string newItem)
        {
            if (!baseItems.Contains(newItem))
            {
                if (baseItems.Count >= MaximumEntries)
                {
                    baseItems.RemoveAt(baseItems.Count - 1);          //Remove the oldest
                }

                baseItems.Insert(0, newItem);
            }
            else
            {
                
            }

            generateFilteredList(filter.Text.Trim());
        }

        /// <summary>
        /// Sets the Filter field to have the focus
        /// </summary>
        public void SetFocusToFilter()
        {
            filter.Focus();
        }

        /// <summary>
        /// Indicates whether this chooser is empty or contains items
        /// </summary>
        public bool IsEmpty { get { return baseItems.Count == 0; } } 


        /// <summary>
        /// Event handler to re-filter the list when the filter criteria changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            generateFilteredList(filter.Text.Trim());
        }

        /// <summary>
        /// Filter the base list using the filter string entered
        /// </summary>
        /// <param name="filterText"></param>
        private void generateFilteredList(string filterText)
        {
            filteredItems = new ObservableCollection<string>();

            bool skipFilter = (filterText.Length == 0);

            Regex regex = null;

            filterLabel.Foreground = defaultForegroundBrush;

            if (!skipFilter)
            {
                try
                {
                    regex = new Regex(filterText, RegexOptions.IgnoreCase);
                    filterLabel.Foreground = Brushes.Green;
                }
                catch (ArgumentException)
                {
                    skipFilter = true;
                    filterLabel.Foreground = Brushes.Red;
                }
            }

            foreach (var baseItem in baseItems)
            {
                if (skipFilter || ( regex != null && regex.Match(baseItem).Success))
                {
                    filteredItems.Add(baseItem);
                }
            }

            this.list.ItemsSource = filteredItems;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (list.SelectedItems.Count > 0)
            {
                SelectedItem = list.SelectedItem.ToString();
            }
        }

        private void filter_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                list.Focus();
            }
        }
    }
}
