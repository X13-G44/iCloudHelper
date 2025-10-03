using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;    // Für Icon
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms; // Für NotifyIcon
using AutoUnzip.view;



namespace AutoUnzip
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public const string APP_TITLE = "iCloudHelper";



        private FileSystemWatcher _Watcher;
        private NotifyIcon _NotifyIcon;
        private WorkStuffClass.FileProcessingCheckpoint _Checkpoint;



        public App ()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Startup += App_Startup;
            Exit += App_Exit;
        }



        private void App_Exit (object sender, ExitEventArgs e)
        {
            if (_NotifyIcon != null)
            {
                _NotifyIcon.Visible = false;
            }
        }



        private void App_Startup (object sender, StartupEventArgs e)
        {
            // Start file monitor.
            Init_FileMonitoring ();

            // Show system tray icon.
            Init_TrayNotifyIcon ();
        }



        /// <summary>
        /// Show system try icon and add context menu items to it,
        /// </summary>
        private void Init_TrayNotifyIcon ()
        {
            Icon icon = SystemIcons.Information;
            Assembly assembly = Assembly.GetExecutingAssembly ();


            using (Stream stream = assembly.GetManifestResourceStream ("AutoUnzip.Resources.icloud-logo-49272-Windows.ico"))
            {
                if (stream != null)
                {
                    icon = new Icon (stream);
                }
            }

            _NotifyIcon = new NotifyIcon ();
            _NotifyIcon.Icon = icon;
            _NotifyIcon.Visible = true;
            _NotifyIcon.Text = "AutoUnzip (Running in background)";

            // Make and add context menu items to the tray icon.
            var contextMenu = new ContextMenu ();
            contextMenu.MenuItems.Add ("Einstellungen", (s, ev) =>
            {
                this.Dispatcher.Invoke (() =>
                {
                    ConfigView dialog = new ConfigView ();
                    dialog.ShowDialog ();

                    if (dialog.DialogResult == true)
                    {
                        // User closed the config window by pressing the save button --> new configuration.

                        Init_FileMonitoring (); // Restart file monitoring with new settings.
                    }
                });
            });
            contextMenu.MenuItems.Add ("-");
            contextMenu.MenuItems.Add ("Anwendung beenden", (s, ev) =>
            {
                _NotifyIcon.Visible = false;
                Shutdown ();
            });
            _NotifyIcon.ContextMenu = contextMenu;
        }



        /// <summary>
        /// Start file monitor by using a FileSystemWatcher instance.
        /// </summary>
        /// <exception cref="DirectoryNotFoundException"></exception>
        private void Init_FileMonitoring ()
        {
            START:

            if (WorkStuffClass.CheckFolder (false) != true)
            {
                if (System.Windows.MessageBox.Show ($"At least one required directory is missing.\nFor this program to work properly, " +
                    $"the directories / folders must be configured correctly.\n\n" +
                    $"Should the configuration window be displayed so that the settings can be adjusted?",
                    $"{APP_TITLE} - Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    ConfigView dialog = new ConfigView ();
                    dialog.ShowDialog ();

                    if (dialog.DialogResult == false)
                    {
                        // User closed the config window without saving the config.

                        Shutdown ();
                        return;
                    }

                    goto START;
                }
                else
                {
                    Shutdown ();
                    return;
                }
            }

            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Dispose ();
            }

            _Watcher = new FileSystemWatcher (AutoUnzip.Properties.Settings.Default.WatchPath, AutoUnzip.Properties.Settings.Default.FilenameToSearch)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            _Watcher.Created += (s, ev) =>
            {
                try
                {
                    // Wait until the files becomes unlocked.
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            using (FileStream fs = File.Open (ev.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep (500);
                        }
                    }

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show ($"An error occurred while waiting for file access to \"{ev.Name}\" located in folder \"{AutoUnzip.Properties.Settings.Default.WatchPath}\".\n\n" +
                        $"No backup files was created!" +
                        $"System error message is \"{ex.Message}\"",
                        $"{APP_TITLE} - Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                // Process the new file.
                var workResult = WorkStuffClass.DoWork (ev.FullPath);
                if (workResult.WorkSuccess)
                {
                    ShowMainWindow (workResult.ExtractedFiles);
                }
                else
                {
                    System.Windows.MessageBox.Show ($"An error occurred while processing a new \"{ev.Name}\" file in \"{AutoUnzip.Properties.Settings.Default.WatchPath}\" folder.\n" +
                        $"The error occurred at the following processing step: {GetEnumDescription (workResult.LastCheckpoint)}.\n\n" +
                        $"Backup files exist in the folder \"{AutoUnzip.Properties.Settings.Default.BackupPath}\".\n\n" +
                        $"Optional error message \"{workResult.ErrorMessage}\"",
                        $"{APP_TITLE} - Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            _Watcher.EnableRaisingEvents = true;
        }



        private void ShowMainWindow (List<String> extractedFiles)
        {
            this.Dispatcher.Invoke (() =>
            {
                if (this.MainWindow == null)
                {
                    this.MainWindow = new MainWindow ();
                }

                if (this.MainWindow.IsVisible == false)
                {
                    this.MainWindow.Show ();
                }
                else
                {
                    this.MainWindow.Activate ();
                }

                (this.MainWindow as MainWindow).ShowExtractedFiles (extractedFiles);
            });
        }



        private static string GetEnumDescription (Enum value)
        {
            var fi = value.GetType ().GetField (value.ToString ());
            var attributes = (System.ComponentModel.DescriptionAttribute[]) fi.GetCustomAttributes (typeof (System.ComponentModel.DescriptionAttribute), false);


            return (attributes.Length > 0) ? attributes[0].Description : value.ToString ();
        }
    }
}
