using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using System.ComponentModel;
using QuickSort.viewmodel;
using QuickSort.model;
using QuickSort.help;



namespace QuickSort.view
{
    public class MainViewModel : IDisposable, INotifyPropertyChanged
    {
        public ObservableCollection<FileTile> FileTileList { get; set; } = new ObservableCollection<FileTile> ();
        public ObservableCollection<TargetFolder> TargetFolderList { get; set; } = new ObservableCollection<TargetFolder> ();
        public ObservableCollection<FileMoveProcPopupNotification> FileMoveProcPopupNotificationList { get; } = new ObservableCollection<FileMoveProcPopupNotification> ();

        private bool _DlgShow = false;
        public bool DlgShow
        {
            get { return _DlgShow; }
            set { _DlgShow = value; OnPropertyChanged (nameof (DlgShow)); }
        }

        private int _DlgFileCount = 0;
        public int DlgFileCount
        {
            get { return _DlgFileCount; }
            set { _DlgFileCount = value; OnPropertyChanged (nameof (DlgFileCount)); }
        }

        private string _DlgTargetPath = "";
        public string DlgTargetPath
        {
            get { return _DlgTargetPath; }
            set { _DlgTargetPath = value; OnPropertyChanged (nameof (DlgTargetPath)); }
        }

        private string _DlgDisplayTargetPath = "";
        public string DlgDisplayTargetPath
        {
            get { return _DlgDisplayTargetPath; }
            set { _DlgDisplayTargetPath = value; OnPropertyChanged (nameof (DlgDisplayTargetPath)); }
        }

        private string _RootPath = "";
        public string RootPath
        {
            get { return _RootPath; }
            set { _RootPath = value; OnPropertyChanged (nameof (RootPath)); }
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

        public RelayCommand Cmd_AddNewTargetFolder
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
                                dialog.Description = "Zielordner auswählen";
                                dialog.ShowNewFolderButton = true;
                                dialog.SelectedPath = Properties.Settings.Default.LastTargetFolderAdded ?? Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
                                System.Windows.Forms.DialogResult result = dialog.ShowDialog ();


                                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace (dialog.SelectedPath))
                                {
                                    string selectedFolder = dialog.SelectedPath;
                                    string folderName = Path.GetFileName (selectedFolder);


                                    // Create a new TargetFolderList instance and add it to the collection.
                                    TargetFolderList.Add (new TargetFolder
                                    {
                                        DisplayName = folderName,
                                        Path = selectedFolder,
                                        AddDate = DateTime.Now.ToFileTimeUtc (),
                                        IsPinned = false,
                                        Cmd_OpenFolderCommand = new RelayCommand (param => CommandExecute_MoveFiles (param as string))
                                    });

                                    // Also, the selected files will be moved to the selected folder.
                                    CommandExecute_MoveFiles (selectedFolder);

                                    Properties.Settings.Default.LastTargetFolderAdded = selectedFolder;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine ($"Exeption in func Cmd_AddNewTargetFolder: {ex.Message}");
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
                            if (item.IsSelected != true)
                                item.IsSelected = true;
                        }
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
                            if (item.IsSelected == true)
                                item.IsSelected = false;
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
                            Properties.Settings.Default.FolderTitleSizeLevel = fileTitleSizeLevel;

                            LoadFilesTitles ();
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
                        LoadFilesTitles ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_StartFileMoveProcess
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DlgShow = false;
                        this.CommandExecute_MoveFiles (this.DlgTargetPath, true);
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_AbortStartFileMoveProcess
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.DlgShow = false;
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
                        if (item is FileTile)
                        {
                            bool shiftKeyIsPressed = false;


                            (item as FileTile).IsSelected = !(item as FileTile).IsSelected;

                            if (Keyboard.IsKeyDown (Key.LeftShift) || Keyboard.IsKeyDown (Key.RightShift))
                            {
                                shiftKeyIsPressed = true;
                            }

                            if (shiftKeyIsPressed == false)
                            {
                                _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTile);
                            }

