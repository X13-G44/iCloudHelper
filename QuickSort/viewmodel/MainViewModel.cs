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



using QuickSort.help;
using QuickSort.model;
using QuickSort.Resources;
using QuickSort.validationrules;
using QuickSort.view;
using QuickSort.viewmodel;
using QuickSort.viewmodel.DlgBox;
using System;
using System.Threading;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;



namespace QuickSort.viewmodel
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private class ImageFileInfo
        {
            public string File { get; set; }
            public BitmapImage Thumbnail { get; set; }
            public bool IsSysIconImage { get; set; }

            public bool FileExists
            {
                get
                {
                    try
                    {
                        return System.IO.File.Exists (this.File);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }



            public bool CompareImageWith (ImageFileInfo imageFile)
            {
                try
                {
                    using (var sha256 = SHA256.Create ())
                    using (var stream1 = System.IO.File.OpenRead (this.File))
                    using (var stream2 = System.IO.File.OpenRead (imageFile.File))
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



        public ObservableCollection<FavoriteTargetFolderModel> FavoriteTargetFolderList { get; set; } = new ObservableCollection<FavoriteTargetFolderModel> ();

        public ObservableCollection<FileTitleModel> FileTileList { get; set; } = new ObservableCollection<FileTitleModel> ();

        public ObservableCollection<VirtualDirectoryModel> VirtualRootDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();
        public ObservableCollection<VirtualDirectoryModel> VirtualFirstStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();
        public ObservableCollection<VirtualDirectoryModel> VirtualSecundStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();

        public ObservableCollection<FileMoveProcPopupNotificationModel> FileMoveProcPopupNotificationList { get; } = new ObservableCollection<FileMoveProcPopupNotificationModel> ();



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

                        var dialog = new ConfigView ();

                        string oldStartPath = QuickSort.Properties.Settings.Default.StartPath;
                        bool oldShowImageFileName = QuickSort.Properties.Settings.Default.ShowImageFileName;


                        dialog.ShowDialog ();

                        if (dialog.DialogResult.Value)
                        {
                            // Update the color theme.
                            SetColorTheme ();

                            // Update the UI language.
                            app.SetUiLanguage ();

                            // Refresh the file title list, when user changed the default start path.
                            // We call the Cmd_ContextMenu_RefreshFileTitleList command for do this,
                            // so we don't duplicate the code for this task.
                            if (oldStartPath != QuickSort.Properties.Settings.Default.StartPath)
                            {
                                this.RootPath = QuickSort.Properties.Settings.Default.StartPath;

                                if (Cmd_ContextMenu_RefreshFileTitleList.CanExecute (null))
                                {
                                    Cmd_ContextMenu_RefreshFileTitleList.Execute (null);
                                }
                            }
                            else if (oldShowImageFileName != QuickSort.Properties.Settings.Default.ShowImageFileName &&
                                     oldStartPath == QuickSort.Properties.Settings.Default.StartPath)
                            {
                                // Update the file title list only, when the image file buffer wasn't changed.
                                // If it was changed, the file title list was indirect updated by refreshing the image file buffer (above).

                                UpdateFileTitleList ();
                            }

                        }
                    },
                    param => true
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
                                view.UserControls.DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_Message"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_Cancle"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgAppExitFileCopyStillActive_ExitApp"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();


                        if (item is FavoriteTargetFolderModel)
                        {
                            string targetPath = (item as FavoriteTargetFolderModel).Path;


                            if (querrySelectedFileList.Count > 0)
                            {
                                PrepareMoveFiles (targetPath);
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
                        if (item is FileTitleModel)
                        {
                            bool shiftKeyIsPressed = false;


                            (item as FileTitleModel).IsSelected = !(item as FileTitleModel).IsSelected;

                            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift))
                            {
                                shiftKeyIsPressed = true;
                            }

                            if (shiftKeyIsPressed == false)
                            {
                                _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleModel);
                            }

                            else if (shiftKeyIsPressed == true)
                            {
                                if (_StartSelectionStartIndex > -1)
                                {
                                    int deltaIndex;
                                    int lowIndex;


                                    _EndSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleModel);

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
                                    _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTitleModel);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();
                        VirtualDirectoryModel virtualDirectoryModel = (item as VirtualDirectoryModel);


                        if (querrySelectedFileList.Count > 0)
                        {
                            PrepareMoveFiles (virtualDirectoryModel.Path);
                        }
                        else
                        {
                            // Unselect all VirtualRootDirectoryList items.
                            var selectedVirtRootDirItems = VirtualRootDirectoryList.Where (x => x.IsSelected);
                            foreach (var virtRootDirItem in selectedVirtRootDirItems)
                            {
                                virtRootDirItem.IsSelected = false;
                            }

                            // Select the clicked item.
                            virtualDirectoryModel.IsSelected = true;

                            // Update the UI.
                            LoadVirtualFirstStageDirectoryList (virtualDirectoryModel.Path);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();
                        VirtualDirectoryModel virtualDirectoryModel = (item as VirtualDirectoryModel);


                        if (querrySelectedFileList.Count > 0)
                        {
                            PrepareMoveFiles (virtualDirectoryModel.Path);
                        }
                        else
                        {
                            // Unselect all VirtualFirstDirectoryList items.
                            var selectedVirtFirstDirItems = VirtualFirstStageDirectoryList.Where (x => x.IsSelected);
                            foreach (var virtFirstDirItem in selectedVirtFirstDirItems)
                            {
                                virtFirstDirItem.IsSelected = false;
                            }

                            // Select the clicked item.
                            virtualDirectoryModel.IsSelected = true;

                            // Update the UI.
                            LoadVirtualSecondStageDirectoryList (virtualDirectoryModel.Path);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();
                        string targetPath = (item as VirtualDirectoryModel).Path;


                        if (querrySelectedFileList.Count > 0)
                        {
                            PrepareMoveFiles (targetPath);
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


                        if (item is FavoriteTargetFolderModel)
                        {
                            targetPath = (item as FavoriteTargetFolderModel).Path;
                        }
                        else if (item is VirtualDirectoryModel)
                        {
                            targetPath = (item as VirtualDirectoryModel).Path;
                        }

                        if (!string.IsNullOrEmpty (targetPath) && Directory.Exists (targetPath))
                        {
                            PrepareMoveFiles (targetPath);
                        }
                    },
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();


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
                                string initialPath = Directory.Exists (Properties.Settings.Default.LastUsedPath) ?
                                                     Properties.Settings.Default.LastUsedPath :
                                                     Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);

                                dialog.Description = LocalizedStrings.GetString ("dlgFavTargFolder_HelpText");
                                dialog.ShowNewFolderButton = true;
                                dialog.SelectedPath = initialPath;

                                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace (dialog.SelectedPath))
                                {
                                    string selectedPath = dialog.SelectedPath;
                                    string folderName = Path.GetFileName (selectedPath);


                                    // Create a new FavoriteTargetFolderList instance and add it to the collection.
                                    FavoriteTargetFolderList.Add (new FavoriteTargetFolderModel
                                    {
                                        DisplayName = folderName,
                                        Path = selectedPath,
                                        AddDate = DateTime.Now.ToFileTimeUtc (),
                                        IsPinned = true,
                                        Cmd_MoveImagesCommand = Cmd_ContextMenu_MoveImages,
                                        Cmd_AddFolderFromListCommand = Cmd_ContextMenu_AddFavoriteTargetFolderItem,
                                        Cmd_RemoveFolderFromListCommand = Cmd_ContextMenu_RemoveFavoriteTargetFolderItem,
                                    });

                                    Properties.Settings.Default.LastUsedPath = selectedPath;
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
                        if (item is FavoriteTargetFolderModel)
                        {
                            this.FavoriteTargetFolderList.Remove (item as FavoriteTargetFolderModel);
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
                                string initialPath = Directory.Exists (Properties.Settings.Default.LastUsedPath) ?
                                                     Properties.Settings.Default.LastUsedPath :
                                                     Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);


                                dialog.Description = LocalizedStrings.GetString ("dlgVirtualDirSec_HelpText");
                                dialog.ShowNewFolderButton = true;
                                dialog.SelectedPath = initialPath;

                                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace (dialog.SelectedPath))
                                {
                                    string selectedPath = dialog.SelectedPath;
                                    string folderName = Path.GetFileName (selectedPath);


                                    // Create a new VirtualDirectoryList instance and add it to the collection.
                                    VirtualRootDirectoryList.Add (new VirtualDirectoryModel
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

                                    Properties.Settings.Default.LastUsedPath = selectedPath;
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
                        if (item is VirtualDirectoryModel)
                        {
                            VirtualFirstStageDirectoryList.Clear ();
                            VirtualSecundStageDirectoryList.Clear ();

                            VirtualRootDirectoryList.Remove (item as VirtualDirectoryModel);

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
                        LoadVirtualFirstStageDirectoryList ((item as VirtualDirectoryModel).Path);
                    },
                    item =>
                    {
                        try
                        {
                            if (item is VirtualDirectoryModel)
                            {
                                if (!String.IsNullOrEmpty ((item as VirtualDirectoryModel).Path) && Directory.Exists ((item as VirtualDirectoryModel).Path))
                                {
                                    return Directory.GetDirectories ((item as VirtualDirectoryModel).Path).Length > 0;
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
                        VirtualDirectoryModel selectedVirtRootDirObject = null;


                        if (item is VirtualDirectoryModel)
                        {
                            // User did right-click on a virtual directory button; which is selected or not.

                            selectedVirtRootDirObject = (VirtualDirectoryModel) item;
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
                                view.UserControls.DlgBoxType.Question,
                                LocalizedStrings.GetString ("lQuestion"),
                                LocalizedStrings.GetString ("dlgNewFolder_QuestionText"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Cancle"),
                                                  view.UserControls.DlgBoxButtonSymbol.Cross,
                                                  null,
                                                  _dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Create"),
                                                    view.UserControls.DlgBoxButtonSymbol.Check,
                                                    selectedVirtRootDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        try
                                                        {
                                                            VirtualDirectoryModel srcVirtDirInstance = (dlgBoxCfg.LeftButton.Parameter as VirtualDirectoryModel);
                                                            string rootPath = srcVirtDirInstance.Path;


                                                            Directory.CreateDirectory (Path.Combine (rootPath, dlgBoxCfg.TextBox.Text));

                                                            LoadVirtualFirstStageDirectoryList (rootPath);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                view.UserControls.DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgNewFolderError_Message", ex.Message));
                                                        }
                                                    }),

                                new DlgBoxTextBox (LocalizedStrings.GetString ("dlgNewFolder_NewFolder"), rules)
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
                        VirtualDirectoryModel selectedVirtDirObject = (VirtualDirectoryModel) item;


                        if (Directory.GetDirectories (selectedVirtDirObject.Path).Length != 0 || Directory.GetFiles (selectedVirtDirObject.Path).Length != 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (

                                view.UserControls.DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgDelayNotEmptyDir_Message", selectedVirtDirObject.Path),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_Cancle"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_DeleteDir"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
                                                    selectedVirtDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        VirtualDirectoryModel selVirtDirObject = (VirtualDirectoryModel) dlgBoxCfg.LeftButton.Parameter;

                                                        try
                                                        {
                                                            Directory.Delete (selVirtDirObject.Path, true);

                                                            // Try to get parent path VirtualDirectory object.
                                                            if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                                            {
                                                                selVirtDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                                                            }

                                                            // Refresh the directory list.
                                                            LoadVirtualFirstStageDirectoryList (selVirtDirObject.Path);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (view.UserControls.DlgBoxType.Error,
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
                                LoadVirtualFirstStageDirectoryList (selectedVirtDirObject.Path);
                            }
                            catch (Exception ex)
                            {
                                this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (view.UserControls.DlgBoxType.Error,
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
                        LoadVirtualSecondStageDirectoryList ((item as VirtualDirectoryModel).Path);
                    },
                    item =>
                    {
                        try
                        {
                            if (item is VirtualDirectoryModel)
                            {
                                if (!String.IsNullOrEmpty ((item as VirtualDirectoryModel).Path) && Directory.Exists ((item as VirtualDirectoryModel).Path))
                                {
                                    return Directory.GetDirectories ((item as VirtualDirectoryModel).Path).Length > 0;
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
                        VirtualDirectoryModel selectedVirtRootDirObject = null;


                        if (item is VirtualDirectoryModel)
                        {
                            // User did right-click on a virtual directory button; which is selected or not.

                            selectedVirtRootDirObject = (VirtualDirectoryModel) item;
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
                                view.UserControls.DlgBoxType.Question,
                                LocalizedStrings.GetString ("lQuestion"),
                                LocalizedStrings.GetString ("dlgNewFolder_QuestionText"),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Cancle"),
                                                  view.UserControls.DlgBoxButtonSymbol.Cross,
                                                  null,
                                                  dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgNewFolder_Create"),
                                                    view.UserControls.DlgBoxButtonSymbol.Check,
                                                    selectedVirtRootDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        try
                                                        {
                                                            VirtualDirectoryModel srcVirtDirInstance = (dlgBoxCfg.LeftButton.Parameter as VirtualDirectoryModel);
                                                            string rootPath = srcVirtDirInstance.Path;


                                                            Directory.CreateDirectory (Path.Combine (rootPath, dlgBoxCfg.TextBox.Text));

                                                            LoadVirtualSecondStageDirectoryList (rootPath);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                                                view.UserControls.DlgBoxType.Error,
                                                                LocalizedStrings.GetString ("lError"),
                                                                LocalizedStrings.GetFormattedString ("dlgNewFolderError_Message", ex.Message));
                                                        }
                                                    }),

                                new DlgBoxTextBox (LocalizedStrings.GetString ("dlgNewFolder_NewFolder"), rules)
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
                        VirtualDirectoryModel selectedVirtDirObject = (VirtualDirectoryModel) item;


                        if (Directory.GetFiles (selectedVirtDirObject.Path).Length != 0 || Directory.GetDirectories (selectedVirtDirObject.Path).Length != 0)
                        {
                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (

                                view.UserControls.DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgDelayNotEmptyDir_Message", selectedVirtDirObject.Path),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_Cancle"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
                                                    null,
                                                    dlgBoxCfg => {; }),

                                new DlgBoxButton (LocalizedStrings.GetString ("dlgDelayNotEmptyDir_DeleteDir"),
                                                    view.UserControls.DlgBoxButtonSymbol.Empty,
                                                    selectedVirtDirObject,
                                                    dlgBoxCfg =>
                                                    {
                                                        VirtualDirectoryModel selVirtDirObject = (VirtualDirectoryModel) dlgBoxCfg.LeftButton.Parameter;

                                                        try
                                                        {
                                                            Directory.Delete (selVirtDirObject.Path, true);

                                                            // Try to get parent path VirtualDirectory object.
                                                            if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                                                            {
                                                                selVirtDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                                                            }

                                                            // Refresh the directory list.
                                                            LoadVirtualSecondStageDirectoryList (selVirtDirObject.Path);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (view.UserControls.DlgBoxType.Error,
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
                                LoadVirtualSecondStageDirectoryList (selectedVirtDirObject.Path);
                            }
                            catch (Exception ex)
                            {
                                this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (view.UserControls.DlgBoxType.Error,
                                    LocalizedStrings.GetString ("lError"),
                                    LocalizedStrings.GetFormattedString ("dlgDeleteDirError_Message", selectedVirtDirObject.Path, ex.Message));
                            }
                        }
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
                            Properties.Settings.Default.FolderTitleSizeLevel = fileTitleSizeLevel;

                            UpdateFileTitleList ();
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
                        LoadImageBufferAsync (true,
                            (infoObj, curCnt, maxCnt) =>
                            {
                                // Add a new image (file info object) to the file title list.

                                _Dispatcher.Invoke (() =>
                                {
                                    this.FileTitleLoadStatus_Show = true;
                                    this.FileTitleLoadStatus_Text = LocalizedStrings.GetFormattedString ("tbFileTitleSec_LodingImages", curCnt, maxCnt);

                                    UpdateFileTitleList (infoObj);
                                });
                            },
                            (errorOccured, errorMessages) =>
                            {
                                // All images have been processed. Check if an error has been occurred.

                                _Dispatcher.Invoke (() =>
                                {
                                    if (errorOccured)
                                    {
                                        string errorMessageString = string.Empty;


                                        errorMessages.ForEach (s => errorMessageString += s + "\n");

                                        this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                            view.UserControls.DlgBoxType.Warning,
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

        private List<ImageFileInfo> _FileTileImageBufferList { get; set; } = new List<ImageFileInfo> ();

        private int _StartSelectionStartIndex = -1;
        private int _EndSelectionStartIndex = -1;

        private string _DlgHelper_MoveFiles_TargetPath;



        public MainViewModel (Dispatcher dispatcher, String path)
        {
            _Dispatcher = dispatcher;
            this.RootPath = path;
            this.FileTileStatusText = this.RootPath;

            SetColorTheme ();
            LoadFavoriteTargetFolderList ();
            LoadVirtualDirectoryList ();

            // Load the image buffer and update the file title list.
            this.FileTileList.Clear ();
            LoadImageBufferAsync (true,
                (infoObj, curCnt, maxCnt) =>
                {
                    // Add a new image (file info object) to the file title list.

                    _Dispatcher.Invoke (() =>
                    {
                        this.FileTitleLoadStatus_Show = true;
                        this.FileTitleLoadStatus_Text = LocalizedStrings.GetFormattedString ("tbFileTitleSec_LodingImages", curCnt, maxCnt);

                        UpdateFileTitleList (infoObj);
                    });
                },
                (errorOccured, errorMessages) =>
                {
                    // All images have been processed. Check if an error has been occurred.

                    _Dispatcher.Invoke (() =>
                    {
                        if (errorOccured)
                        {
                            string errorMessageString = string.Empty;


                            errorMessages.ForEach (s => errorMessageString += s + "\n");

                            this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialogSimply (
                                view.UserControls.DlgBoxType.Warning,
                                LocalizedStrings.GetString ("lWarning"),
                                LocalizedStrings.GetFormattedString ("dlgErrorLoadingImagesFromDir_Message", errorMessageString));
                        }

                        this.FileTitleLoadStatus_Show = false;
                    });
                });
        }



        public void Dispose ()
        {
            // Store the selected folder in the settings.

            Properties.Settings.Default.FavoriteTargetFolderCollection.Clear ();

            foreach (var favTargetFolder in FavoriteTargetFolderList)
            {
                var item = new FavoriteTargetFolderSettingItem (favTargetFolder);

                Properties.Settings.Default.FavoriteTargetFolderCollection.Add (item.ToString ());
            }

            Properties.Settings.Default.VirtualRootDirectoryCollection.Clear ();

            foreach (var virtRootDir in VirtualRootDirectoryList)
            {
                var item = new VirtualRootDirectorySettingItem (virtRootDir);

                Properties.Settings.Default.VirtualRootDirectoryCollection.Add (item.ToString ());
            }
        }



        private Task LoadImageBufferAsync (bool forceFullLoad, Action<ImageFileInfo, int, int> onImageLoad, Action<bool, List<string>> onAllImagesLoad)
        {
            return Task.Run (() =>
            {
                bool errorOccured = false;
                List<string> errorMessages = new List<string> ();


                try
                {
                    var files = Directory.GetFiles (this.RootPath);
                    int fileCnt = 1;


                    if (forceFullLoad)
                    {
                        _FileTileImageBufferList.Clear ();
                    }

                    // Load all images from the directory into our buffer list or update them.
                    foreach (var file in files)
                    {
                        try
                        {
                            String fileExt = Path.GetExtension (file).ToLower ();
                            bool loadImageMustBeExecute = false;
                            ImageFileInfo imageFileInfoToUpdate = null;

                            ImageFileInfo imageFileInfo = new ImageFileInfo () { File = file, Thumbnail = null };


                            // Check if the image is already in the buffer list and or we have to load the (new) image.

                            START_IMAGE_IN_BUFFER_CHECK:

                            var matchingImageBufferList = _FileTileImageBufferList.Where (x => x.File == imageFileInfo.File).ToList ();
                            if (matchingImageBufferList.Count == 0)
                            {
                                loadImageMustBeExecute = true;
                            }
                            else if (matchingImageBufferList.Count == 1)
                            {
                                // Image is in our buffer list, but has the file content been changed?
                                if (imageFileInfo.CompareImageWith (matchingImageBufferList[0]) == false)
                                {
                                    loadImageMustBeExecute = true;
                                    imageFileInfoToUpdate = matchingImageBufferList[0];
                                }
                            }
                            else
                            {
                                // There must be only one image in the buffer list.
                                // If there are more images (it is an error), remove all of them and reload the image.
                                matchingImageBufferList.ForEach (x => _FileTileImageBufferList.Remove (x));

                                goto START_IMAGE_IN_BUFFER_CHECK;
                            }

                            if (loadImageMustBeExecute)
                            {
                                if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png" || fileExt == ".bmp" || fileExt == ".heic")
                                {
                                    // This approach locks the file until the application is closed.
                                    //thumb = new BitmapImage (new Uri (file));

                                    BitmapImage bi = null;


                                    using (var fstream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        bi = new BitmapImage ();
                                        bi.BeginInit ();
                                        bi.CacheOption = BitmapCacheOption.OnLoad;
                                        bi.StreamSource = fstream;
                                        bi.StreamSource.Flush ();
                                        bi.EndInit ();
                                        bi.Freeze ();

                                        bi.StreamSource.Dispose ();
                                    }

                                    imageFileInfo.Thumbnail = bi;
                                    imageFileInfo.IsSysIconImage = false;
                                }
                                else
                                {
                                    // Get windows default icon.
                                    ImageSource imageSource = IconHelper.GetFileIcon (file);
                                    imageFileInfo.Thumbnail = IconHelper.ConvertImageSourceToBitmapImage (imageSource);
                                    imageFileInfo.IsSysIconImage = true;
                                }
                            }

                            if (imageFileInfoToUpdate == null)
                            {
                                if (loadImageMustBeExecute)
                                {
                                    // Add new image item to buffer list.
                                    _FileTileImageBufferList.Add (imageFileInfo);
                                }
                            }
                            else
                            {
                                // Update existing item in buffer.
                                imageFileInfoToUpdate = imageFileInfo;
                            }

                            // Execute / fire the onImageLoad callback handler.
                            onImageLoad?.Invoke (imageFileInfo, fileCnt++, files.Length);
                        }
                        catch (Exception ex)
                        {
                            errorOccured = true;
                            errorMessages.Add ($"{file} -> {ex.Message}");
                        }
                    }

                    // Remove none existing files from the buffer list.
                    _FileTileImageBufferList.RemoveAll (x => !x.FileExists);

                    // Execute / fire the onAllImageLoad callback handler.
                    onAllImagesLoad?.Invoke (errorOccured, errorMessages);
                }
                catch (Exception ex)
                {
                    errorOccured = true;
                    errorMessages.Add (ex.Message);

                    // Execute / fire the onAllImageLoad callback handler.
                    onAllImagesLoad?.Invoke (errorOccured, errorMessages);
                }
            });
        }



        private void UpdateFileTitleList (ImageFileInfo singleImageFileInfo = null)
        {
            int height = 128;
            int width = 128;


            switch (Properties.Settings.Default.FolderTitleSizeLevel)
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

            if (singleImageFileInfo == null)
            {
                this.FileTileList.Clear ();

                foreach (var imageFileInfoItem in _FileTileImageBufferList)
                {
                    if (imageFileInfoItem.FileExists)
                    {
                        FileTileList.Add (new FileTitleModel
                        {
                            DisplayName = Path.GetFileName (imageFileInfoItem.File),
                            Thumbnail = imageFileInfoItem.Thumbnail,
                            Height = height,
                            Width = width,
                            HideFilenameText = !Properties.Settings.Default.ShowImageFileName,
                            SizeLevel = Properties.Settings.Default.FolderTitleSizeLevel,

                            File = imageFileInfoItem.File,

                            IsSysIconImage = imageFileInfoItem.IsSysIconImage,
                        });
                    }
                }
            }
            else
            {
                if (singleImageFileInfo.FileExists)
                {
                    FileTileList.Add (new FileTitleModel
                    {
                        DisplayName = Path.GetFileName (singleImageFileInfo.File),
                        Thumbnail = singleImageFileInfo.Thumbnail,
                        Height = height,
                        Width = width,
                        HideFilenameText = !Properties.Settings.Default.ShowImageFileName,
                        SizeLevel = Properties.Settings.Default.FolderTitleSizeLevel,

                        File = singleImageFileInfo.File,

                        IsSysIconImage = singleImageFileInfo.IsSysIconImage,
                    });
                }
            }
        }



        private void LoadFavoriteTargetFolderList ()
        {
            try
            {
                List<FavoriteTargetFolderSettingItem> favTargetFolderList = new List<FavoriteTargetFolderSettingItem> ();


                // Check if the Properties.Settings.FavoriteTargetFolderCollection setting exists, if not, create it.
                if (Properties.Settings.Default.FavoriteTargetFolderCollection == null)
                {
                    Properties.Settings.Default.FavoriteTargetFolderCollection = new System.Collections.Specialized.StringCollection ();
                }

                // Clear the UI collections.
                FavoriteTargetFolderList.Clear ();

                // Transfer the Properties.Settings.Default.FavoriteTargetFolderCollection entries into the favTargetFolderList for linq operations.
                foreach (var targetFolderItemString in Properties.Settings.Default.FavoriteTargetFolderCollection)
                {
                    favTargetFolderList.Add (FavoriteTargetFolderSettingItem.Parse (targetFolderItemString));
                }

                // Order and sort the list.
                // Only entries that are younger / newer then 30 days or pinned entries are shown.
                long dayLimitThreshold = DateTime.UtcNow.AddDays (-30).ToFileTimeUtc ();
                var querryList = favTargetFolderList
                    .Where (x => Directory.Exists (x.Path) && (x.Date > dayLimitThreshold) || (x.IsPinned))
                    .OrderByDescending (x => x.Date);

                foreach (var querryItem in querryList)
                {
                    FavoriteTargetFolderList.Add (new FavoriteTargetFolderModel
                    {
                        DisplayName = querryItem.DisplayName,
                        Path = querryItem.Path,
                        AddDate = querryItem.Date,
                        IsPinned = querryItem.IsPinned,
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
                List<VirtualRootDirectorySettingItem> virtRootDirList = new List<VirtualRootDirectorySettingItem> ();


                // Check if the Properties.Settings.VirtualRootDirectoryCollection setting exists, if not, create it.
                if (Properties.Settings.Default.VirtualRootDirectoryCollection == null)
                {
                    Properties.Settings.Default.VirtualRootDirectoryCollection = new System.Collections.Specialized.StringCollection ();
                }

                // Clear the UI collections.
                VirtualRootDirectoryList.Clear ();

                // Transfer the Properties.Settings.Default.VirtualRootDirectoryCollection entries into the virtRootDirList for linq operations.
                foreach (var virtRootDirItemstring in Properties.Settings.Default.VirtualRootDirectoryCollection)
                {
                    virtRootDirList.Add (VirtualRootDirectorySettingItem.Parse (virtRootDirItemstring));
                }

                // Order and sort the list.
                var querryList = virtRootDirList.Where (x => Directory.Exists (x.Path));

                foreach (var querryItem in querryList)
                {
                    VirtualRootDirectoryList.Add (new VirtualDirectoryModel
                    {
                        DisplayName = querryItem.DisplayName,
                        Path = querryItem.Path,

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



        private void PrepareMoveFiles (string targetPath)
        {
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();


            if (querrySelectedFileList.Count () > 0)
            {
                if (Properties.Settings.Default.ShowMoveDlg)
                {
                    this.DialogBoxConfiguration = DlgBoxViewModel.ShowDialog (
                        view.UserControls.DlgBoxType.Question,
                        LocalizedStrings.GetString ("lQuestion"),
                        LocalizedStrings.GetFormattedString ("dlgFileMove_QuestionText", querrySelectedFileList.Count (), Path.GetFileName (targetPath)),

                        new DlgBoxButton (LocalizedStrings.GetString ("dlgFileMove_Cancle"),
                            view.UserControls.DlgBoxButtonSymbol.Check,
                            null,
                            dlgBoxCfg => {; }),

                        new DlgBoxButton (LocalizedStrings.GetString ("dlgFileMove_StartMove"),
                            view.UserControls.DlgBoxButtonSymbol.Check,
                            targetPath,
                            dlgBoxCfg => { MoveFiles (dlgBoxCfg.LeftButton.Parameter as String); }),

                        null);
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
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTitleModel> ();


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

                    if (QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionAutoInsert)
                    {
                        // Check favorite target folder list limit.
                        if (FavoriteTargetFolderList.Count >= QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionLimit)
                        {
                            // Remove the oldest entry that is not pinned.
                            var itemToRemove = FavoriteTargetFolderList.Where (x => x.IsPinned == false).OrderBy (x => x.AddDate).FirstOrDefault ();
                            if (itemToRemove != null)
                            {
                                FavoriteTargetFolderList.Remove (itemToRemove);
                            }
                        }

                        FavoriteTargetFolderList.Add (new FavoriteTargetFolderModel
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
                    List<FileTitleModel> querrySelectedFileList_Local = querrySelectedFileList;
                    var cts = new CancellationTokenSource ();
                    var popup = new FileMoveProcPopupNotificationModel ()
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
                        UpdateFileTitleList ();
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
            switch (QuickSort.Properties.Settings.Default.ColorThemeId)
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



        private void LoadVirtualFirstStageDirectoryList (string path)
        {
            VirtualFirstStageDirectoryList.Clear ();
            VirtualSecundStageDirectoryList.Clear ();

            try
            {
                if (!string.IsNullOrEmpty (path) && Directory.Exists (path))
                {
                    String[] subDirList = Directory.GetDirectories (path);


                    foreach (var subDir in subDirList)
                    {
                        // Create a new VirtualDirectoryList instance and add it to the collection.
                        VirtualFirstStageDirectoryList.Add (new VirtualDirectoryModel
                        {
                            DisplayName = Path.GetFileName (subDir),
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
            }
            catch
            {
                ;
            }
        }



        private void LoadVirtualSecondStageDirectoryList (string path)
        {
            VirtualSecundStageDirectoryList.Clear ();

            try
            {
                if (!string.IsNullOrEmpty (path) && Directory.Exists (path))
                {
                    String[] subDirList = Directory.GetDirectories (path);


                    foreach (var subDir in subDirList)
                    {
                        // Create a new VirtualDirectoryList instance and add it to the collection.
                        VirtualSecundStageDirectoryList.Add (new VirtualDirectoryModel
                        {
                            DisplayName = Path.GetFileName (subDir),
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
            }
            catch
            {
                ;
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
