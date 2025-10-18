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
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;    // Für Icon
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms; // Für NotifyIcon
using WPFLocalizeExtension.Engine;



namespace AutoUnzip
{
    public partial class App : System.Windows.Application
    {
        public const string APP_TITLE = "iCloudHelper";


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
            // Update the UI language (for dlg message box).
            SetUiLanguage ();

            START:

            if (FileWorkModel.CheckFolder (false) != true)
            {
                if (System.Windows.MessageBox.Show (LocalizedStrings.GetString ("dlg_InvalidConfig"),
                    $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
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

            // Update the UI language.
            SetUiLanguage ();

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
                _Watcher.EnableRaisingEvents = false;
                _Watcher.Dispose ();
            }

            _Watcher = new FileSystemWatcher (AutoUnzip.Properties.Settings.Default.WatchPath, AutoUnzip.Properties.Settings.Default.FilenameToSearch)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime,
                EnableRaisingEvents = true,
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
                    System.Windows.MessageBox.Show (LocalizedStrings.GetFormattedString (
                            "dlg_AchiveFileAccessError",
                            ev.Name,
                            AutoUnzip.Properties.Settings.Default.WatchPath,
                            ex.Message),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }

                // Process the new file.
                var workResult = FileWorkModel.DoWork (ev.FullPath);
                if (workResult.WorkSuccess)
                {
                    ShowMainWindow (workResult.ExtractedFiles);
                }
                else
                {
                    System.Windows.MessageBox.Show (LocalizedStrings.GetFormattedString (
                            "dlg_DoWorkError",
                            ev.Name,
                            AutoUnzip.Properties.Settings.Default.WatchPath,
                            AutoUnzip.Properties.Settings.Default.BackupPath,
                            workResult.ErrorMessage),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };
        }



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



        public void SetUiLanguage ()
        {
            switch (AutoUnzip.Properties.Settings.Default.Language)
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
    }
}
