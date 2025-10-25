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



using QuickSort.Resources;
using QuickSort.View;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WPFLocalizeExtension.Engine;



namespace QuickSort
{
    public partial class App : Application
    {
        public const string APP_TITLE = "iCloudHelper";
        public const string AUTOUNZIP_FILENAME = "AutoUnzip.exe";

        public readonly CultureInfo SystemCultureInfo;



        public string StartPath { get; private set; }



        public App ()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture; // Setup default culture info.
            this.SystemCultureInfo = CultureInfo.CurrentUICulture;              // Get and store windows default culture info.
        }



        protected override void OnStartup (StartupEventArgs e)
        {
            MainView mainView = null;
            string starPath = e.Args.Length == 1 ? e.Args[0] : string.Empty;


            base.OnStartup (e);

            // Update the UI language (for dlg message box).
            SetUiLanguage ();

            if (String.IsNullOrEmpty (GetAutoUnzipFile ()))
            {
                MessageBox.Show (LocalizedStrings.GetFormattedString ("dlg_MissingFile", App.AUTOUNZIP_FILENAME),
                                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Error);

                Shutdown ();
                return;
            }


#warning 'Check and solve this problem'
            // Initialize main UI window.
            // We need this to do before we show the ConfigurationWindow. Without this, the application will exit, then we instantiate the mainView class!?
            mainView = new MainView ();


            // Load system configuration.
            if (ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration () == true)
            {
                // Configuration is okay. Check directories.

                if (CheckConfiguration (false) == false)
                {
                    if (MessageBox.Show (LocalizedStrings.GetString ("dlg_InvalidConfigDirs"),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        // Show configuration window.
                        // Start AutoUnzop with parameter "showconfig" to show configuration dialog window.

                        Process prc = Process.Start (GetAutoUnzipFile (), "showconfig");
                        prc.WaitForExit ();


                        if (prc.ExitCode == 1)
                        {
                            // User closed the config window without saving the config.
                            // If result code is "1", then user have saved configuration.

                            ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration ();

                            StartPath = ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath;
                        }
                        else
                        {
                            // User closed window without changing the wrong settings => Exit.

                            Shutdown ();
                            return;
                        }
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

                if (MessageBox.Show (LocalizedStrings.GetFormattedString ("dlg_CreateDefaultConfig",
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
                    catch
                    {
                        MessageBox.Show (LocalizedStrings.GetString ("dlg_CreateDefaultDirError"),
                            $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lError")}",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);

                        Shutdown ();
                        return;
                    }
                }
                else
                {
                    // Start AutoUnzop with parameter "showconfig" to show configuration dialog window.

                    Process prc = Process.Start (GetAutoUnzipFile (), "showconfig");
                    prc.WaitForExit ();

                    if (prc.ExitCode != 1)
                    {
                        // User closed the config window without saving the config.
                        // If result code is "1", then user have saved configuration.

                        Shutdown ();
                        return;
                    }
                    else
                    {
                        ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration ();
                    }
                }
            }

            // Select working root path.
            if (string.IsNullOrEmpty (starPath) || !Directory.Exists (starPath))
            {
                if (string.IsNullOrEmpty (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath) || !Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath))
                {
                    starPath = ConfigurationStorage.SpecialFolders.GetPicturePath ();
                }
                else
                {
                    this.StartPath = ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath;
                }
            }
            else
            {
                this.StartPath = starPath;
            }

            // Update the UI language.
            SetUiLanguage ();

            // Start and show main UI window.
            mainView = new MainView ();
            mainView.Show ();
        }



        /// <summary>
        /// Exit application and save / update configuration.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit (ExitEventArgs e)
        {
            ConfigurationStorage.ConfigurationStorageModel.SaveConfiguration ();

            base.OnExit (e);
        }



        static public bool CheckConfiguration (bool allowExceptionOnError)
        {
            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Extract directory \"" + ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath + "\" does not exists.");
                }

                return false;
            }

            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.BackupPath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Backup directory \"" + ConfigurationStorage.ConfigurationStorageModel.BackupPath + "\" does not exists.");
                }

                return false;
            }

            if (Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.MonitoringPath) == false)
            {
                if (allowExceptionOnError)
                {
                    throw new FileNotFoundException ("Monitoring directory \"" + ConfigurationStorage.ConfigurationStorageModel.MonitoringPath + "\" does not exists.");
                }

                return false;
            }

            return true;
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
        /// Get the path to AutoUnzip application ("AutoUnzip.exe").
        /// </summary>
        /// <returns>File (Filename + Path) to "AutoUnzip.exe". On error, an empty string is returned. </returns>
        public string GetAutoUnzipFile ()
        {
            try
            {
                String quicksortFile = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, AUTOUNZIP_FILENAME);


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
