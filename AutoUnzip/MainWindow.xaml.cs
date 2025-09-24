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
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> LastExtractedFiles { get; set; } = new ObservableCollection<string> ();

        private string _Message;



        public string Message
        {
            get { return _Message; }
            set { _Message = value; OnPropertyChanged (nameof (Message)); }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        public MainWindow ()
        {
            InitializeComponent ();

            DataContext = this;
            this.Opacity = 0; // Unsichtbar starten
        }



        public void ShowExtractedFiles (List<string> fileCollection)
        {
            const int MAX_FILES_TO_SHOW = 5;
            int selectedFiles = 0;


            this.LastExtractedFiles.Clear ();

            if (fileCollection.Count > MAX_FILES_TO_SHOW)
            {
                Random rnd = new Random ();


                while (selectedFiles <= MAX_FILES_TO_SHOW)
                {
                    int rndFileIndex = rnd.Next (fileCollection.Count);


                    this.LastExtractedFiles.Add (fileCollection[rndFileIndex]);

                    selectedFiles++;
                }
            }
            else
            {
                foreach (string file in fileCollection)
                {
                    this.LastExtractedFiles.Add (file);
                }
            }

            this.Message = $"{fileCollection.Count} iCloud Bilder extrahiert:";

            RelocateWindow ();

            StartFadeinAnimation ();
        }



        private void RelocateWindow ()
        {
            const int MARGIN = 20;


            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            var workingArea = primaryScreen.WorkingArea;

            double windowWidth = this.Width;
            double windowHeight = this.Height;

            double newLeft = workingArea.Right - windowWidth - MARGIN;
            double newTop = workingArea.Bottom - windowHeight - MARGIN;


            this.Top = newTop;
            this.Left = newLeft;
        }



        private void StartFadeinAnimation ()
        {
            var fadeInAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration (TimeSpan.FromSeconds (1.5))
            };

            //fadeInAnimation.Completed += (s, e) => StartFadeoutAnimationWithDelay ();

            this.BeginAnimation (UIElement.OpacityProperty, fadeInAnimation);
        }



        private async void StartFadeoutAnimationWithDelay ()
        {
            await Task.Delay (TimeSpan.FromSeconds (10));
            StartFadeoutAnimation ();
        }



        private void StartFadeoutAnimation ()
        {
            var fadeOutAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration (TimeSpan.FromSeconds (3))
            };

            fadeOutAnimation.Completed += (s, e) => this.Close ();

            this.BeginAnimation (UIElement.OpacityProperty, fadeOutAnimation);
        }



        private void Border_MouseLeftButtonUp (object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty (Properties.Settings.Default.QuickSortApp) == false &&
                    File.Exists (Properties.Settings.Default.QuickSortApp))
                {
                    Process.Start (Properties.Settings.Default.QuickSortApp, "\"" + Properties.Settings.Default.ExtractPath + "\"");
                }

                this.Close ();
            }
            catch
            {
                this.Close ();
            }
        }



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
