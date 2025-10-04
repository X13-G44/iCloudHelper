using QuickSort.view;
using QuickSort;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;



namespace QuickSort
{
    public partial class App : Application
    {
        public const string APP_TITLE = "iCloudHelper";



        public string StartPath { get; private set; }



        public App ()
        {
            ;
        }



        protected override void OnStartup (StartupEventArgs e)
        {
            string starPath = e.Args.Length == 1 ? e.Args[0] : string.Empty;


            base.OnStartup (e);

            if (string.IsNullOrEmpty (starPath) || !Directory.Exists (starPath))
            {
                // No path argument. Use default start path from configuration.
                if (CheckConfiguration () == false)
                {
                    if (System.Windows.MessageBox.Show ($"Your configuration is invalid or has not been setup yet.\nFor this program to work properly, " +
                        $"the directories / folders must be configured correctly.\n\n" +
                        $"Should the configuration window be displayed so that the settings can be adjusted?",
                        $"{APP_TITLE} - Warning",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        // Show configuration window.

                        var dialog = new ConfigView ();


                        dialog.ShowDialog ();

                        if (dialog.DialogResult.Value)
                        {
                            this.StartPath = QuickSort.Properties.Settings.Default.StartPath;
                        }
                        else
                        {
                            // User closed window without changing the wrong settings => Exit.
                            this.Shutdown ();

                            return;
                        }
                    }
                    else
                    {
                        // No valid start path argument + No valid start path setting + User does not want to fix configuration => Exit.
                        this.Shutdown ();

                        return;
                    }
                }
                else
                {
                    this.StartPath = QuickSort.Properties.Settings.Default.StartPath;
                }
            }
            else
            {
                this.StartPath = starPath;
            }


            // Start and show main UI window.
            var mainView = new MainView ();
            mainView.Show ();
        }



        protected override void OnExit (ExitEventArgs e)
        {
            QuickSort.Properties.Settings.Default.Save ();

            base.OnExit (e);
        }



        static public bool CheckConfiguration ()
        {
            try
            {
                if (!String.IsNullOrEmpty (QuickSort.Properties.Settings.Default.StartPath))
                {
                    return Directory.Exists (QuickSort.Properties.Settings.Default.StartPath);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
