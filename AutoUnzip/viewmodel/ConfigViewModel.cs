using AutoUnzip.view;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;



namespace AutoUnzip.viewmodel
{
    public class ConfigViewModel : INotifyPropertyChanged
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

                            dialog.Description = " Ordner zum Überwachen der heruntergeladenen ZIP.Datei auswählen.";

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

                            dialog.Description = "Zielordner für die entpackte(n) Dateien auswählen.";

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

                            dialog.Description = "Zielordner für die Backup-Dateien auswählen.";

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
                            dialog.Title = $"{App.APP_TITLE} - Select QuickSort application";
                            dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
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

        public RelayCommandWithAdditionalFields Cmd_SaveConfig
        {
            get
            {
                return new RelayCommandWithAdditionalFields (
                    (param, hostInst, userParam) =>
                    {
                        AutoUnzip.Properties.Settings.Default.WatchPath = this.WatchPath;
                        AutoUnzip.Properties.Settings.Default.FilenameToSearch = this.FilenameToSearch;
                        AutoUnzip.Properties.Settings.Default.ExtractPath = this.ExtractPath;
                        AutoUnzip.Properties.Settings.Default.TempFolderPrefix = this.TempFolderPrefix;
                        AutoUnzip.Properties.Settings.Default.BackupPath = this.BackupPath;
                        AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod = this.BackupRetentionPeriod;
                        AutoUnzip.Properties.Settings.Default.QuickSortApp = this.QuickSortApp;
                        AutoUnzip.Properties.Settings.Default.ColorThemeId = (uint) (this.UseDarkModeColorTheme == true ? 1 : 0);

                        AutoUnzip.Properties.Settings.Default.Save ();

                        (hostInst as ConfigViewModel)._View.DialogResult = true;
                        (hostInst as ConfigViewModel)._View.Close ();
                    },
                    (param, hostInst, userParam) => true,
                    this,
                    null
                );
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        private Dispatcher _Dispatcher;
        private ConfigView _View;



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
            this.QuickSortApp= AutoUnzip.Properties.Settings.Default.QuickSortApp;
            this.UseDarkModeColorTheme = AutoUnzip.Properties.Settings.Default.ColorThemeId == 1 ? true : false;
        }



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
