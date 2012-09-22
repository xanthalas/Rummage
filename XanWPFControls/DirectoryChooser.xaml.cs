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

namespace XanWPFControls
{
    /// <summary>
    /// Interaction logic for DirectoryChooser.xaml
    /// </summary>
    public partial class DirectoryChooser : UserControl
    {
        private readonly char[] LINE_SEPARATOR = { '\n' };
        private readonly char[] TRIM_CHARACTERS = { '\n', '\r', ' ' };

        private int previousLengthOfText = 0;

        private bool skipEventHandler = false;

        private Key lastKeyPressed;

        public TextBox InternalTextBox
        {
            get { return this.baseText; }
        }

        public event TextChangedEventHandler TextChanged
        {
            add { this.baseText.TextChanged += value; }
            remove { this.baseText.TextChanged -= value; }
        }

        
        public DirectoryChooser()
        {
            InitializeComponent();
            baseText.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            baseText.AcceptsReturn = true;
        }

        private void baseText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (skipEventHandler || lastKeyPressed == Key.Back || lastKeyPressed == Key.Delete)
            {
                previousLengthOfText = baseText.Text.Length;
                return;
            }


            //Figure out which line we are on
            int lineNumber = getLineNumber(baseText.Text, baseText.SelectionStart);
            var savedSelectionStart = baseText.SelectionStart;

            string[] lines = baseText.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            string currentLine = lines[lineNumber];

            if (currentLine.Length > 0)
            {
               var directoryScanner = DirectoryScanner.GetMatchingPath(currentLine);

                if (directoryScanner.MatchingPathTail.Length > 0)
                {
                    lines[lineNumber] = directoryScanner.BaseDirectory + "\\" + directoryScanner.MatchingPathTail;
                    StringBuilder replaceText = new StringBuilder();

                    foreach (var line in lines)
                    {
                        replaceText.AppendLine(line.TrimEnd(TRIM_CHARACTERS));
                    }
                    skipEventHandler = true;
                    baseText.Text = replaceText.ToString().TrimEnd(TRIM_CHARACTERS);
                    skipEventHandler = false;
                    baseText.Select(savedSelectionStart, directoryScanner.MatchingPathTail.Length - directoryScanner.PartialDirectoryCharacters.Length);
                }
                
            }
        }

        /// <summary>
        /// Determine the line number which the user is typing on
        /// </summary>
        /// <param name="text"></param>
        /// <param name="selectionStart"></param>
        /// <returns></returns>
        private int getLineNumber(string text, int selectionStart)
        {
            string textUpToCaret = text.Substring(0, selectionStart);

            //Figure out which line we are on by counting the CRLFs between the start of the text and the cursor position
            int lineCount = 0;
            int startIndex = textUpToCaret.IndexOf(Environment.NewLine);

            while (startIndex >= 0)
            {
                lineCount++;
                startIndex = textUpToCaret.IndexOf(Environment.NewLine, ++startIndex);
            }

            return lineCount;
        }

        /// <summary>
        /// Save details of the last key pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void baseText_PreviewKeyDown_1(object sender, KeyEventArgs e)
        {
            lastKeyPressed = e.Key;
        }

    }
}
