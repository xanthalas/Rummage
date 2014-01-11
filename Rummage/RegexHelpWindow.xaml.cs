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
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Rummage
{
    /// <summary>
    /// Interaction logic for RegexHelpWindow.xaml
    /// </summary>
    public partial class RegexHelpWindow : Window
    {
        public RegexHelpWindow()
        {
            InitializeComponent();

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);

            try
            {
                regexHelpDocument.Navigate(Path.Combine(Environment.CurrentDirectory, "regexhelp.html"));
            }
            catch (Exception)
            {
                //Do nothing.
            }

        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
