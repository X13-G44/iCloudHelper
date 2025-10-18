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



using AutoUnzip.Resources;
using AutoUnzip.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;



namespace AutoUnzip.ViewModel
{
    public class ConfigViewModel : ViewModelBase
    {
        private string _WatchPath;
        public string WatchPath
        {
            get { return _WatchPath; }
            set { _WatchPath = value; OnPropertyChanged (nameof (WatchPath)); }
        }

        private string _FilenameToSearch;
        public string FilenameToSearch
        {
            get { return _FilenameToSearch; }
            set { _FilenameToSearch = value; OnPropertyChanged (nameof (FilenameToSearch)); }
        }

        private string _ExtractPath;
        public string ExtractPath
        {
            get { return _ExtractPath; }
            set { _ExtractPath = value; OnPropertyChanged (nameof (ExtractPath)); }
        }

        private string _TempFolderPrefix;
        public string TempFolderPrefix
        {
            get { return _TempFolderPrefix; }
            set { _TempFolderPrefix = value; OnPropertyChanged (nameof (TempFolderPrefix)); }
        }

        private string _BackupPath;
        public string BackupPath
        {
            get { return _BackupPath; }
            set { _BackupPath = value; OnPropertyChanged (nameof (BackupPath)); }
        }

        private int _BackupRetentionPeriod;
        public int BackupRetentionPeriod
        {
            get { return _BackupRetentionPeriod; }
            set { _BackupRetentionPeriod = value; OnPropertyChanged (nameof (BackupRetentionPeriod)); }
        }

        private string _QuickSortApp;
        public string QuickSortApp
        {
            get { return _QuickSortApp; }
            set { _QuickSortApp = value; OnPropertyChanged (nameof (QuickSortApp)); }
        }

        private bool _UseDarkModeColorTheme = false;
        public bool UseDarkModeColorTheme
        {
            get { return _UseDarkModeColorTheme; }
            set { _UseDarkModeColorTheme = value; OnPropertyChanged (nameof (UseDarkModeColorTheme)); }
        }

        private int _Language;
        public int Language
        {
            get { return _Language; }
            set { _Language = value; OnPropertyChanged (nameof (Language)); }
        }

        private string _AppVersionStr;
        public string AppVersionStr
        {
            get { return _AppVersionStr; }
            set { _AppVersionStr = value; OnPropertyChanged (nameof (AppVersionStr)); }
        }



        public RelayCommand Cmd_BrowseWatchPath
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var dialog = new FolderBrowserDialog ())
                        {
                            if (Directory.Exists (this.WatchPath))
                            {
                                dialog.SelectedPath = this.WatchPath;
                            }
                            
                            dialog.Description = LocalizedStrings.GetString ("dlg_SelectWatchPath");

                            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                this.WatchPath = dialog.SelectedPath;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_BrowseExtractPath
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var dialog = new FolderBrowserDialog ())
                        {
                            if (Directory.Exists (this.ExtractPath))
                            {
                                dialog.SelectedPath = this.ExtractPath;
                            }

                            dialog.Description = LocalizedStrings.GetString("dlg_SelectExtractPath");

                            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                this.ExtractPath = dialog.SelectedPath;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_BrowseBackupPath
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var dialog = new FolderBrowserDialog ())
                        {
                            if (Directory.Exists (this.BackupPath))
                            {
                                dialog.SelectedPath = this.BackupPath;
                            }

                            dialog.Description = LocalizedStrings.GetString("dlg_SelectBackupPath");

                            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                this.BackupPath = dialog.SelectedPath;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_BrowseQuickSortApp
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var dialog = new OpenFileDialog ())
                        {
                            dialog.Title = LocalizedStrings.GetString("dlg_SelectQuickSortFileDlgTitle");
                            dialog.Filter = LocalizedStrings.GetString("dlg_SelectQuickSortFileDlgFilter");
                            dialog.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFiles);
                            dialog.CheckFileExists = true;

                            if (File.Exists (QuickSortApp))
                            {
                                dialog.InitialDirectory = QuickSortApp;
                            }

                            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                QuickSortApp = dialog.FileName;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand<ConfigViewModel, Object> Cmd_SaveConfig
        {
            get
            {
                return new RelayCommand<ConfigViewModel, Object> (
                    (param, viewModel, userParam) =>
                    {
                        AutoUnzip.Properties.Settings.Default.WatchPath = this.WatchPath;
                        AutoUnzip.Properties.Settings.Default.FilenameToSearch = this.FilenameToSearch;
                        AutoUnzip.Properties.Settings.Default.ExtractPath = this.ExtractPath;
                        AutoUnzip.Properties.Settings.Default.TempFolderPrefix = this.TempFolderPrefix;
                        AutoUnzip.Properties.Settings.Default.BackupPath = this.BackupPath;
                        AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod = this.BackupRetentionPeriod;
                        AutoUnzip.Properties.Settings.Default.QuickSortApp = this.QuickSortApp;
                        AutoUnzip.Properties.Settings.Default.ColorThemeId = (uint) (this.UseDarkModeColorTheme == true ? 1 : 0);
                        AutoUnzip.Properties.Settings.Default.Language = this.Language;

                        AutoUnzip.Properties.Settings.Default.Save ();

                        viewModel._View.DialogResult = true;
                        viewModel._View.Close ();
                    },
                    (param, viewModel, userParam) => true,
                    this,
                    null
                );
            }
        }

        public RelayCommand Cmd_OpenHomepage
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            System.Diagnostics.Process.Start ("https://github.com/X13-G44/iCloudHelper");
                        }
                        catch
                        {
                            ;
                        }
                    },
                    param => true
                );
            }
        }



        private readonly Dispatcher _Dispatcher;
        private readonly ConfigView _View;



        public ConfigViewModel (Dispatcher dispatcher, ConfigView view)
        {
            this._Dispatcher = dispatcher;
            this._View = view;

            this.WatchPath = AutoUnzip.Properties.Settings.Default.WatchPath;
            this.FilenameToSearch = AutoUnzip.Properties.Settings.Default.FilenameToSearch;
            this.ExtractPath = AutoUnzip.Properties.Settings.Default.ExtractPath;
            this.TempFolderPrefix = AutoUnzip.Properties.Settings.Default.TempFolderPrefix;
            this.BackupPath = AutoUnzip.Properties.Settings.Default.BackupPath;
            this.BackupRetentionPeriod = AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod;
            this.QuickSortApp = AutoUnzip.Properties.Settings.Default.QuickSortApp;
            this.UseDarkModeColorTheme = AutoUnzip.Properties.Settings.Default.ColorThemeId == 1 ? true : false;
            this.Language = AutoUnzip.Properties.Settings.Default.Language;

            Assembly assembly = Assembly.GetExecutingAssembly ();
            this.AppVersionStr = $"V{assembly.GetName ().Version.ToString ()}";
        }
    }
}
