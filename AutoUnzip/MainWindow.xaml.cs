using AutoUnzip.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace AutoUnzip
{
    public partial class MainWindow : Window
    {
        const int MARGIN = 20;



        public MainWindow (List<String> extractedFiles)
        {
            InitializeComponent ();

            this.DataContext = new viewmodel.MainViewModel (Dispatcher, this, extractedFiles);
            this.Opacity = 0;   // Start hidden

            this.Loaded += (s, e) =>
            {
                // Relocate the window after it has been loaded and its size determined.
                RelocateWindow ();
            };
        }



        private void RelocateWindow ()
        {
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            var workingArea = primaryScreen.WorkingArea;

            double newLeft = workingArea.Right - this.Width - MARGIN;
            double newTop = workingArea.Bottom - this.Height - MARGIN;

            this.Top = newTop;
            this.Left = newLeft;
        }
    }
}
