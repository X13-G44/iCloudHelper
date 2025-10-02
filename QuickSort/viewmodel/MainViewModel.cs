using QuickSort.help;
using QuickSort.model;
using QuickSort.validationrules;
using QuickSort.view;
using QuickSort.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml.Linq;
using static QuickSort.validationrules.CheckDirectoryNameValidationRule;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;



namespace QuickSort.viewmodel
{
    public class MainViewModel : IDisposable, INotifyPropertyChanged
    {
        private class DlgHelperCreateNewDirectory
        {
            public VirtualDirectoryModel SourceModel { get; private set; }
            public Action<VirtualDirectoryModel> ListRefreshFunction { get; private set; }



            public DlgHelperCreateNewDirectory (VirtualDirectoryModel sourceModelInstance, Action<VirtualDirectoryModel> listRefreshFunction)
            {
                this.SourceModel = sourceModelInstance;
                this.ListRefreshFunction = listRefreshFunction;
            }
        }



        public ObservableCollection<FavoriteTargetFolderModel> FavoriteTargetFolderList { get; set; } = new ObservableCollection<FavoriteTargetFolderModel> ();

        public ObservableCollection<FileTileModel> FileTileList { get; set; } = new ObservableCollection<FileTileModel> ();

        public ObservableCollection<VirtualDirectoryModel> VirtualRootDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();
        public ObservableCollection<VirtualDirectoryModel> VirtualFirstStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();
        public ObservableCollection<VirtualDirectoryModel> VirtualSecundStageDirectoryList { get; set; } = new ObservableCollection<VirtualDirectoryModel> ();

        public ObservableCollection<FileMoveProcPopupNotificationModel> FileMoveProcPopupNotificationList { get; } = new ObservableCollection<FileMoveProcPopupNotificationModel> ();



        private bool _DialogOverlay_MoveFiles_Show = false;
        public bool DialogOverlay_MoveFiles_Show
        {
            get { return _DialogOverlay_MoveFiles_Show; }
            set { _DialogOverlay_MoveFiles_Show = value; OnPropertyChanged (nameof (DialogOverlay_MoveFiles_Show)); }
        }

        private int _DialogOverlay_MoveFiles_FileCount = 0;
        public int DialogOverlay_MoveFiles_FileCount
        {
            get { return _DialogOverlay_MoveFiles_FileCount; }
            set { _DialogOverlay_MoveFiles_FileCount = value; OnPropertyChanged (nameof (DialogOverlay_MoveFiles_FileCount)); }
        }

        private string _DialogOverlay_MoveFiles_ShortTargetPath = "";
        public string DialogOverlay_MoveFiles_ShortTargetPath
        {
            get { return _DialogOverlay_MoveFiles_ShortTargetPath; }
            set { _DialogOverlay_MoveFiles_ShortTargetPath = value; OnPropertyChanged (nameof (DialogOverlay_MoveFiles_ShortTargetPath)); }
        }

        private bool _DialogOverlay_ProcessHeicImages_Show = false;
        public bool DialogOverlay_ProcessHeicImages_Show
        {
            get { return _DialogOverlay_ProcessHeicImages_Show; }
            set { _DialogOverlay_ProcessHeicImages_Show = value; OnPropertyChanged (nameof (DialogOverlay_ProcessHeicImages_Show)); }
        }

        private bool _DialogOverlay_NewDirectoryName_Show;
        public bool DialogOverlay_NewDirectoryName_Show
        {
            get { return _DialogOverlay_NewDirectoryName_Show; }
            set { _DialogOverlay_NewDirectoryName_Show = value; OnPropertyChanged (nameof (DialogOverlay_NewDirectoryName_Show)); }
        }

        private string _DialogOverlay_NewDirectoryName_Name;
        public string DialogOverlay_NewDirectoryName_Name
        {
            get { return _DialogOverlay_NewDirectoryName_Name; }
            set { _DialogOverlay_NewDirectoryName_Name = value; OnPropertyChanged (nameof (DialogOverlay_NewDirectoryName_Name)); }
        }