                            else if (shiftKeyIsPressed == true)
                            {
                                if (_StartSelectionStartIndex > -1)
                                {
                                    int deltaIndex;
                                    int lowIndex;


                                    _EndSelectionStartIndex = this.FileTileList.IndexOf (item as FileTile);

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
                                    _StartSelectionStartIndex = this.FileTileList.IndexOf (item as FileTile);
                                }
                            }
                        }
                    },
                    param => true
                );
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        private Dispatcher _Dispatcher;
        private int _StartSelectionStartIndex = -1;
        private int _EndSelectionStartIndex = -1;



        public MainViewModel (Dispatcher dispatcher, String path)
        {
            _Dispatcher = dispatcher;
            _RootPath = path;

            LoadFilesTitles ();
            LoadTargetFolder ();
        }



        public void Dispose ()
        {
            // Store the selected folder in the settings.

            Properties.Settings.Default.LastTargetFolderCollection.Clear ();

            foreach (var targetFolder in TargetFolderList)
            {
                var item = new TargetFolderSettingItem (targetFolder.Path, targetFolder.AddDate, targetFolder.DisplayName, targetFolder.IsPinned);

                Properties.Settings.Default.LastTargetFolderCollection.Add (item.ToString ());
            }
        }



        private void LoadFilesTitles ()
        {
            try
            {
                var files = Directory.GetFiles (this.RootPath);
                int height = 0;
                int width = 0;
                bool hideFilenameText = false;


                switch (Properties.Settings.Default.FolderTitleSizeLevel)
                {
                    case 1:
                    default:
                        {
                            // Middle symbol size.
                            height = width = 100;
                            hideFilenameText = false;
                            break;
                        }

                    case 0:
                        {
                            // Small symbol size.
                            height = width = 48;
                            hideFilenameText = true;
                            break;
                        }

                    case 2:
                        {
                            // Large symbol size.
                            height = width = 400;
                            hideFilenameText = false;
                            break;
                        }
                }

                FileTileList.Clear ();

                foreach (var file in files)
                {
                    String ext = Path.GetExtension (file).ToLower ();
                    ImageSource thumb;


                    if (ext == ".jpg" || ext == ".png" || ext == ".bmp")
                    {
                        thumb = new BitmapImage (new Uri (file));
                    }
                    else
                    {
                        // Get windows default icon.
                        thumb = IconHelper.GetFileIcon (file);
                    }

                    FileTileList.Add (new FileTile
                    {
                        DisplayName = Path.GetFileName (file),
                        Thumbnail = thumb,
                        Height = height,
                        Width = width,
                        HideFilenameText = hideFilenameText,
                        SizeLevel = Properties.Settings.Default.FolderTitleSizeLevel,

                        File = file,
                        Filesize = (int)(new FileInfo (file).Length / 1024), // Convert to kB.
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



        private void LoadTargetFolder ()
        {
            try
            {
                List<TargetFolderSettingItem> targetFolderList = new List<TargetFolderSettingItem> ();


                // Check if the Properties.Settings.LastTargetFolderCollection setting exists, if not, create it.
                if (Properties.Settings.Default.LastTargetFolderCollection == null)
                {
                    Properties.Settings.Default.LastTargetFolderCollection = new System.Collections.Specialized.StringCollection ();
                }

                // Clear the UI collection.
                TargetFolderList.Clear ();

                // Transfer the Properties.Settings.Default.LastTargetFolderCollection entries into the targetFolderList for linq operations.
                foreach (var targetFolderItemString in Properties.Settings.Default.LastTargetFolderCollection)
                {
                    targetFolderList.Add (TargetFolderSettingItem.Parse (targetFolderItemString));
                }

                // Order and sort the list.
                // Update the UI.
                //var querryList = targetFolderList.Where (x => Directory.Exists (x.Path)).OrderByDescending (x => x.Date);
                // Nur Einträge, die neuer als 30 Tage sind
                var grenze = DateTime.UtcNow.AddDays (-30).ToFileTimeUtc ();
                var querryList = targetFolderList
                    .Where (x => Directory.Exists (x.Path) && (x.Date > grenze) || (x.IsPinned))
                    .OrderByDescending (x => x.Date);
                foreach (var querryItem in querryList)
                {
                    TargetFolderList.Add (new TargetFolder
                    {
                        DisplayName = querryItem.DisplayName,
                        Path = querryItem.Path,
                        AddDate = querryItem.Date,
                        IsPinned = querryItem.IsPinned,
                        Cmd_OpenFolderCommand = new RelayCommand (param => CommandExecute_MoveFiles (param as string)),
                        Cmd_RemoveFolderFromListCommand = new RelayCommand (item =>
                        {
                            if (item is TargetFolder)
                            {
                                this.TargetFolderList.Remove (item as TargetFolder);
                            }
                        }),
                    });
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine ($"Error loading target folders from settings: {ex.Message}");
            }
        }



        private void CommandExecute_MoveFiles (string targetPath, bool calledByRelayCmd = false)
        {
            var querrySelectedFileList = FileTileList.Where (x => x.IsSelected).ToList<FileTile> ();


            if (querrySelectedFileList.Count () > 0)
            {
                if (Properties.Settings.Default.ShowMoveDlg && calledByRelayCmd == false)
                {
                    this.DlgFileCount = querrySelectedFileList.Count;
                    this.DlgShow = true;
                    this.DlgTargetPath = targetPath;
                    this.DlgDisplayTargetPath = Path.GetFileName (targetPath);

                    return;
                }

                // Remove selectet items from FileTileList.
                foreach (var item in querrySelectedFileList)
                {
                    FileTileList.Remove (item);
                }

                Task.Run (() =>
                {
                    List<FileTile> querrySelectedFileList_Local = querrySelectedFileList;


                    var popup = new FileMoveProcPopupNotification ()
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

                                //File.Move (fileItem.FullPath, targetFile);
                                //File.Delete (fileItem.FullPath);
                                Task.Delay (2000).Wait ();  // Simulate a delay for moving files.
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

                    _Dispatcher.Invoke (() => LoadFilesTitles ());
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



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
