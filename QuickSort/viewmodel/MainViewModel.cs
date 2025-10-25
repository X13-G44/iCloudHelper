/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				16.10.2025
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



using QuickSort.Help;
using QuickSort.Model;
using QuickSort.Resources;
using QuickSort.ValidationRules;
using QuickSort.View;
using QuickSort.View.UserControls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using System.Xml.Linq;



namespace QuickSort.ViewModel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        public ObservableCollection<FavoriteTargetFolderViewModel> FavoriteTargetFolderList { get; set; } = new ObservableCollection<FavoriteTargetFolderViewModel> ();

        public ObservableCollection<FileTitleViewModel> FileTileList { get; set; } = new ObservableCollection<FileTitleViewModel> ();

        public ObservableCollection<VirtualDirectoryViewModel> VirtualRootDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryViewModel> ();
        public ObservableCollection<VirtualDirectoryViewModel> VirtualFirstStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryViewModel> ();
        public ObservableCollection<VirtualDirectoryViewModel> VirtualSecundStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryViewModel> ();

        public ObservableCollection<FileMoveProcPopupNotificationViewModel> FileMoveProcPopupNotificationList { get; } = new ObservableCollection<FileMoveProcPopupNotificationViewModel> ();



        private DlgBoxViewModel _DlgBoxConfig = null;
        public DlgBoxViewModel DialogBoxConfiguration
        {
            get { return _DlgBoxConfig; }
            set { _DlgBoxConfig = value; OnPropertyChanged (nameof (DialogBoxConfiguration)); }
        }

        private string _RootPath = "";
        public string RootPath
        {
            get { return _RootPath; }
            set { _RootPath = value; OnPropertyChanged (nameof (RootPath)); }
        }

        private string _FileTileStatusText = "";
        public string FileTileStatusText
        {
            get { return _FileTileStatusText; }
            set { _FileTileStatusText = value; OnPropertyChanged (nameof (FileTileStatusText)); }
        }

        private bool _FileTitleLoadStatus_Show;
        public bool FileTitleLoadStatus_Show
        {
            get { return _FileTitleLoadStatus_Show; }
            set { _FileTitleLoadStatus_Show = value; OnPropertyChanged (nameof (FileTitleLoadStatus_Show)); }
        }

        private string _FileTitleLoadStatus_Text;
        public string FileTitleLoadStatus_Text
        {
            get { return _FileTitleLoadStatus_Text; }
            set { _FileTitleLoadStatus_Text = value; OnPropertyChanged (nameof (FileTitleLoadStatus_Text)); }
        }



        public RelayCommand Cmd_ShowConfigWindow
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        App app = (App) Application.Current;

                        string oldStartPath = ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath;
                        bool oldShowImageFileName = ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName;
                        int oldFileTitleSizeLevel = ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel;
                        int oldFileTitleListSortOrder = ConfigurationStorage.ConfigurationStorageModel.FileTitleSortOrder;


                        try
                        {
                            // Start AutoUnzip with parameter "showconfig" to show configuration dialog window.

                            Process prc = Process.Start (app.GetAutoUnzipFile (), "showconfig");
                            prc.WaitForExit ();

                            if (prc.ExitCode == 1)
                            {
                                // If result code is "1", then user have saved configuration.
                                // Load configuration and update out file title list.

                                ConfigurationStorage.ConfigurationStorageModel.LoadConfiguration ();

                                // Update the color theme.
                                SetColorTheme ();

                                // Update the UI language.
                                app.SetUiLanguage ();

                                // Refresh the file title list, when user changed the root path.
                                // We call the Cmd_ContextMenu_RefreshFileTitleList command for do this,
                                // so we don't duplicate the code for this task.
                                if (oldStartPath != ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath)
                                {
                                    this.RootPath = ConfigurationStorage.ConfigurationStorageModel.ExtractImagePath;

                                    if (Cmd_ContextMenu_RefreshFileTitleList.CanExecute (null))
                                    {
                                        Cmd_ContextMenu_RefreshFileTitleList.Execute (null);
                                    }
                                }

                                // Change item size or change the filename text in FileTitle list.
                                if (oldShowImageFileName != ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName ||
                                    oldFileTitleSizeLevel != ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel)
                                {
                                    FileTitleListResize ();
                                }

                                // Sort FileTitle list.
                                if (oldFileTitleListSortOrder != ConfigurationStorage.ConfigurationStorageModel.FileTitleSortOrder)
                                {
                                    FileTitleListSort ();
                                }
                            }
                        }
                        catch
                        {; }
                    },
                    _ =>
                    {
                        App app = (App) Application.Current;


                        return !String.IsNullOrEmpty (app.GetAutoUnzipFile ());
                    }
                );
            }
        }

        public RelayCommand Cmd_ApplicationClose
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        if (this.FileMoveProcPopupNotificationList.Count != 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_Message"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_Cancle"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_ExitApp"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => { App.Current.Shutdown (0); }),

                                null);
                        }
                        else
                        {
                            App.Current.Shutdown (0);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ApplicationMinimize
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        var mainWindow = System.Windows.Application.Current?.MainWindow;


                        if (mainWindow != null)
                        {
                            mainWindow.WindowState = WindowState.Minimized;
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ApplicationMaximizeRestore
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        var mainWindow = System.Windows.Application.Current?.MainWindow;


                        if (mainWindow != null)
                        {
                            if (mainWindow.WindowState == WindowState.Maximized)
                            {
                                mainWindow.WindowState = WindowState.Normal;
                            }
                            else
                            {
                                mainWindow.WindowState = WindowState.Maximized;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ClickOnFavoriteTargetFolderClick
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


                        if (item is FavoriteTargetFolderViewModel)
                        {
                            string targetPath = (item as FavoriteTargetFolderViewModel).Path;


                            if (querrySelectedFileList.Count > 0)
                            {
                                MoveFilesPrepare (targetPath);
                            }
                            else if (Directory.Exists (targetPath))
                            {
                                Process.Start ("explorer.exe", targetPath);
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ClickOnFileTitleItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        if (item is FileTitleViewModel)
                        {
                            bool shiftKeyIsPressed = false;


                            (item as FileTitleViewModel).IsSelected = !(item as FileTitleViewModel).IsSelected;

                            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift))
                            {
                                shiftKeyIsPressed = true;
                            }

                            if (shiftKeyIsPressed == false)
                            {
                                _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleViewModel);
                            }

                            else if (shiftKeyIsPressed == true)
                            {
                                if (_StartSelectionStartIndex > -1)
                                {
                                    int deltaIndex;
                                    int lowIndex;


                                    _EndSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleViewModel);

                                    deltaIndex = Math.Abs (_EndSelectionStartIndex - _StartSelectionStartIndex);
                                    lowIndex = Math.Min (_EndSelectionStartIndex, _StartSelectionStartIndex);

                                    for (int i = 0; i <= deltaIndex; i++)
                                    {
                                        this.FileTileList[i + lowIndex].IsSelected = true;
                                    }

                                    _StartSelectionStartIndex = -1;
                                    _EndSelectionStartIndex = -1;
                                }
                                else
                                {
                                    // If the start selection index is not set, just select the current item.
                                    _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleViewModel);
                                }
                            }

                            UpdateFileTitelStatusText ();
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ClickOnVirtualRootDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();
                        VirtualDirectoryViewModel virtualDirectoryModel = (item as VirtualDirectoryViewModel);


                        if (querrySelectedFileList.Count > 0)
                        {
                            MoveFilesPrepare (virtualDirectoryModel.Path, virtualDirectoryModel);
                        }
                        else
                        {
                            SelectVirtuaDirectoryListItem (virtualDirectoryModel);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ClickOnVirtualFirstStagDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();
                        VirtualDirectoryViewModel virtualDirectoryModel = (item as VirtualDirectoryViewModel);


                        if (querrySelectedFileList.Count > 0)
                        {
                            MoveFilesPrepare (virtualDirectoryModel.Path, virtualDirectoryModel);
                        }
                        else
                        {
                            SelectVirtuaDirectoryListItem (virtualDirectoryModel);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ClickOnVirtualSecondStagDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();
                        VirtualDirectoryViewModel virtualDirectoryModel = (item as VirtualDirectoryViewModel);


                        if (querrySelectedFileList.Count > 0)
                        {
                            MoveFilesPrepare (virtualDirectoryModel.Path);
                        }
                        else
                        {
                            SelectVirtuaDirectoryListItem (virtualDirectoryModel);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_MoveImages
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        string targetPath = String.Empty;


                        if (item is FavoriteTargetFolderViewModel)
                        {
                            targetPath = (item as FavoriteTargetFolderViewModel).Path;
                        }
                        else if (item is VirtualDirectoryViewModel)
                        {
                            targetPath = (item as VirtualDirectoryViewModel).Path;
                        }

                        if (!string.IsNullOrEmpty (targetPath) && Directory.Exists (targetPath))
                        {
                            MoveFiles (targetPath);
                        }
                    },
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


                        return querrySelectedFileList.Count () > 0;
                    }
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_AddFavoriteTargetFolderItem
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog ())
                            {
                                string initialPath = Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.LastUsedPath) ?
                                                     ConfigurationStorage.ConfigurationStorageModel.LastUsedPath :
                                                     Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

                                dialog.Description = LocalizedStrings.GetString ("dlgFavTargFolder_HelpText");
                                dialog.ShowNewFolderButton = true;
                                dialog.SelectedPath = initialPath;

                                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace (dialog.SelectedPath))
                                {
                                    string selectedPath = dialog.SelectedPath;
                                    string folderName = Path.GetFileName (selectedPath);


                                    // Create a new FavoriteTargetFolderList instance and add it to the collection.
                                    FavoriteTargetFolderList.Add (new FavoriteTargetFolderViewModel
                                    {
                                        DisplayName = folderName,
                                        Path = selectedPath,
                                        AddDate = DateTime.Now.ToFileTimeUtc (),
                                        IsPinned = true,
                                        Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,
                                        Cmd_AddFolderFromListCommand = Cmd_ContextMenu_AddFavoriteTargetFolderItem,
                                        Cmd_RemoveFolderFromListCommand = Cmd_ContextMenu_RemoveFavoriteTargetFolderItem,
                                    });

                                    ConfigurationStorage.ConfigurationStorageModel.LastUsedPath = selectedPath;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_AddFavoriteTargetFolderItem: {ex.Message}");
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_RemoveFavoriteTargetFolderItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        if (item is FavoriteTargetFolderViewModel)
                        {
                            this.FavoriteTargetFolderList.Remove (item as FavoriteTargetFolderViewModel);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_AddVirtualRootDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            using (var dialog = new System.Windows.Forms.FolderBrowserDialog ())
                            {
                                string initialPath = Directory.Exists (ConfigurationStorage.ConfigurationStorageModel.LastUsedPath) ?
                                                     ConfigurationStorage.ConfigurationStorageModel.LastUsedPath :
                                                     Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);


                                dialog.Description = LocalizedStrings.GetString ("dlgVirtualDirSec_HelpText");
                                dialog.ShowNewFolderButton = true;
                                dialog.SelectedPath = initialPath;

                                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace (dialog.SelectedPath))
                                {
                                    string selectedPath = dialog.SelectedPath;
                                    string folderName = Path.GetFileName (selectedPath);


                                    // Create a new VirtualDirectoryList instance and add it to the collection.
                                    VirtualRootDirectoryList.Add (new VirtualDirectoryViewModel
                                    {
                                        DisplayName = folderName,
                                        Path = selectedPath,

                                        Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,

                                        Cmd_ShowSubDirsCommand = Cmd_ContextMenu_ShowVirtualFirstStageDirectoryItem,

                                        Cmd_AddToListCommand = Cmd_ContextMenu_AddVirtualRootDirectoryItem,
                                        Cmd_RemoveItemFromListCommand = Cmd_ContextMenu_RemoveVirtualRootDirectoryItem,

                                        Cmd_CreateSubDirsCommand = Cmd_ContextMenu_VirtualFirstStageCreateDirectory,
                                        Cmd_DeleteSubDirsCommand = null, // Not used for root directory.


                                    });

                                    ConfigurationStorage.ConfigurationStorageModel.LastUsedPath = selectedPath;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_AddVirtualRootDirectoryItem: {ex.Message}");
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_RemoveVirtualRootDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        if (item is VirtualDirectoryViewModel)
                        {
                            VirtualFirstStageDirectoryList.Clear ();
                            VirtualSecundStageDirectoryList.Clear ();

                            VirtualRootDirectoryList.Remove (item as VirtualDirectoryViewModel);

                            // Clear all selected items in the VirtualRootDirectoryList.
                            var selectedVirtRootDirItems = VirtualRootDirectoryList.Where (x => x.IsSelected);
                            foreach (var virtRootDirItem in selectedVirtRootDirItems)
                            {
                                virtRootDirItem.IsSelected = false;
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_ShowVirtualFirstStageDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        SelectVirtuaDirectoryListItem ((VirtualDirectoryViewModel) item);
                    },
                    item =>
                    {
                        try
                        {
                            if (item is VirtualDirectoryViewModel)
                            {
                                if (!String.IsNullOrEmpty ((item as VirtualDirectoryViewModel).Path) && Directory.Exists ((item as VirtualDirectoryViewModel).Path))
                                {
                                    return Directory.GetDirectories ((item as VirtualDirectoryViewModel).Path).Length > 0;
                                }
                            }

                            return false;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_ShowVirtualFirstStageDirectoryItem: {ex.Message}");
                            return false;
                        }
                    }
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_VirtualFirstStageCreateDirectory
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        VirtualDirectoryViewModel selectedVirtRootDirObject = null;


                        if (item is VirtualDirectoryViewModel)
                        {
                            // User did right-click on a virtual directory button; which is selected or not.

                            selectedVirtRootDirObject = (VirtualDirectoryViewModel) item;
                        }
                        else
                        {
                            // User did right-click into the background of the "VirtualFirstStageDirectoryList" UI.

                            if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtRootDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                            }
                        }

                        if (selectedVirtRootDirObject != null)
                        {
                            Collection<ValidationRule> rules = new Collection<ValidationRule> ();


                            // Generate a directory exists validation rule object. It will be later used for check the user input in the dialog box.
                            rules.Add (new CheckDirectoryNameValidationRule () { RootDirectory = selectedVirtRootDirObject.Path });

                            // Setup and show dialog box.
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                DlgBoxType.Question,
                                LocalizedStrings.GetString ("lQuestion"),
                                LocalizedStrings.GetString ("dlgNewFolder_QuestionText"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Cancle"),
                                                  DlgBoxButtonSymbol.Cross,
                                                  null,
                                                  _dlgBoxCfg => {; }),

                                leftButton: new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Create"),
                                                    DlgBoxButtonSymbol.Check,
                                                    selectedVirtRootDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        try
                                                        {
                                                            VirtualDirectoryViewModel srcVirtDirInstance = (dlgBoxCfg.LeftButton.Parameter as VirtualDirectoryViewModel);
                                                            string rootPath = srcVirtDirInstance.Path;


                                                            Directory.CreateDirectory (Path.Combine (rootPath, dlgBoxCfg.TextBox.Text));

                                                            SelectVirtuaDirectoryListItem (srcVirtDirInstance);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgNewFolderError_Message", ex.Message));
                                                        }
                                                    }),

                                textBox: new DlgBoxTextBox (LocalizedStrings.GetString ("dlgNewFolder_NewFolder"), rules)
                            );
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_VirtualFirstStageDeleteDirectory
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        VirtualDirectoryViewModel selectedVirtDirObject = (VirtualDirectoryViewModel) item;


                        if (Directory.GetDirectories (selectedVirtDirObject.Path).Length != 0 || Directory.GetFiles (selectedVirtDirObject.Path).Length != 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgDelayNotEmptyDir_Message", selectedVirtDirObject.Path),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_Cancle"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_DeleteDir"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    selectedVirtDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        VirtualDirectoryViewModel selVirtDirObject = (VirtualDirectoryViewModel) dlgBoxCfg.LeftButton.Parameter;

                                                        try
                                                        {
                                                            Directory.Delete (selVirtDirObject.Path, true);

                                                            // Try to get parent path VirtualDirectory object.
                                                            if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                                            {
                                                                selVirtDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                                                            }

                                                            // Refresh the directory list.
                                                            SelectVirtuaDirectoryListItem (selVirtDirObject);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgDeleteDirError_Message", selVirtDirObject.Path, ex.Message));
                                                        }
                                                    }),

                                null);
                        }
                        else
                        {
                            try
                            {
                                Directory.Delete (selectedVirtDirObject.Path, true);

                                // Try to get parent path VirtualDirectory object.
                                if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                {
                                    selectedVirtDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                                }

                                // Refresh the directory list.
                                SelectVirtuaDirectoryListItem (selectedVirtDirObject);
                            }
                            catch (Exception ex)
                            {
                                this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                    DlgBoxType.Error,
                                    LocalizedStrings.GetString ("lError"),
                                    LocalizedStrings.GetFormattedString ("dlgDeleteDirError_Message", selectedVirtDirObject.Path, ex.Message));
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_ShowVirtualSecondStageDirectoryItem
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        SelectVirtuaDirectoryListItem ((VirtualDirectoryViewModel) item);
                    },
                    item =>
                    {
                        try
                        {
                            if (item is VirtualDirectoryViewModel)
                            {
                                if (!String.IsNullOrEmpty ((item as VirtualDirectoryViewModel).Path) && Directory.Exists ((item as VirtualDirectoryViewModel).Path))
                                {
                                    return Directory.GetDirectories ((item as VirtualDirectoryViewModel).Path).Length > 0;
                                }
                            }

                            return false;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_ShowVirtualSecondStageDirectoryItem: {ex.Message}");
                            return false;
                        }
                    }
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_VirtualSecondStageCreateDirectory
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        VirtualDirectoryViewModel selectedVirtRootDirObject = null;


                        if (item is VirtualDirectoryViewModel)
                        {
                            // User did right-click on a virtual directory button; which is selected or not.

                            selectedVirtRootDirObject = (VirtualDirectoryViewModel) item;
                        }
                        else
                        {
                            // User did right-click into the background of the "VirtualSecondStageDirectoryList" UI.

                            if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtRootDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                            }
                        }

                        if (selectedVirtRootDirObject != null)
                        {
                            Collection<ValidationRule> rules = new Collection<ValidationRule> ();


                            // Generate a directory exists validation rule object. It will be later used for check the user input in the dialog box.
                            rules.Add (new CheckDirectoryNameValidationRule () { RootDirectory = selectedVirtRootDirObject.Path });

                            // Setup and show dialog box.
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                DlgBoxType.Question,
                                LocalizedStrings.GetString ("lQuestion"),
                                LocalizedStrings.GetString ("dlgNewFolder_QuestionText"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Cancle"),
                                                  DlgBoxButtonSymbol.Cross,
                                                  null,
                                                  dlgBoxCfg => {; }),

                                leftButton: new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Create"),
                                                    DlgBoxButtonSymbol.Check,
                                                    selectedVirtRootDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        try
                                                        {
                                                            VirtualDirectoryViewModel srcVirtDirInstance = (dlgBoxCfg.LeftButton.Parameter as VirtualDirectoryViewModel);
                                                            string rootPath = srcVirtDirInstance.Path;


                                                            Directory.CreateDirectory (Path.Combine (rootPath, dlgBoxCfg.TextBox.Text));

                                                            SelectVirtuaDirectoryListItem (srcVirtDirInstance);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgNewFolderError_Message", ex.Message));
                                                        }
                                                    }),

                                textBox: new DlgBoxTextBox (LocalizedStrings.GetString ("dlgNewFolder_NewFolder"), rules)
                            );
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_VirtualSecondStageDeleteDirectory
        {
            get
            {
                return new RelayCommand (
                    item =>
                    {
                        VirtualDirectoryViewModel selectedVirtDirObject = (VirtualDirectoryViewModel) item;


                        if (Directory.GetFiles (selectedVirtDirObject.Path).Length != 0 || Directory.GetDirectories (selectedVirtDirObject.Path).Length != 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgDelayNotEmptyDir_Message", selectedVirtDirObject.Path),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_Cancle"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_DeleteDir"),
                                                    DlgBoxButtonSymbol.Empty,
                                                    selectedVirtDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        VirtualDirectoryViewModel selVirtDirObject = (VirtualDirectoryViewModel) dlgBoxCfg.LeftButton.Parameter;

                                                        try
                                                        {
                                                            Directory.Delete (selVirtDirObject.Path, true);

                                                            // Try to get parent path VirtualDirectory object.
                                                            if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                                            {
                                                                selVirtDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                                                            }

                                                            // Refresh the directory list.
                                                            SelectVirtuaDirectoryListItem (selVirtDirObject);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgDeleteDirError_Message", selVirtDirObject.Path, ex.Message));
                                                        }
                                                    }),

                                null);
                        }
                        else
                        {
                            try
                            {
                                Directory.Delete (selectedVirtDirObject.Path, true);

                                // Try to get parent path VirtualDirectory object.
                                if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                {
                                    selectedVirtDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                                }

                                // Refresh the directory list.
                                SelectVirtuaDirectoryListItem (selectedVirtDirObject);
                            }
                            catch (Exception ex)
                            {
                                this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                    DlgBoxType.Error,
                                    LocalizedStrings.GetString ("lError"),
                                    LocalizedStrings.GetFormattedString ("dlgDeleteDirError_Message", selectedVirtDirObject.Path, ex.Message));
                            }
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_OpenICloud
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            Process.Start ("https://www.icloud.com");
                        }
                        catch
                        {; }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_SelectAllFileTitles
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        foreach (var item in this.FileTileList)
                        {
                            item.IsSelected = true;
                        }

                        UpdateFileTitelStatusText ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_UnSelectAllFileTitles
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        foreach (var item in this.FileTileList)
                        {
                            item.IsSelected = false;
                        }

                        UpdateFileTitelStatusText ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_DeleteSelectedImageFiles
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


                        if (querrySelectedFileList.Count > 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                                 DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgDeleteImageFile_Message", querrySelectedFileList.Count),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDeleteImageFile_Cancle"),
                                                    DlgBoxButtonSymbol.Cross,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDeleteImageFile_Delete"),
                                                    DlgBoxButtonSymbol.Check,
                                                    null,
                                                    dlgBoxCfg =>
                                                    {
                                                        bool hasError = false;


                                                        try
                                                        {
                                                            var querrySelectedFileListX = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


                                                            foreach (var item in querrySelectedFileListX)
                                                            {
                                                                File.Delete (item.File);
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            hasError = true;
                                                        }

                                                        if (hasError)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetString ("dlgDeleteImageFile_ErrorMessage"));
                                                        }

                                                        FileTitleListResize ();
                                                    }),

                                null);
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_SetFileTitleSize
        {
            get
            {
                return new RelayCommand (
                    sizeLevel =>
                    {
                        int fileTitleSizeLevel;


                        if (int.TryParse (sizeLevel as string, out fileTitleSizeLevel))
                        {
                            ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel = fileTitleSizeLevel;

                            FileTitleListResize ();
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_SetFileTitleSortOrder
        {
            get
            {
                return new RelayCommand (
                    sizeLevel =>
                    {
                        int fileTitleSortOrder;


                        if (int.TryParse (sizeLevel as string, out fileTitleSortOrder))
                        {
                            ConfigurationStorage.ConfigurationStorageModel.FileTitleSortOrder = fileTitleSortOrder;

                            FileTitleListSort ();
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_ContextMenu_RefreshFileTitleList
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        // Clear the current file title list; so new items can be added and the old one are removed.
                        this.FileTileList.Clear ();

                        // Refresh the image buffer and update the file title list.
                        ImageFileBufferModel.RefreshBufferAsync (
                            this.RootPath,
                            true,
                            (bufferImageFile, curCnt, maxCnt, useMultiTask) =>
                            {
                                // Add a new image (file info object) to the file title list.

                                _Dispatcher.Invoke (() =>
                                {
                                    this.FileTitleLoadStatus_Show = true;
                                    this.FileTitleLoadStatus_Text = LocalizedStrings.GetFormattedString ("tbFileTitleSec_LodingImages", curCnt, maxCnt);

                                    FileTitleListAdd (bufferImageFile);

#warning When multiTaks is used, we chould sort the list every n-time.

                                });
                            },
                            (errorMessages) =>
                            {
                                // All images have been processed. Check if an error has been occurred.

                                _Dispatcher.Invoke (() =>
                                {
                                    FileTitleListSort ();

                                    if (errorMessages?.Count > 0)
                                    {
                                        string errorMessageString = string.Empty;


                                        errorMessages.ForEach (s => errorMessageString += s + "\n");

                                        this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                            DlgBoxType.Warning,
                                            LocalizedStrings.GetString ("lWarning"),
                                            LocalizedStrings.GetFormattedString ("dlgErrorLoadingImagesFromDir_Message", errorMessageString));
                                    }

                                    this.FileTitleLoadStatus_Show = false;
                                });
                            });
                    },
                    param => true
                );
            }
        }



        private readonly Dispatcher _Dispatcher;

        private int _StartSelectionStartIndex = -1;
        private int _EndSelectionStartIndex = -1;



        public MainViewModel (Dispatcher dispatcher, String path)
        {
            _Dispatcher = dispatcher;
            this.RootPath = path;
            this.FileTileStatusText = this.RootPath;

            SetColorTheme ();
            LoadFavoriteTargetFolderList ();
            LoadVirtualDirectoryList ();

            // Load the image buffer and update the file title list.
            ImageFileBufferModel.RefreshBufferAsync (
                this.RootPath,
                true,
                (bufferImageFile, curCnt, maxCnt, useMultiTask) =>
                {
                    // Add a new image (file info object) to the file title list.

                    _Dispatcher.Invoke (() =>
                    {
                        this.FileTitleLoadStatus_Show = true;
                        this.FileTitleLoadStatus_Text = LocalizedStrings.GetFormattedString ("tbFileTitleSec_LodingImages", curCnt, maxCnt);

                        FileTitleListAdd (bufferImageFile);
                    });
                },
                (errorMessages) =>
                {
                    // All images have been processed. Check if an error has been occurred.

                    _Dispatcher.Invoke (() =>
                    {
                        FileTitleListSort ();

                        if (errorMessages?.Count > 0)
                        {
                            string errorMessageString = string.Empty;


                            errorMessages.ForEach (s => errorMessageString += s + "\n");

                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgErrorLoadingImagesFromDir_Message", errorMessageString));
                        }

                        this.FileTitleLoadStatus_Show = false;
                    });
                });
        }



        public void Dispose ()
        {
            ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolder_ClearStorage ();

            foreach (var item in this.FavoriteTargetFolderList)
            {
                ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolder_AddItemToStorage (item.Path, item.AddDate, item.DisplayName, item.IsPinned);
            }


            ConfigurationStorage.ConfigurationStorageModel.VirtualDirectory_ClearStorage ();

            foreach (var item in this.VirtualRootDirectoryList)
            {
                ConfigurationStorage.ConfigurationStorageModel.VirtualDirectory_AddItemToStorage (item.Path, item.DisplayName);
            }
        }



        private void FileTitleListAdd (ImageFileBufferItem imageFileBufferItem)
        {
            int height = 128;
            int width = 128;


            switch (ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel)
            {
                case 0:
                    {
                        // Small symbol size.
                        height = width = 180;
                        break;
                    }

                default:
                case 1:
                    {
                        // Middle symbol size.
                        height = width = 275;
                        break;
                    }

                case 2:
                    {
                        // Large symbol size.
                        height = width = 560;
                        break;
                    }
            }

            if (imageFileBufferItem.FileExists)
            {
                FileTileList.Add (new FileTitleViewModel
                {
                    DisplayName = Path.GetFileName (imageFileBufferItem.File),
                    Thumbnail = imageFileBufferItem.Thumbnail,
                    Height = height,
                    Width = width,
                    HideFilenameText = !ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName,
                    SizeLevel = ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel,

                    File = imageFileBufferItem.File,

                    IsSysIconImage = imageFileBufferItem.IsSysIconImage,

                    TakenDate = imageFileBufferItem.TakenDate,
                    CreationTime = imageFileBufferItem.CreationTime,
                });
            }
        }



        private void FileTitleListSort ()
        {
            // Check for and removed, none existing image files.
            var querryDeletedImages = this.FileTileList.Where (x => !x.FileExists);
            foreach (var image in querryDeletedImages)
            {
                this.FileTileList.Remove (image);
            }

            // Start item sort.
            switch (ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel)
            {
                case 0:
                    {
                        // Sort by Display Name

                        var sortableList = new List<FileTitleViewModel> (this.FileTileList);


                        sortableList.Sort (delegate (FileTitleViewModel x, FileTitleViewModel y)
                        {
                            if (x.DisplayName == null && y.DisplayName == null) return 0;
                            else if (x.DisplayName == null) return -1;
                            else if (y.DisplayName == null) return 1;
                            else return x.DisplayName.CompareTo (y.DisplayName);
                        });

                        for (int i = 0; i < sortableList.Count; i++)
                        {
                            FileTileList.Move (FileTileList.IndexOf (sortableList[i]), i);
                        }

                        break;
                    }

                case 1:
                    {
                        // Sort by File Name

                        var sortableList = new List<FileTitleViewModel> (this.FileTileList);


                        sortableList.Sort (delegate (FileTitleViewModel x, FileTitleViewModel y)
                        {
                            if (x.Filename == null && y.Filename == null) return 0;
                            else if (x.Filename == null) return -1;
                            else if (y.Filename == null) return 1;
                            else return x.Filename.CompareTo (y.Filename);
                        });

                        for (int i = 0; i < sortableList.Count; i++)
                        {
                            FileTileList.Move (FileTileList.IndexOf (sortableList[i]), i);
                        }

                        break;
                    }

                case 2:
                    {
                        // Sort by File Creation Date

                        var sortableList = new List<FileTitleViewModel> (this.FileTileList);


                        sortableList.Sort ((x, y) => DateTime.Compare (x.CreationTime, y.CreationTime));

                        for (int i = 0; i < sortableList.Count; i++)
                        {
                            FileTileList.Move (FileTileList.IndexOf (sortableList[i]), i);
                        }

                        break;
                    }

                case 3:
                    {
                        // Sort by Image Take Date

                        var sortableList = new List<FileTitleViewModel> (this.FileTileList);


                        sortableList.Sort ((x, y) => DateTime.Compare (x.TakenDate, y.TakenDate));

                        for (int i = 0; i < sortableList.Count; i++)
                        {
                            FileTileList.Move (FileTileList.IndexOf (sortableList[i]), i);
                        }

                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }



        private void FileTitleListResize ()
        {
            int height = 128;
            int width = 128;


            switch (ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel)
            {
                case 0:
                    {
                        // Small symbol size.
                        height = width = 180;
                        break;
                    }

                default:
                case 1:
                    {
                        // Middle symbol size.
                        height = width = 275;
                        break;
                    }

                case 2:
                    {
                        // Large symbol size.
                        height = width = 560;
                        break;
                    }
            }

            // Check for and removed, none existing image files.
            var querryDeletedImages = this.FileTileList.Where (x => !x.FileExists);
            foreach (var image in querryDeletedImages)
            {
                this.FileTileList.Remove (image);
            }

            // Update the image size and file name text.
            for (int i = 0; i < this.FileTileList.Count; i++)
            {
                this.FileTileList[i].Height = height;
                this.FileTileList[i].Width = width;
                this.FileTileList[i].SizeLevel = ConfigurationStorage.ConfigurationStorageModel.FileTitleSizeLevel;

                this.FileTileList[i].HideFilenameText = !ConfigurationStorage.ConfigurationStorageModel.ShowImageFileName;
            }
        }



        private void LoadFavoriteTargetFolderList ()
        {
            try
            {
                // Clear the UI collections.
                this.FavoriteTargetFolderList.Clear ();

                for (int i = 0; i < ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolder_CountStorageItems (); i++)
                {
                    string path;
                    long date;
                    string displayName;
                    bool isPinned;


                    ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolder_GetStorageItem (i, out path, out date, out displayName, out isPinned);

                    this.FavoriteTargetFolderList.Add (new FavoriteTargetFolderViewModel
                    {
                        DisplayName = displayName,
                        Path = path,
                        AddDate = date,
                        IsPinned = isPinned,
                        Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,
                        Cmd_AddFolderFromListCommand = Cmd_ContextMenu_AddFavoriteTargetFolderItem,
                        Cmd_RemoveFolderFromListCommand = Cmd_ContextMenu_RemoveFavoriteTargetFolderItem,
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"Error loading target folders from settings: {ex.Message}");
            }
        }



        private void LoadVirtualDirectoryList ()
        {
            try
            {
                // Clear the UI collections.
                this.VirtualRootDirectoryList.Clear ();

                for (int i = 0; i < ConfigurationStorage.ConfigurationStorageModel.VirtualDirectory_CountStorageItems (); i++)
                {
                    string path;
                    string displayName;


                    ConfigurationStorage.ConfigurationStorageModel.VirtualDirectory_GetStorageItem (i, out path, out displayName);

                    this.VirtualRootDirectoryList.Add (new VirtualDirectoryViewModel
                    {
                        Stage = VirtualDirectoryViewModel.StageType.RootStage,
                        DisplayName = displayName,
                        Path = path,

                        Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,

                        Cmd_ShowSubDirsCommand = Cmd_ContextMenu_ShowVirtualFirstStageDirectoryItem,

                        Cmd_AddToListCommand = Cmd_ContextMenu_AddVirtualRootDirectoryItem,
                        Cmd_RemoveItemFromListCommand = Cmd_ContextMenu_RemoveVirtualRootDirectoryItem,

                        Cmd_CreateSubDirsCommand = Cmd_ContextMenu_VirtualFirstStageCreateDirectory,
                        Cmd_DeleteSubDirsCommand = null, // Not used for root directory.
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"Error loading virtual directory from settings: {ex.Message}");
            }
        }



        private void MoveFilesPrepare (string targetPath, VirtualDirectoryViewModel optionalOpenTargetVirtualDirectory = null)
        {
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


            if (querrySelectedFileList.Count () > 0)
            {
                if (ConfigurationStorage.ConfigurationStorageModel.ShowMoveDlg)
                {
                    string questionText = optionalOpenTargetVirtualDirectory == null ?
                        LocalizedStrings.GetFormattedString ("dlgFileMove_QuestionText_A", querrySelectedFileList.Count (), Path.GetFileName (targetPath)) :
                        LocalizedStrings.GetFormattedString ("dlgFileMove_QuestionText_B", querrySelectedFileList.Count (), Path.GetFileName (targetPath));

                    this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                        DlgBoxType.Question,
                        LocalizedStrings.GetString ("lQuestion"),
                        questionText,

                        new DlgBoxButton (LocalizedStrings.GetString ("dlgFileMove_Cancle"),
                            DlgBoxButtonSymbol.Check,
                            null,
                            dlgBoxCfg => {; }),

                        optionalOpenTargetVirtualDirectory != null ? new DlgBoxButton (LocalizedStrings.GetString ("dlgFileMove_OpenDirOnly"),
                            DlgBoxButtonSymbol.OpenFolder,
                            optionalOpenTargetVirtualDirectory,
                            dlgBoxCfg => { SelectVirtuaDirectoryListItem ((VirtualDirectoryViewModel) dlgBoxCfg.CenterButton.Parameter); }) : null,

                        new DlgBoxButton (LocalizedStrings.GetString ("dlgFileMove_StartMove"),
                            DlgBoxButtonSymbol.Move,
                            targetPath,
                            dlgBoxCfg => { MoveFiles (dlgBoxCfg.LeftButton.Parameter as String); })
                    );
                }
                else
                {
                    MoveFiles (targetPath);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty (targetPath) && Directory.Exists (targetPath))
                {
                    Process.Start ("explorer.exe", targetPath);
                }
            }
        }



        private void MoveFiles (string targetPath)
        {
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleViewModel> ();


            if (querrySelectedFileList.Count () > 0)
            {
                // Append the target path to the favorite target folder list - or update the add date if already existing.
                var existingFavTargetFolderItem = FavoriteTargetFolderList.Where (x => x.Path == targetPath).FirstOrDefault ();
                if (existingFavTargetFolderItem != null)
                {
                    // Update favorite target folder item.

                    existingFavTargetFolderItem.AddDate = DateTime.Now.ToFileTimeUtc ();
                }
                else
                {
                    // Append new favorite target folder item.

                    if (ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionAutoInsert)
                    {
                        // Check favorite target folder list limit.
                        if (FavoriteTargetFolderList.Count >= ConfigurationStorage.ConfigurationStorageModel.FavoriteTargetFolderCollectionLimit)
                        {
                            // Remove the oldest entry that is not pinned.
                            var itemToRemove = FavoriteTargetFolderList.Where (x => x.IsPinned == false).OrderBy (x => x.AddDate).FirstOrDefault ();
                            if (itemToRemove != null)
                            {
                                FavoriteTargetFolderList.Remove (itemToRemove);
                            }
                        }

                        FavoriteTargetFolderList.Add (new FavoriteTargetFolderViewModel
                        {
                            DisplayName = Path.GetFileName (targetPath),
                            Path = targetPath,
                            AddDate = DateTime.Now.ToFileTimeUtc (),
                            IsPinned = false,
                            Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,
                            Cmd_AddFolderFromListCommand = Cmd_ContextMenu_AddFavoriteTargetFolderItem,
                            Cmd_RemoveFolderFromListCommand = Cmd_ContextMenu_RemoveFavoriteTargetFolderItem,
                        });
                    }
                }

                // Remove selected items from FileTileList.
                foreach (var item in querrySelectedFileList)
                {
                    FileTileList.Remove (item);
                }

                // Move the image files.
                Task.Run (() =>
                {
                    List<FileTitleViewModel> querrySelectedFileList_Local = querrySelectedFileList;
                    var cts = new CancellationTokenSource ();
                    var popup = new FileMoveProcPopupNotificationViewModel ()
                    {
                        TargetPath = Path.GetFileName (targetPath),
                        FileCount = querrySelectedFileList_Local.Count (),
                        FileProcessed = 0,
                        CurrentFileName = "",
                        Cmd_Abort = new RelayCommand (_ => cts.Cancel ())
                    };


                    _Dispatcher.Invoke (() => FileMoveProcPopupNotificationList.Add (popup));


                    foreach (var fileItem in querrySelectedFileList_Local)
                    {
                        string targetFile = Path.Combine (targetPath, fileItem.DisplayName);


                        _Dispatcher.Invoke (() => popup.FileProcessed++);
                        _Dispatcher.Invoke (() => popup.CurrentFileName = fileItem.DisplayName);


                        if (File.Exists (fileItem.File) == true)
                        {
                            try
                            {
                                if (File.Exists (targetFile) == false)
                                {
                                    File.Move (fileItem.File, targetFile);
                                    File.Delete (fileItem.File);
                                }
                                else
                                {
                                    // A file with the same name already exists in the target directory.

                                    // Check, if the both files have the same content.
                                    if (FileContentEqual (targetFile, fileItem.File) == true)
                                    {
                                        // File content is equal. Don't move the file; only delete them.

                                        File.Delete (fileItem.File);
                                    }
                                    else
                                    {
                                        // File content is not equal. Copy the file and add a name postfix.

                                        string randomSuffix = Path.GetRandomFileName ().Replace (".", string.Empty).Substring (0, 3); // Generate a random 3-Chars long suffix string.
                                        string targetFilePostfix = Path.Combine (targetPath, $"{Path.GetFileNameWithoutExtension (fileItem.Filename)}_{randomSuffix}{Path.GetExtension (fileItem.Filename)}");


                                        File.Move (fileItem.File, targetFilePostfix);
                                        File.Delete (fileItem.File);
                                    }
                                }
                            }
                            catch
                            {
                                _Dispatcher.Invoke (() => popup.HasFileProcessError = true);
                            }
                        }

                        if (cts.Token.IsCancellationRequested)
                        {
                            break;
                        }
                    }


                    _Dispatcher.Invoke (async () =>
                    {
                        // On file processing error, we show the faulted popup longer (by the bound property, the UI shows the popup in red color).
                        if (popup.HasFileProcessError)
                        {
                            await Task.Delay (5000);
                        }

                        // Start Fadeout effect.
                        popup.IsFadingOut = true;

                        // Delay for Fadeout effect before removing popup object from list.
                        await Task.Delay (600);

                        // Remove popup object from list.
                        FileMoveProcPopupNotificationList.Remove (popup);

                        // Refresh the file title list.
                        FileTitleListResize ();
                    });
                });
            }
            else
            {
                if (!string.IsNullOrEmpty (targetPath) && Directory.Exists (targetPath))
                {
                    Process.Start ("explorer.exe", targetPath);
                }
            }
        }



        private void SetColorTheme ()
        {
            string themeFile;
            switch (ConfigurationStorage.ConfigurationStorageModel.ColorThemeId)
            {
                default:
                case 0:
                    {
                        themeFile = "/view/theme/ColorThemeLightMode.xaml";
                        break;
                    }

                case 1:
                    {
                        themeFile = "/view/theme/ColorThemeDarkMode.xaml";
                        break;
                    }
            }
            var dict = new ResourceDictionary { Source = new Uri (themeFile, UriKind.Relative) };


            // Remove the old (initial) theme.
            var oldDict = System.Windows.Application.Current.Resources.MergedDictionaries.FirstOrDefault (d => d.Source != null && (d.Source.OriginalString.Contains ("/view/theme/ColorThemeDarkMode.xaml") ||
                                                                                                                                    d.Source.OriginalString.Contains ("/view/theme/ColorThemeLightMode.xaml")));
            if (oldDict != null)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Remove (oldDict);
            }

            // Add new theme.
            System.Windows.Application.Current.Resources.MergedDictionaries.Add (dict);
        }



        private void UpdateFileTitelStatusText ()
        {
            var count = this.FileTileList.Count (x => x.IsSelected);
            if (count > 1)
            {
                this.FileTileStatusText = LocalizedStrings.GetFormattedString ("tbFileTitleSec_SubTitle_2", count);
            }
            else if (count == 1)
            {
                this.FileTileStatusText = LocalizedStrings.GetString ("tbFileTitleSec_SubTitle_1");
            }
            else
            {
                this.FileTileStatusText = this.RootPath;
            }
        }



        private void SelectVirtuaDirectoryListItem (VirtualDirectoryViewModel sourceVirtualDirectoryModel)
        {
            string path = sourceVirtualDirectoryModel.Path;


            switch (sourceVirtualDirectoryModel.Stage)
            {
                case VirtualDirectoryViewModel.StageType.RootStage:
                    {
                        // Unselect all VirtualRootDirectoryList items.
                        var selectedVirtRootDirItems = VirtualRootDirectoryList.Where (x => x.IsSelected);
                        foreach (var virtRootDirItem in selectedVirtRootDirItems)
                        {
                            virtRootDirItem.IsSelected = false;
                        }

                        // Select the clicked item.
                        sourceVirtualDirectoryModel.IsSelected = true;

                        // Clear also first and second stage lists.
                        VirtualFirstStageDirectoryList.Clear ();
                        VirtualSecundStageDirectoryList.Clear ();

                        // Fill the first stage directory lists.
                        if (!string.IsNullOrEmpty (path) && Directory.Exists (path))
                        {
                            String[] subDirList = Directory.GetDirectories (path);


                            foreach (var subDir in subDirList)
                            {
                                // Create a new VirtualDirectoryList instance and add it to the collection.
                                VirtualFirstStageDirectoryList.Add (new VirtualDirectoryViewModel
                                {
                                    Stage = VirtualDirectoryViewModel.StageType.FirstStage,
                                    DisplayName = System.IO.Path.GetFileName (subDir),
                                    Path = subDir,

                                    Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,

                                    Cmd_ShowSubDirsCommand = Cmd_ContextMenu_ShowVirtualSecondStageDirectoryItem,

                                    Cmd_AddToListCommand = null,
                                    Cmd_RemoveItemFromListCommand = null,

                                    Cmd_CreateSubDirsCommand = Cmd_ContextMenu_VirtualSecondStageCreateDirectory,
                                    Cmd_DeleteSubDirsCommand = Cmd_ContextMenu_VirtualFirstStageDeleteDirectory,
                                });
                            }
                        }

                        break;
                    }

                case VirtualDirectoryViewModel.StageType.FirstStage:
                    {
                        // Unselect all VirtualFirstDirectoryList items.
                        var selectedVirtFirstDirItems = VirtualFirstStageDirectoryList.Where (x => x.IsSelected);
                        foreach (var virtFirstDirItem in selectedVirtFirstDirItems)
                        {
                            virtFirstDirItem.IsSelected = false;
                        }

                        // Select the clicked item.
                        sourceVirtualDirectoryModel.IsSelected = true;

                        // Clear also second stage lists.
                        VirtualSecundStageDirectoryList.Clear ();

                        // Fill the second stage directory lists.
                        if (!string.IsNullOrEmpty (path) && Directory.Exists (path))
                        {
                            String[] subDirList = Directory.GetDirectories (path);


                            foreach (var subDir in subDirList)
                            {
                                // Create a new VirtualDirectoryList instance and add it to the collection.
                                VirtualSecundStageDirectoryList.Add (new VirtualDirectoryViewModel
                                {
                                    Stage = VirtualDirectoryViewModel.StageType.SecondStage,
                                    DisplayName = System.IO.Path.GetFileName (subDir),
                                    Path = subDir,

                                    Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,

                                    Cmd_ShowSubDirsCommand = null,

                                    Cmd_AddToListCommand = null,
                                    Cmd_RemoveItemFromListCommand = null,

                                    Cmd_CreateSubDirsCommand = null, // No sub directories in second stage.
                                    Cmd_DeleteSubDirsCommand = Cmd_ContextMenu_VirtualSecondStageDeleteDirectory,
                                });
                            }
                        }

                        break;
                    }

                case VirtualDirectoryViewModel.StageType.SecondStage:
                    {
                        // Nothing to do here...

                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }



        public bool FileContentEqual (string fileA, string fileB)
        {
            if (String.IsNullOrEmpty (fileA) || String.IsNullOrEmpty (fileB))
            {
                return false;
            }

            try
            {
                using (var sha256 = SHA256.Create ())
                using (var stream1 = System.IO.File.OpenRead (fileA))
                using (var stream2 = System.IO.File.OpenRead (fileB))
                {
                    var hash1 = sha256.ComputeHash (stream1);
                    var hash2 = sha256.ComputeHash (stream2);


                    return StructuralComparisons.StructuralEqualityComparer.Equals (hash1, hash2);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
