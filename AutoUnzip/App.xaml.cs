using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.IO.Compression;
using System.Windows.Forms; // Für NotifyIcon
using System.Drawing;
using System.Reflection;    // Für Icon



namespace AutoUnzip
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private enum FileProcessingCheckpoint
        {
            [System.ComponentModel.Description ("Kein Schritt")]
            None = 0,
            [System.ComponentModel.Description ("Warte auf freien Dateihandle")]
            WaitForFreeFileHandle,
            [System.ComponentModel.Description ("Vorbereitung des temporären Ordners")]
            PrepareTempfolder,
            [System.ComponentModel.Description ("ZIP-Datei entpacken")]
            ExtractZipFile,
            [System.ComponentModel.Description ("ZIP-Datei sichern")]
            BackupZipFile,
            [System.ComponentModel.Description ("Dateien in Zielordner verschieben")]
            MoveExtractFilesToTargetfolder,
            [System.ComponentModel.Description ("Temporären Ordner löschen")]
            DeleteTempfolder,
            [System.ComponentModel.Description ("Backup-Ordner bereinigen")]
            CleanupBacklupFolder,
            [System.ComponentModel.Description ("Fertig")]
            Finished,
        }



        private FileSystemWatcher _Watcher;
        private NotifyIcon _NotifyIcon;
        private FileProcessingCheckpoint _Checkpoint = FileProcessingCheckpoint.None;



        public App ()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            Startup += App_Startup;
            Exit += App_Exit;
        }



        private void App_Exit (object sender, ExitEventArgs e)
        {
            if (_NotifyIcon != null)
            {
                _NotifyIcon.Visible = false;
            }
        }



        private void App_Startup (object sender, StartupEventArgs e)
        {
            // Start file monitoring
            Init_FileMonitoring ();

            // Start system tray icon
            Init_TrayNotifyIcon ();
        }



        private void Init_TrayNotifyIcon ()
        {
            Icon icon = SystemIcons.Information;
            var assembly = Assembly.GetExecutingAssembly ();


            using (Stream stream = assembly.GetManifestResourceStream ("AutoUnzip.Resources.icloud-logo-49272-Windows.ico"))
            {
                if (stream != null)
                {
                    icon = new Icon (stream);
                }
            }

            _NotifyIcon = new NotifyIcon ();
            _NotifyIcon.Icon = icon;
            _NotifyIcon.Visible = true;
            _NotifyIcon.Text = "AutoUnzip (Running in background)";

            // Kontextmenü für das Tray-Icon
            var contextMenu = new ContextMenu ();
            contextMenu.MenuItems.Add ("Exit application", (s, ev) =>
            {
                _NotifyIcon.Visible = false;
                Shutdown ();
            });
            _NotifyIcon.ContextMenu = contextMenu;
        }



        private void Init_FileMonitoring ()
        {
            try
            {
                // Sicherstellen, dass die Zielordner existieren
                Directory.CreateDirectory (AutoUnzip.Properties.Settings.Default.ExtractPath);
                Directory.CreateDirectory (AutoUnzip.Properties.Settings.Default.BackupPath);

                if (Directory.Exists (AutoUnzip.Properties.Settings.Default.WatchPath) == false)
                {
                    throw new DirectoryNotFoundException ("Folder \"" + AutoUnzip.Properties.Settings.Default.WatchPath + "\" not exists.");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show ("An error ocurred while creating / checking required folders.\n\nMessage: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                Shutdown ();

                return;
            }

            _Watcher = new FileSystemWatcher (AutoUnzip.Properties.Settings.Default.WatchPath, AutoUnzip.Properties.Settings.Default.FilenameToSearch)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime
            };

            _Watcher.Created += (s, ev) =>
            {
                try
                {
                    List<String> extractFiles = new List<string> ();


                    _Checkpoint = FileProcessingCheckpoint.WaitForFreeFileHandle;

                    // Warten, bis die Datei nicht mehr gesperrt ist
                    for (int i = 0; i < 10; i++)
                    {
                        try
                        {
                            using (FileStream fs = File.Open (ev.FullPath, FileMode.Open, FileAccess.Read, FileShare.None))
                            {
                                break;
                            }
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep (500);
                        }
                    }


                    _Checkpoint = FileProcessingCheckpoint.PrepareTempfolder;


                    // Temporären Ordner erstellen
                    string tempPath = Path.Combine (Path.GetTempPath (), AutoUnzip.Properties.Settings.Default.TempFolderPrefix + Path.GetRandomFileName ().Replace (".", string.Empty).Substring (0, 8));
                    if (Directory.Exists (tempPath))
                    {
                        Directory.Delete (tempPath, true);
                    }
                    Directory.CreateDirectory (tempPath);


                    _Checkpoint = FileProcessingCheckpoint.ExtractZipFile;


                    // Entpacken in temporären Ordner
                    ZipFile.ExtractToDirectory (ev.FullPath, tempPath);


                    _Checkpoint = FileProcessingCheckpoint.BackupZipFile;


                    // ZIP-Datei in Backupordner verschieben
                    if (AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod >= 0)
                    {
                        string backupZipFileName = Path.GetFileNameWithoutExtension (ev.FullPath) + "_" + DateTime.Now.ToString ("yyyyMMdd_HHmmss") + ".zip";
                        string backupZipFile = Path.Combine (AutoUnzip.Properties.Settings.Default.BackupPath, backupZipFileName);
                        if (File.Exists (backupZipFile))
                        {
                            File.Delete (backupZipFile);
                        }
                        File.Move (ev.FullPath, backupZipFile);
                    }


                    _Checkpoint = FileProcessingCheckpoint.MoveExtractFilesToTargetfolder;


                    // Entpackte Dateien vom Tempordner in den Zielordner verschieben
                    string[] fileList = Directory.GetFiles (tempPath, "*", SearchOption.AllDirectories);
                    foreach (var file in fileList)
                    {
                        string newTargetFile = Path.Combine (AutoUnzip.Properties.Settings.Default.ExtractPath, Path.GetFileName (file));


                        if (File.Exists (newTargetFile))
                        {
                            string randomSuffix = Path.GetRandomFileName ().Replace (".", string.Empty).Substring (0, 3); // 3-stelliger zufälliger Suffix
                            newTargetFile = Path.Combine (AutoUnzip.Properties.Settings.Default.ExtractPath, $"{Path.GetFileNameWithoutExtension (file)}_{randomSuffix}{Path.GetExtension (file)}");
                        }

                        File.Move (file, Path.Combine (AutoUnzip.Properties.Settings.Default.ExtractPath, Path.GetFileName (newTargetFile)));

                        extractFiles.Add (newTargetFile);
                    }


                    _Checkpoint = FileProcessingCheckpoint.DeleteTempfolder;


                    // Temporären Ordner löschen
                    Directory.Delete (tempPath, true);


                    _Checkpoint = FileProcessingCheckpoint.CleanupBacklupFolder;


                    // Lösche alle Dateien im Backup-Ordner, die älter als xx Tage sind
                    if (AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod > 0)
                    {
                        var files = Directory.GetFiles (AutoUnzip.Properties.Settings.Default.BackupPath, "*.zip", SearchOption.TopDirectoryOnly);
                        foreach (var backupFile in files)
                        {
                            try
                            {
                                if (File.GetCreationTime (backupFile) < DateTime.Now.AddDays (AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod * -1))
                                {
                                    File.Delete (backupFile);
                                }
                            }
                            catch
                            {
                                ; // Ignoriere Fehler beim Löschen von alten Backup-Dateien
                            }
                        }
                    }


                    _Checkpoint = FileProcessingCheckpoint.Finished;


                    ShowMainWindow (extractFiles);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show ($"An error occurred while processing a new \"{ev.Name}\" file in \"{AutoUnzip.Properties.Settings.Default.WatchPath}\" folder.\n" +
                        $"The error occurred at the following processing step: {GetEnumDescription (_Checkpoint)}.\n\n" +
                        $"Backup files exist in the folder \"{AutoUnzip.Properties.Settings.Default.BackupPath}\".\n\n" +
                        $"System error message is \"{ex.Message}\"",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            _Watcher.EnableRaisingEvents = true;
        }



        private void ShowMainWindow (List<String> extractedFiles)
        {
            this.Dispatcher.Invoke (() =>
            {
                if (this.MainWindow == null)
                {
                    this.MainWindow = new MainWindow ();
                }

                if (this.MainWindow.IsVisible == false)
                {
                    this.MainWindow.Show ();
                }
                else
                {
                    this.MainWindow.Activate ();
                }

                (this.MainWindow as MainWindow).ShowExtractedFiles (extractedFiles);
            });
        }



        private static string GetEnumDescription (Enum value)
        {
            var fi = value.GetType ().GetField (value.ToString ());
            var attributes = (System.ComponentModel.DescriptionAttribute[])fi.GetCustomAttributes (typeof (System.ComponentModel.DescriptionAttribute), false);


            return (attributes.Length > 0) ? attributes[0].Description : value.ToString ();
        }

    }
}
