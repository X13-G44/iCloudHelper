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



using QuickSort;
using QuickSort.Resources;
using QuickSort.view;
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



        public string StartPath { get; private set; }



        public App ()
        {
            LocalizeDictionary.Instance.Culture = CultureInfo.CurrentUICulture;
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
                    if (System.Windows.MessageBox.Show (LocalizedStrings.GetString ("dlg_InvalidConfig"),
                        $"{App.APP_TITLE} - {LocalizedStrings.GetString ("lWarning")}",
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
