using System;
using System.Collections.Generic;
using System.ComponentModel;
using RummageCore;
using System.IO;

namespace Rummage
{
    /// <summary>
    /// Holds a collection of all the matches for a given item
    /// </summary>
    public class MatchingItem : INotifyPropertyChanged, IComparable
    {
        private string itemKey;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The key for this collection of matches. Typically this is the item name
        /// </summary>
        public string ItemKey
        {
            get { return this.itemKey; }
            set 
            {
                if (this.itemKey != value)
                {
                    this.itemKey = value;
                    NotifyPropertyChanged("ItemKey");
                }
            }
        }

        /// <summary>
        /// Returns the extension type of this item
        /// </summary>
        public string ItemType
        {
            get
            {
                FileInfo fi = new FileInfo(itemKey);
                return (fi.Extension.Trim().Length == 0 ? "-" : fi.Extension);
            }
        }

        /// <summary>
        /// Gets the number of items in this collection
        /// </summary>
        public int MatchingLinesCount
        {
            get { return matches.Count; }
        }

        /// <summary>
        /// The collection of matches found for this item
        /// </summary>
        public List<IMatch> matches { get; set; }

        /// <summary>
        /// Create a new MatchingItem
        /// </summary>
        /// <param name="key"></param>
        public MatchingItem(string key)
        {
            ItemKey = key;
            matches = new List<IMatch>();
        }
        /// <summary>
        /// Create a new MatchingItem
        /// </summary>
        /// <param name="key"></param>
        public MatchingItem(string key, IMatch match)
            :this(key)
        {
            matches.Add(match);
        }


        #region INotifyPropertyChanged & IComparable Members

        /// <summary>
        /// Determines whether the MatchingItem passed in is equal to this one
        /// </summary>
        /// <param name="item">The MatchingItem to compare</param>
        /// <returns></returns>
        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if (obj == null || GetType() != obj.GetType()) return false;

            MatchingItem mi = obj as MatchingItem;

            return this.ItemKey == mi.ItemKey;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// Compares the current object to the one passed in. 
        /// </summary>
        /// <param name="obj">The object (usually a ConnectIniData) to compare</param>
        /// <returns>0 if they are equal or -1 if they are not equal</returns>
        public int CompareTo(object obj)
        {
            MatchingItem mi = (MatchingItem)obj;

            if (mi.ItemKey == this.ItemKey)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        #endregion
    }
}
