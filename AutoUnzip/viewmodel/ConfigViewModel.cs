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
using System.Windows.Controls;
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

        private bool _BackupEnabled;
        public bool BackupEnabled
        {
            get { return _BackupEnabled; }
            set { _BackupEnabled = value; OnPropertyChanged (nameof (BackupEnabled)); }
        }

        private string _BackupPath;
        public string BackupPath
        {
            get { return _BackupPath; }
            set { _BackupPath = value; OnPropertyChanged (nameof (BackupPath)); }
        }

        private bool _BackupRetentionCheckEnabled;
        public bool BackupRetentionCheckEnabled
        {
            get { return _BackupRetentionCheckEnabled; }
            set { _BackupRetentionCheckEnabled = value; OnPropertyChanged (nameof (BackupRetentionCheckEnabled)); }
        }

        private int _BackupRetentionPeriod;
        public int BackupRetentionPeriod
        {
            get { return _BackupRetentionPeriod; }
            set { _BackupRetentionPeriod = value; OnPropertyChanged (nameof (BackupRetentionPeriod)); }
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

        private bool _ShowMoveDlg;
        public bool ShowMoveDlg
        {
            get { return _ShowMoveDlg; }
            set { _ShowMoveDlg = value; OnPropertyChanged (nameof (ShowMoveDlg)); }
        }

        private bool _ShowImageFileName;
        public bool ShowImageFileName
        {
            get { return _ShowImageFileName; }
            set { _ShowImageFileName = value; OnPropertyChanged (nameof (ShowImageFileName)); }
        }

        private int _ColorGroupMode;
        public int ColorGroupMode
        {
            get { return _ColorGroupMode; }
            set { _ColorGroupMode = value; OnPropertyChanged (nameof (ColorGroupMode)); }
        }

        private bool _FavoriteTargetFolderCollectionAutoInsert;
        public bool FavoriteTargetFolderCollectionAutoInsert
        {
            get { return _FavoriteTargetFolderCollectionAutoInsert; }
            set { _FavoriteTargetFolderCollectionAutoInsert = value; OnPropertyChanged (nameof (FavoriteTargetFolderCollectionAutoInsert)); }
        }

        private int _FavoriteTargetFolderCollectionLimit;
        public int FavoriteTargetFolderCollectionLimit
        {
            get { return _FavoriteTargetFolderCollectionLimit; }
            set { _FavoriteTargetFolderCollectionLimit = value; OnPropertyChanged (nameof (FavoriteTargetFolderCollectionLimit)); }
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

        public RelayCommand<ConfigViewModel, Object> Cmd_SaveConfig
        {
            get
            {
                return new RelayCommand<ConfigViewModel, Object> (
                    (param, viewModel, userParam) =>
                    {
                        ConfigurationStorage.ConfigurationStorageModel.MonitoringPath = this.WatchPath;
                        ConfigurationStorage.ConfigurationStorageModel.MonitoringFilename = this.FilenameToSearch;
                        ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath = this.ExtractPath;
                        ConfigurationStorage.ConfigurationStorageModel.BackupEnabled = this.BackupEnabled;                        
                        ConfigurationStorage.ConfigurationStorageModel.BackupPath = this.BackupPath;
                        ConfigurationStorage.ConfigurationStorageModel.BackupRetentionCheckEnabled = this.BackupRetentionCheckEnabled;
                        ConfigurationStorage.ConfigurationStorageModel.BackupRetentionPeriod = this.BackupRetentionPeriod;
                        ConfigurationStorage.ConfigurationStorageModel.ColorThemeId = (int) (this.UseDarkModeColorTheme == true ? 1 : 0);
                        ConfigurationStorage.ConfigurationStorageModel.LanguageId = this.Language;
                        ConfigurationStorage.ConfigurationStorageModel.ShowMoveDlg = this.ShowMoveDlg;
                        ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName = this.ShowImageFileName;
                        ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionAutoInsert = this.FavoriteTargetFolderCollectionAutoInsert;
                        ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionLimit = this.FavoriteTargetFolderCollectionLimit;
                        ConfigurationStorage.ConfigurationStorageModel.FileTitleGroupMode = this.ColorGroupMode;

                        ConfigurationStorage.ConfigurationStorageModel.SaveConfiguration ();

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

            this.WatchPath = ConfigurationStorage.ConfigurationStorageModel.MonitoringPath;
            this.FilenameToSearch = ConfigurationStorage.ConfigurationStorageModel.MonitoringFilename;
            this.ExtractPath = ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath;
            this.BackupEnabled = ConfigurationStorage.ConfigurationStorageModel.BackupEnabled;
            this.BackupPath = ConfigurationStorage.ConfigurationStorageModel.BackupPath;
            this.BackupRetentionCheckEnabled = ConfigurationStorage.ConfigurationStorageModel.BackupRetentionCheckEnabled;
            this.BackupRetentionPeriod = ConfigurationStorage.ConfigurationStorageModel.BackupRetentionPeriod;
            this.UseDarkModeColorTheme = ConfigurationStorage.ConfigurationStorageModel.ColorThemeId == 1 ? true : false;
            this.Language = ConfigurationStorage.ConfigurationStorageModel.LanguageId;
            this.ShowMoveDlg= ConfigurationStorage.ConfigurationStorageModel.ShowMoveDlg;
            this.ShowImageFileName= ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName;
            this.FavoriteTargetFolderCollectionAutoInsert= ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionAutoInsert;
            this.FavoriteTargetFolderCollectionLimit= ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionLimit;
            this.ColorGroupMode = ConfigurationStorage.ConfigurationStorageModel.FileTitleGroupMode;

            Assembly assembly = Assembly.GetExecutingAssembly ();
            this.AppVersionStr = $"V{assembly.GetName ().Version.ToString ()}";
        }
    }
}