        private string _DialogOverly_NewDirectoryRootPath;
        public string DialogOverly_NewDirectoryRootPath
        {
            get { return _DialogOverly_NewDirectoryRootPath; }
            set { _DialogOverly_NewDirectoryRootPath = value; OnPropertyChanged (nameof (DialogOverly_NewDirectoryRootPath)); }
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



        public RelayCommand Cmd_ShowConfigWindow
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        var dialog = new ConfigView ();


                        dialog.ShowDialog ();

                        if (dialog.DialogResult.Value)
                        {
                            this.RootPath = QuickSort.Properties.Settings.Default.StartPath;

                            SetColorTheme ();
                            LoadFileTitleList ();
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
                            if (System.Windows.MessageBox.Show ("Es werden gerade noch Dateien im Hintergrund bearbeitet.\n\nWenn Sie die Anwendung beenden, wird dieser Vorganng abgebrochen. Es " +
                                "werden nicht alle ausgewählten Dateien in den Ziel-Ordner verschoben sein!\n\nSoll die Anwendung wirklich beendet werden?",
                                "Warnung", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes) == MessageBoxResult.OK)
                            {
                                return;
                            }
                        }

                        App.Current.Shutdown (0);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();


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
                        if (item is FileTileModel)
                        {
                            bool shiftKeyIsPressed = false;


                            (item as FileTileModel).IsSelected = !(item as FileTileModel).IsSelected;

                            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift))
                            {
                                shiftKeyIsPressed = true;
                            }

                            if (shiftKeyIsPressed == false)
                            {
                                _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTileModel);
                            }

