/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;



namespace QuickSort.ViewModel
{
    public class ConfigViewModel : ViewModelBase
    {
        private string _StartPath;
        public string StartPath
        {
            get { return _StartPath; }
            set { _StartPath = value; OnPropertyChanged (nameof (StartPath)); }
        }

        private bool _ShowMoveDlg;
        public bool ShowMoveDlg
        {
            get { return _ShowMoveDlg; }
            set { _ShowMoveDlg = value; OnPropertyChanged (nameof (ShowMoveDlg)); }
        }

        private bool _ShowImgFileName;
        public bool ShowImgFileName
        {
            get { return _ShowImgFileName; }
            set { _ShowImgFileName = value; }
        }

        private bool _UseDarkModeColorTheme = false;
        public bool UseDarkModeColorTheme
        {
            get { return _UseDarkModeColorTheme; }
            set { _UseDarkModeColorTheme = value; OnPropertyChanged (nameof (UseDarkModeColorTheme)); }
        }

        private int _MaxFavoriteTargetFolderCollectionItems;
        public int MaxFavoriteTargetFolderCollectionItems
        {
            get { return _MaxFavoriteTargetFolderCollectionItems; }
            set { _MaxFavoriteTargetFolderCollectionItems = value; OnPropertyChanged (nameof (MaxFavoriteTargetFolderCollectionItems)); }
        }

        private bool _AutoInsertFavoriteTargetFolderCollectionItems;
        public bool AutoInsertFavoriteTargetFolderCollectionItems
        {
            get { return _AutoInsertFavoriteTargetFolderCollectionItems; }
            set { _AutoInsertFavoriteTargetFolderCollectionItems = value; OnPropertyChanged (nameof (AutoInsertFavoriteTargetFolderCollectionItems)); }
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



        public RelayCommand Cmd_BrowseStartFolder
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var dialog = new FolderBrowserDialog ())
                        {
                            if (Directory.Exists (this.StartPath))
                            {
                                dialog.SelectedPath = this.StartPath;
                            }

                            dialog.Description = LocalizedStrings.GetString ("dlgConfig_StartPath");

                            if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                this.StartPath = dialog.SelectedPath;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand<ConfigViewModel, object> Cmd_SaveConfig
        {
            get
            {
                return new RelayCommand<ConfigViewModel, object> (
                    (param, viewModel, userParam) =>
                    {
                        ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath = this.StartPath;
                        ConfigurationStorage.ConfigurationStorageModel.ShowMoveDlg = this.ShowMoveDlg;
                        ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName = this.ShowImgFileName;
                        ConfigurationStorage.ConfigurationStorageModel.ColorThemeId = (int) (this.UseDarkModeColorTheme == true ? 1 : 0);
                        ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionLimit = this.MaxFavoriteTargetFolderCollectionItems;
                        ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionAutoInsert = this.AutoInsertFavoriteTargetFolderCollectionItems;
                        ConfigurationStorage.ConfigurationStorageModel.LanguageId = this.Language;

                        ConfigurationStorage.ConfigurationStorageModel.SaveConfiguration();

                        viewModel._View.DialogResult = true;
                        viewModel._View.Close ();
                    },
                    (param, hostInst, userParam) => true,
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

            this.StartPath = ConfigurationStorage.ConfigurationStorageModel.DefaultStartPath;
            this.ShowMoveDlg = ConfigurationStorage.ConfigurationStorageModel.ShowMoveDlg;
            this.ShowImgFileName = ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName;
            this.UseDarkModeColorTheme = ConfigurationStorage.ConfigurationStorageModel.ColorThemeId == 1 ? true : false;
            this.MaxFavoriteTargetFolderCollectionItems = ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionLimit;
            this.AutoInsertFavoriteTargetFolderCollectionItems = ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionAutoInsert;
            this.Language = ConfigurationStorage.ConfigurationStorageModel.LanguageId;

            Assembly assembly = Assembly.GetExecutingAssembly ();
            this.AppVersionStr = $"V{assembly.GetName ().Version.ToString ()}";
        }
    }
}
