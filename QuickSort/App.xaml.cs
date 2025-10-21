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

            // Load system configuration.
            bool configOkay = ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration (true);


            base.OnStartup (e);

            // Update the UI language (for dlg message box).
            SetUiLanguage ();

            if (string.IsNullOrEmpty (starPath) || !Directory.Exists (starPath) || configOkay == false)
            {
                // No path argument. Use default start path from configuration.
                if (CheckConfiguration () == false || configOkay == false)
                {
                    if (MessageBox.Show (LocalizedStrings.GetString ("dlg_InvalidConfig"),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {

#warning 'Check and solve this problem'
                        // Initialize main UI window.
                        // We need this to do before we show the ConfigurationWindow. Without this, the application will exit, then we instantiate the mainView instance!?
                        mainView = new MainView ();


                        // Show configuration window.

                        var dialog = new ConfigView ();
                        dialog.ShowDialog ();

                        if (dialog.DialogResult.Value)
                        {
                            this.StartPath = ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath;
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
                    this.StartPath = ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath;
                }
            }
            else
            {
                this.StartPath = starPath;
            }

            // Update the UI language.
            SetUiLanguage ();

            //// Start and show main UI window.
            mainView = new MainView ();
            mainView.Show ();
        }



        protected override void OnExit (ExitEventArgs e)
        {
            ConfigurationStorage.ConfigurationStorageModel.SaveConfiguration ();

            base.OnExit (e);
        }



        static public bool CheckConfiguration ()
        {
            try
            {
                if (!String.IsNullOrEmpty (ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath))
                {
                    return Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath);
                }

                return false;
            }
            catch
            {
                return false;
            }
        }



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
    }
}
