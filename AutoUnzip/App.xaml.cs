/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				06.10.2025
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2025 Christian Harscher (alias X13-G44)
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the Apache License as
/// published by the Free Software Foundation, either version 2 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// Apache License for more details.
///
/// You should have received a copy of the Apache License
/// along with this program. If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
///      
/// ////////////////////////////////////////////////////////////////////////



using AutoUnzip.Model;
using AutoUnzip.Resources;
using AutoUnzip.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms; // For NotifyIcon
using WPFLocalizeExtension.Engine;



namespace AutoUnzip
{
    public partial class App : System.Windows.Application
    {
        public const string APP_TITLE = "iCloudHelper";
        public const string QUICKSORT_FILENAME = "QuickSort.exe";


        public readonly CultureInfo SystemCultureInfo;


        private FileSystemWatcher _Watcher = null;
        private NotifyIcon _NotifyIcon = null;



        public App ()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture; // Setup default culture info.
            this.SystemCultureInfo = CultureInfo.CurrentUICulture;              // Get and store windows default culture info.


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
            string startParam = e.Args.Length == 1 ? e.Args[0] : string.Empty;


            // Update the UI language (for dlg message box).
            SetUiLanguage ();

            if (String.IsNullOrEmpty (GetQuickSortFile ()))
            {
                System.Windows.MessageBox.Show (LocalizedStrings.GetFormattedString ("dlg_MissingFile", App.QUICKSORT_FILENAME),
                                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                Shutdown ();
                return;
            }

            if (startParam.ToLower () == "showconfig")
            {
                #region Show Configuration Window

                // Load system configuration.
                ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration ();

                ConfigView configView = new ConfigView ();
                configView.ShowDialog ();

                if (configView.DialogResult.Value == true)
                {
                    // Configuration data was changed and saved.

                    App.Current.Shutdown (1);
                }
                else
                {
                    // User closed the window without saving.
                    App.Current.Shutdown (0);
                }

                #endregion
            }
            else
            {
                #region Normal App Start

                START:

                // Load system configuration.
                if (ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration ())
                {
                    // Configuration is okay. Check directories.

                    if (FileWorkModel.CheckFolder (false) != true)
                    {
                        if (System.Windows.MessageBox.Show (LocalizedStrings.GetString ("dlg_InvalidConfigDirs"),
                            $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Warning) == MessageBoxResult.Yes)
                        {
                            // Show configuration window.

                            ConfigView dialog = new ConfigView ();
                            dialog.ShowDialog ();

                            if (dialog.DialogResult == false)
                            {
                                // User closed window without changing the wrong settings => Exit.

                                Shutdown ();
                                return;
                            }

                            goto START;
                        }
                        else
                        {
                            // setting + User does not want to fix configuration => Exit.

                            Shutdown ();
                            return;
                        }
                    }
                }
                else
                {
                    // No configuration is present. Create new one + default directories.

                    if (System.Windows.MessageBox.Show (LocalizedStrings.GetFormattedString ("dlg_CreateDefaultConfig",
                            ConfigurationStorage.ConfigurationStorageModel.MonitoringPath,
                            ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath,
                            ConfigurationStorage.ConfigurationStorageModel.BackupPath),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        // User want default config + directories.
                        // Try to create default directories...

                        try
                        {
                            Directory.CreateDirectory (ConfigurationStorage.ConfigurationStorageModel.MonitoringPath);
                            Directory.CreateDirectory (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath);
                            Directory.CreateDirectory (ConfigurationStorage.ConfigurationStorageModel.BackupPath);

                            ConfigurationStorage.ConfigurationStorageModel.SaveConfiguration ();
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show (LocalizedStrings.GetString ("dlg_CreateDefaultDirError"),
                                $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                            Shutdown ();
                            return;
                        }
                    }
                    else
                    {
                        // User don't want use default configuration. Show config dlg.

                        ConfigView dialog = new ConfigView ();
                        dialog.ShowDialog ();

                        if (dialog.DialogResult == false)
                        {
                            // User closed the config window without saving the config.

                            Shutdown ();
                            return;
                        }
                    }
                }

                // Update the UI language.
                SetUiLanguage ();

                // Start file monitor.
                Init_FileMonitoring ();

                // Show system tray icon.
                Init_TrayNotifyIcon ();

                #endregion
            }
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

            if (_NotifyIcon != null)
            {
                _NotifyIcon.Visible = false;
                _NotifyIcon.Dispose ();
            }

            _NotifyIcon = new NotifyIcon ();
            _NotifyIcon.Icon = icon;
            _NotifyIcon.Visible = true;
            _NotifyIcon.Text = $"{APP_TITLE} - AutoUnzip";

            // Make and add context menu items to the tray icon.
            var contextMenu = new ContextMenu ();
            contextMenu.MenuItems.Add (LocalizedStrings.GetString ("dlg_TrayNotiOpenICloud"), (s, ev) =>
            {
                try
                {
                    Process.Start ("https://www.icloud.com");
                }
                catch
                {; }
            });
            if (String.IsNullOrEmpty (GetQuickSortFile ()) == false)
                contextMenu.MenuItems.Add (LocalizedStrings.GetString ("dlg_TrayNotiStartQuickSort"), (s, ev) =>
                {
                    try
                    {
                        Process.Start (GetQuickSortFile ());
                    }
                    catch
                    {; }
                });
            contextMenu.MenuItems.Add (LocalizedStrings.GetString ("dlg_TrayNotiStartSerachManual"), (s, ev) =>
            {
                try
                {
                    string[] files = Directory.GetFiles (ConfigurationStorage.ConfigurationStorageModel.MonitoringPath, ConfigurationStorage.ConfigurationStorageModel.MonitoringFilename);

                    foreach (string file in files)
                    {
                        NewImageArchiveFileDetected (file);
                    }
                }
                catch
                {; }
            });
            contextMenu.MenuItems.Add ("-");
            contextMenu.MenuItems.Add (LocalizedStrings.GetString ("dlg_TrayNotiSettings"), (s, ev) =>
            {
                this.Dispatcher.Invoke (() =>
                {
                    ConfigView dialog = new ConfigView ();
                    dialog.ShowDialog ();

                    if (dialog.DialogResult == true)
                    {
                        // User closed the config window by pressing the save button --> new configuration.

                        SetUiLanguage ();           // Update the UI language.
                        Init_FileMonitoring ();     // Restart file monitoring with new settings.
                        Init_TrayNotifyIcon ();     // Update the tray notify icon with new UI language.
                    }
                });
            });
            contextMenu.MenuItems.Add ("-");
            contextMenu.MenuItems.Add (LocalizedStrings.GetString ("dlg_TrayNotiExit"), (s, ev) =>
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
            if (_Watcher != null)
            {
                _Watcher.EnableRaisingEvents = true;
                _Watcher.Dispose ();
            }

            _Watcher = new FileSystemWatcher (ConfigurationStorage.ConfigurationStorageModel.MonitoringPath, ConfigurationStorage.ConfigurationStorageModel.MonitoringFilename)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
            };

            _Watcher.Created += (s, ev) => NewImageArchiveFileDetected (ev.FullPath);
            _Watcher.Renamed += (s, ev) => NewImageArchiveFileDetected (ev.FullPath);
        }



        /// <summary>
        /// Function is called, when a new image file archive was detected 
        /// and the file content must be processed.
        /// </summary>
        /// <param name="file"></param>
        private void NewImageArchiveFileDetected (string file)
        {
            // Process the new detected image archive file.

            var workResult = FileWorkModel.DoWork (file);
            if (workResult.WorkSuccess)
            {
                ShowMainWindow (workResult.ExtractedFiles);
            }
            else
            {
                System.Windows.MessageBox.Show (LocalizedStrings.GetFormattedString (
                        "dlg_DoWorkError",
                        file,
                        ConfigurationStorage.ConfigurationStorageModel.MonitoringFilename,
                        ConfigurationStorage.ConfigurationStorageModel.BackupPath,
                        workResult.ErrorMessage),
                    $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }



        /// <summary>
        /// Show main view.
        /// </summary>
        /// <param name="extractedFiles"></param>
        private void ShowMainWindow (List<String> extractedFiles)
        {
            this.Dispatcher.Invoke (() =>
            {
                this.MainWindow = new MainWindow (extractedFiles);
                this.MainWindow.Show ();
            });
        }



        private static string GetEnumDescription (Enum value)
        {
            var fi = value.GetType ().GetField (value.ToString ());
            var attributes = (System.ComponentModel.DescriptionAttribute[]) fi.GetCustomAttributes (typeof (System.ComponentModel.DescriptionAttribute), false);


            return (attributes.Length > 0) ? attributes[0].Description : value.ToString ();
        }



        /// <summary>
        /// Update / set UI language by our configuration.
        /// See property "ConfigurationStorageModel.LanguageId".
        /// </summary>
        public void SetUiLanguage ()
        {
            switch (ConfigurationStorage.ConfigurationStorageModel.LanguageId)
            {
                default:
                case 0:
                    {
                        LocalizedStrings.SetCulture (this.SystemCultureInfo.Name);

                        break;
                    }

                case 1:
                    {
                        LocalizedStrings.SetCulture ("de-DE");
                        break;
                    }

                case 2:
                    {
                        LocalizedStrings.SetCulture ("en-US");
                        break;
                    }
            }
        }



        /// <summary>
        /// Get the path to QuickSort application ("QuickSort.exe").
        /// </summary>
        /// <returns>File (Filename + Path) to "QuickSort.exe". On error, an empty string is returned. </returns>
        public string GetQuickSortFile ()
        {
            try
            {
                String quicksortFile = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, QUICKSORT_FILENAME);


                if (File.Exists (quicksortFile))
                {
                    return quicksortFile;
                }

                return String.Empty;
            }
            catch
            {
                return String.Empty;
            }
        }

    }
}
