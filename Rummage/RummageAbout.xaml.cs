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

namespace Rummage
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class RummageAbout : Window
    {
        public RummageAbout()
        {
            InitializeComponent();

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter || e.Key == Key.Space)
            {
                Close();
            }
        }
    }
}