                            else if (shiftKeyIsPressed == true)
                            {
                                if (_StartSelectionStartIndex > -1)
                                {
                                    int deltaIndex;
                                    int lowIndex;


                                    _EndSelectionStartIndex = this.FileTileList.IndexOf (item as FileTileModel);

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
                                    _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTileModel);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();
                        string targetPath = (item as VirtualDirectoryModel).Path;


                        if (querrySelectedFileList.Count > 0)
                        {
                            PrepareMoveFiles (targetPath);
                        }
                        else
                        {
                            LoadVirtualFirstStageDirectoryList (item as VirtualDirectoryModel);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();
                        string targetPath = (item as VirtualDirectoryModel).Path;


                        if (querrySelectedFileList.Count > 0)
                        {
                            PrepareMoveFiles (targetPath);
                        }
                        else
                        {
                            LoadVirtualSecondStageDirectoryList (item as VirtualDirectoryModel);
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
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();
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
                            MoveFiles (targetPath);
                        }
                    },
                    item =>
                    {
                        var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();


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


                                dialog.Description = "Zielordner auswählen";
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


                                dialog.Description = "Zielordner auswählen";
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
                        try
                        {
                            LoadVirtualFirstStageDirectoryList (item as VirtualDirectoryModel);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_ShowVirtualFirstStageDirectoryItem: {ex.Message}");
                        }
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
                        try
                        {
                            VirtualDirectoryModel selectedVirtRootDirObject = null;


                            if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtRootDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                            }
                            else
                            {
                                selectedVirtRootDirObject = (VirtualDirectoryModel) item;
                            }

                            if (selectedVirtRootDirObject != null)
                            {
                                this.DialogOverlay_NewDirectoryName_Show = true;
                                this.DialogOverlay_NewDirectoryName_Name = "Neuer Ordner";
                                this.DialogOverly_NewDirectoryRootPath = selectedVirtRootDirObject.Path;

                                this._DlgHelper_CreateNewDirectory_Data = new DlgHelperCreateNewDirectory (selectedVirtRootDirObject,
                                    (srcVirtDirInstance) =>
                                    {
                                        this.LoadVirtualFirstStageDirectoryList (srcVirtDirInstance);
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_VirtualFirstStageCreateDirectory: {ex.Message}");
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
                        try
                        {
                            VirtualDirectoryModel selectedVirtDirObject = (VirtualDirectoryModel) item;


                            if (Directory.GetDirectories (selectedVirtDirObject.Path).Length > 0 || Directory.GetFiles (selectedVirtDirObject.Path).Length > 0)
                            {
                                if (System.Windows.MessageBox.Show ($"The selected directory:\n\n\t\"{selectedVirtDirObject.Path}\"\n\n" +
                                    $"is not empty!\n\n" +
                                    $"Are your sure to delete the directory?",
                                    $"{App.APP_TITLE} - Warning",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Warning,
                                    MessageBoxResult.No) != MessageBoxResult.Yes)
                                {
                                    return;
                                }
                            }

                            Directory.Delete (selectedVirtDirObject.Path, true);

                            // Try to get parent path VirtualDirectory object.
                            if (this.VirtualRootDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtDirObject = this.VirtualRootDirectoryList.Where (x => x.IsSelected).First ();
                            }

                            // Refresh the directory list.
                            LoadVirtualFirstStageDirectoryList (selectedVirtDirObject);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_VirtualFirstStageDeleteDirectory: {ex.Message}");
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
                        try
                        {
                            LoadVirtualSecondStageDirectoryList (item as VirtualDirectoryModel);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_ShowVirtualSecondStageDirectoryItem: {ex.Message}");
                        }
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
                        try
                        {
                            VirtualDirectoryModel selectedVirtRootDirObject = null;


                            if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtRootDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                            }
                            else
                            {
                                selectedVirtRootDirObject = (VirtualDirectoryModel) item;
                            }

                            if (selectedVirtRootDirObject != null)
                            {
                                this.DialogOverlay_NewDirectoryName_Show = true;
                                this.DialogOverlay_NewDirectoryName_Name = "Neuer Ordner";
                                this.DialogOverly_NewDirectoryRootPath = selectedVirtRootDirObject.Path;

                                this._DlgHelper_CreateNewDirectory_Data = new DlgHelperCreateNewDirectory (selectedVirtRootDirObject,
                                    (srcVirtDirInstance) =>
                                    {
                                        this.LoadVirtualSecondStageDirectoryList (srcVirtDirInstance);
                                    });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_VirtualSecondStageCreateDirectory: {ex.Message}");
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
                        try
                        {
                            VirtualDirectoryModel selectedVirtDirObject = (VirtualDirectoryModel) item;


                            if (Directory.GetDirectories (selectedVirtDirObject.Path).Length > 0 || Directory.GetFiles (selectedVirtDirObject.Path).Length > 0)
                            {
                                if (System.Windows.MessageBox.Show ($"The selected directory:\n\n\t\"{selectedVirtDirObject.Path}\"\n\n" +
                                $"is not empty!\n\n" +
                                $"Are your sure to delete the directory?",
                                $"{App.APP_TITLE} - Warning",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Warning,
                                MessageBoxResult.No) != MessageBoxResult.Yes)
                                {
                                    return;
                                }
                            }

                            Directory.Delete (selectedVirtDirObject.Path, true);

                            // Try to get parent path VirtualDirectory object.
                            if (this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).Count () > 0)
                            {
                                selectedVirtDirObject = this.VirtualFirstStageDirectoryList.Where (x => x.IsSelected).First ();
                            }

                            // Refresh the directory list.
                            LoadVirtualSecondStageDirectoryList (selectedVirtDirObject);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_ContextMenu_VirtualSecondStageDeleteDirectory: {ex.Message}");
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

                            LoadFileTitleList ();
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
                        PrepareLoadFileTitleList ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_StartFileMoveProcess
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DialogOverlay_MoveFiles_Show = false;
                        this.MoveFiles (_DlgHelper_MoveFiles_TargetPath);
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_AbortStartFileMoveProcess
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DialogOverlay_MoveFiles_Show = false;
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_ProcessHeicImages
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DialogOverlay_ProcessHeicImages_Show = false;
                        this._ProcessHeicImages = true;
                        this.LoadFileTitleList ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_DontProcessHeicImages
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DialogOverlay_ProcessHeicImages_Show = false;
                        this._ProcessHeicImages = false;
                        this.LoadFileTitleList ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_NewDirectoryNameAccept
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            string newPath = String.Empty;
                            CheckDirectoryNameResult checkNewDirNameResult = CheckDirectoryNameResult.Success;


                            DialogOverlay_NewDirectoryName_Show = false;

                            checkNewDirNameResult = CheckDirectoryNameValidationRule.IsValidDirectoryName (DialogOverlay_NewDirectoryName_Name, _DlgHelper_CreateNewDirectory_Data.SourceModel.Path);

                            if (checkNewDirNameResult == CheckDirectoryNameResult.InvalidDirectoryName)
                            {
                                System.Windows.MessageBox.Show ($"The directory name \"{DialogOverlay_NewDirectoryName_Name}\" is not valid.",
                                                                $"{App.APP_TITLE} - Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                                return;
                            }
                            else if (checkNewDirNameResult == CheckDirectoryNameResult.DirectoryAlreadyExists)
                            {
                                System.Windows.MessageBox.Show ($"A directory with the name \"{DialogOverlay_NewDirectoryName_Name}\" already exists.",
                                                                $"{App.APP_TITLE} - Error",
                                                                MessageBoxButton.OK,
                                                                MessageBoxImage.Error);
                                return;
                            }

                            Directory.CreateDirectory (Path.Combine (_DlgHelper_CreateNewDirectory_Data.SourceModel.Path, DialogOverlay_NewDirectoryName_Name));

                            // Refresh the directory list.
                            _DlgHelper_CreateNewDirectory_Data.ListRefreshFunction (_DlgHelper_CreateNewDirectory_Data.SourceModel);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exception in func Cmd_Dlg_NewDirectoryNameAccept: {ex.Message}");
                        }
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_Dlg_NewDirectoryNameAbort
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        DialogOverlay_NewDirectoryName_Show = false;
                    },
                    param => true
                );
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        private Dispatcher _Dispatcher;
        private int _StartSelectionStartIndex = -1;
        private int _EndSelectionStartIndex = -1;

        private bool _ProcessHeicImages = true;

        private string _DlgHelper_MoveFiles_TargetPath;
        private DlgHelperCreateNewDirectory _DlgHelper_CreateNewDirectory_Data;



        public MainViewModel (Dispatcher dispatcher, String path)
        {
            _Dispatcher = dispatcher;
            this.RootPath = path;
            this.FileTileStatusText = this.RootPath;

            SetColorTheme ();
            PrepareLoadFileTitleList ();
            LoadFavoriteTargetFolderList ();
            LoadVirtualDirectoryList ();
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



        private void PrepareLoadFileTitleList ()
        {
            try
            {
                if (IsHeicImagePresent () == false)
                {
                    LoadFileTitleList ();
                }
                else
                {
                    this.DialogOverlay_ProcessHeicImages_Show = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"Error loading files from path {this.RootPath}: {ex.Message}");
            }
        }



        private void LoadFileTitleList ()
        {
            try
            {
                var files = Directory.GetFiles (this.RootPath);
                int height = 0;
                int width = 0;


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

                FileTileList.Clear ();

                foreach (var file in files)
                {
                    String ext = Path.GetExtension (file).ToLower ();
                    ImageSource thumb;


                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
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

                        thumb = bi;
                    }
                    else if (ext == ".heic" && _ProcessHeicImages)
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

                        thumb = bi;
                    }
                    else
                    {
                        // Get windows default icon.
                        thumb = IconHelper.GetFileIcon (file);
                    }

                    FileTileList.Add (new FileTileModel
                    {
                        DisplayName = Path.GetFileName (file),
                        Thumbnail = thumb,
                        Height = height,
                        Width = width,
                        HideFilenameText = !Properties.Settings.Default.ShowImageFileName,
                        SizeLevel = Properties.Settings.Default.FolderTitleSizeLevel,

                        File = file,
                        Filesize = (int) (new FileInfo (file).Length / 1024), // Convert site from byte to kB.
                        CreationTime = File.GetCreationTime (file),
                        LastAccessTime = File.GetLastAccessTime (file),
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"Error loading files from path {this.RootPath}: {ex.Message}");
            }
        }



        private bool IsHeicImagePresent ()
        {
            var files = Directory.GetFiles (this.RootPath);


            foreach (var file in files)
            {
                String ext = Path.GetExtension (file).ToLower ();


                if (ext == ".heic")
                {
                    return true;
                }
            }

            return false;
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
                Debug.WriteLine ($"Error loading target folders from settings: {ex.Message}");
            }
        }



        private void PrepareMoveFiles (string targetPath)
        {
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();


            if (querrySelectedFileList.Count () > 0)
            {
                if (Properties.Settings.Default.ShowMoveDlg)
                {
                    this.DialogOverlay_MoveFiles_Show = true;
                    this.DialogOverlay_MoveFiles_FileCount = querrySelectedFileList.Count;
                    this._DlgHelper_MoveFiles_TargetPath = targetPath;
                    this.DialogOverlay_MoveFiles_ShortTargetPath = Path.GetFileName (targetPath);
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
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTileModel> ();


            if (querrySelectedFileList.Count () > 0)
            {
                // Append the target path to the favorite target folder list - or update the add date if already existing.
                var existingFavTargetFolderItem = FavoriteTargetFolderList.Where (x => x.Path == targetPath).FirstOrDefault ();
                if (existingFavTargetFolderItem != null)
                {
                    existingFavTargetFolderItem.AddDate = DateTime.Now.ToFileTimeUtc ();
                }
                else
                {
                    if (QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionAutoInsert)
                    {
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
                    List<FileTileModel> querrySelectedFileList_Local = querrySelectedFileList;


                    var popup = new FileMoveProcPopupNotificationModel ()
                    {
                        TargetPath = Path.GetFileName (targetPath),
                        FileCount = querrySelectedFileList_Local.Count (),
                        FileProcessed = 0,
                        CurrentFileName = "",
                    };
                    popup.Cmd_ClosePopup = new RelayCommand (async _ =>
                    {
                        popup.IsFadingOut = true;
                        await Task.Delay (500);
                        FileMoveProcPopupNotificationList.Remove (popup);
                    });


                    _Dispatcher.Invoke (() => FileMoveProcPopupNotificationList.Add (popup));

                    foreach (var fileItem in querrySelectedFileList_Local)
                    {
                        string targetFile = Path.Combine (targetPath as string, fileItem.DisplayName);


                        if (File.Exists (fileItem.File) == true && File.Exists (targetFile) == false)
                        {
                            try
                            {
                                Debug.WriteLine ($"Moving fileItem {fileItem.File} to {targetFile}");

                                _Dispatcher.Invoke (() => popup.FileProcessed++);
                                _Dispatcher.Invoke (() => popup.CurrentFileName = fileItem.DisplayName);

                                File.Move (fileItem.File, targetFile);
                                File.Delete (fileItem.File);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine ($"Error moving fileItem {fileItem.File} to {targetFile}: {ex.Message}");
                            }
                        }
                    }

                    _Dispatcher.Invoke (() => popup.Cmd_ClosePopup?.Execute (popup));
                }).ContinueWith (t =>
                {
                    if (t.IsFaulted)
                    {
                        Debug.WriteLine ($"Error moving files: {t.Exception?.Message}");
                    }

                    // Refresh the file title list.
                    _Dispatcher.Invoke (() => LoadFileTitleList ());
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
            var oldDict = Application.Current.Resources.MergedDictionaries.FirstOrDefault (d => d.Source != null && (d.Source.OriginalString.Contains ("/view/theme/ColorThemeDarkMode.xaml") ||
                                                                                                                     d.Source.OriginalString.Contains ("/view/theme/ColorThemeLightMode.xaml")));
            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove (oldDict);
            }

            // Add new theme.
            Application.Current.Resources.MergedDictionaries.Add (dict);
        }



        private void UpdateFileTitelStatusText ()
        {
            var count = this.FileTileList.Count (x => x.IsSelected);
            if (count > 1)
            {
                this.FileTileStatusText = $"{count} Bilder ausgewählt";
            }
            else if (count == 1)
            {
                this.FileTileStatusText = $"{count} Bild ausgewählt";
            }
            else
            {
                this.FileTileStatusText = this.RootPath;
            }
        }



        private void LoadVirtualFirstStageDirectoryList (VirtualDirectoryModel modelObject)
        {
            VirtualFirstStageDirectoryList.Clear ();
            VirtualSecundStageDirectoryList.Clear ();

            try
            {
                // Unselect all VirtualRootDirectoryList items.
                var selectedVirtRootDirItems = VirtualRootDirectoryList.Where (x => x.IsSelected);
                foreach (var virtRootDirItem in selectedVirtRootDirItems)
                {
                    virtRootDirItem.IsSelected = false;
                }

                // Select the clicked item.
                modelObject.IsSelected = true;


                // Process the sub directories.


                if (!string.IsNullOrEmpty (modelObject.Path) && Directory.Exists (modelObject.Path))
                {
                    String[] subDirList = Directory.GetDirectories (modelObject.Path);


                    foreach (var subDir in subDirList)
                    {
                        string folderName = Path.GetFileName (subDir);


                        // Create a new VirtualDirectoryList instance and add it to the collection.
                        VirtualFirstStageDirectoryList.Add (new VirtualDirectoryModel
                        {
                            DisplayName = folderName,
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



        private void LoadVirtualSecondStageDirectoryList (VirtualDirectoryModel modelObject)
        {
            VirtualSecundStageDirectoryList.Clear ();

            try
            {
                // Unselect all VirtualFirstDirectoryList items.
                var selectedVirtFirstDirItems = VirtualFirstStageDirectoryList.Where (x => x.IsSelected);
                foreach (var virtFirstDirItem in selectedVirtFirstDirItems)
                {
                    virtFirstDirItem.IsSelected = false;
                }

                // Select the clicked item.
                modelObject.IsSelected = true;


                // Process the sub directories.


                if (!string.IsNullOrEmpty (modelObject.Path) && Directory.Exists (modelObject.Path))
                {
                    String[] subDirList = Directory.GetDirectories (modelObject.Path);


                    foreach (var subDir in subDirList)
                    {
                        string folderName = Path.GetFileName (subDir);


                        // Create a new VirtualDirectoryList instance and add it to the collection.
                        VirtualSecundStageDirectoryList.Add (new VirtualDirectoryModel
                        {
                            DisplayName = folderName,
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



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
