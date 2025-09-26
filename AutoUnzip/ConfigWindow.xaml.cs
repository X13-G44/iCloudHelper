using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace AutoUnzip
{
    public partial class ConfigWindow : Window
    {
        private Brush _CommonForeground;
        private Brush _CommonBorderBrush;

        private string _LastUsedPath = "";



        public ConfigWindow ()
        {
            InitializeComponent ();


            _CommonForeground = tbWatchPath.Foreground;
            _CommonBorderBrush = tbWatchPath.BorderBrush;

            tbWatchPath.Text = AutoUnzip.Properties.Settings.Default.WatchPath;
            tbFilenameToSearch.Text = AutoUnzip.Properties.Settings.Default.FilenameToSearch;
            tbExtractPath.Text = AutoUnzip.Properties.Settings.Default.ExtractPath;
            tbTempFolderPrefix.Text = AutoUnzip.Properties.Settings.Default.TempFolderPrefix;
            tbBackupPath.Text = AutoUnzip.Properties.Settings.Default.BackupPath;
            tbBackupRetentionPeriod.Text = AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod.ToString ();
            tbQuickSortApp.Text = AutoUnzip.Properties.Settings.Default.QuickSortApp;
            cbColorTheme.IsChecked = AutoUnzip.Properties.Settings.Default.ColorThemeId == 1 ? true : false;

            CheckConfig ();
        }



        private void SaveButton_Click (object sender, RoutedEventArgs e)
        {
            if (this.CheckConfig () == false)
            {
                if (System.Windows.MessageBox.Show ($"Your configuration is not valid.\n\nDo you still want to save it?",
                    $"{App.APP_TITLE} - Configuration error",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }
            }

            AutoUnzip.Properties.Settings.Default.WatchPath = tbWatchPath.Text;
            AutoUnzip.Properties.Settings.Default.FilenameToSearch = tbFilenameToSearch.Text;
            AutoUnzip.Properties.Settings.Default.ExtractPath = tbExtractPath.Text;
            AutoUnzip.Properties.Settings.Default.TempFolderPrefix = tbTempFolderPrefix.Text;
            AutoUnzip.Properties.Settings.Default.BackupPath = tbBackupPath.Text;
            AutoUnzip.Properties.Settings.Default.BackupRetentionPeriod = int.Parse (tbBackupRetentionPeriod.Text);
            AutoUnzip.Properties.Settings.Default.QuickSortApp = tbQuickSortApp.Text;
            AutoUnzip.Properties.Settings.Default.ColorThemeId = (uint) (cbColorTheme.IsChecked.Value == true ? 1 : 0);

            AutoUnzip.Properties.Settings.Default.Save ();

            this.DialogResult = true;
            Close ();
        }



        private void btnBrowseWatchPath_Click (object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog ())
            {
                if (Directory.Exists (tbWatchPath.Text))
                {
                    dialog.SelectedPath = tbWatchPath.Text;
                }
                else if (Directory.Exists (_LastUsedPath))
                {
                    dialog.SelectedPath = _LastUsedPath;
                }

                dialog.Description = $"Zu überwachenden Ordner auswählen";

                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                {
                    tbWatchPath.Text = dialog.SelectedPath;

                    _LastUsedPath = dialog.SelectedPath;

                    CheckConfig ();
                }
            }
        }



        private void btnBrowseExtractPath_Click (object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog ())
            {
                if (Directory.Exists (tbExtractPath.Text))
                {
                    dialog.SelectedPath = tbExtractPath.Text;
                }
                else if (Directory.Exists (_LastUsedPath))
                {
                    dialog.SelectedPath = _LastUsedPath;
                }

                dialog.Description = $"Zielordner für entpackte Dateien auswählen";

                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                {
                    tbExtractPath.Text = dialog.SelectedPath;

                    _LastUsedPath = dialog.SelectedPath;

                    CheckConfig ();
                }
            }
        }



        private void btnBrowseBackupPath_Click (object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog ())
            {
                if (Directory.Exists (tbBackupPath.Text))
                {
                    dialog.SelectedPath = tbBackupPath.Text;
                }
                else if (Directory.Exists (_LastUsedPath))
                {
                    dialog.SelectedPath = _LastUsedPath;
                }

                dialog.Description = $"Zielordner für Backup-Dateien auswählen";

                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                {
                    tbBackupPath.Text = dialog.SelectedPath;

                    _LastUsedPath = dialog.SelectedPath;

                    CheckConfig ();
                }
            }
        }



        private void btnBrowseQuickSortApp_Click (object sender, RoutedEventArgs e)
        {
            using (var dialog = new OpenFileDialog ())
            {
                dialog.Title = $"{App.APP_TITLE} - Select QuickSort application";
                dialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                dialog.InitialDirectory = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFiles);
                dialog.CheckFileExists = true;

                if (File.Exists (tbQuickSortApp.Text))
                {
                    dialog.InitialDirectory = tbQuickSortApp.Text;
                }

                if (dialog.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                {
                    tbQuickSortApp.Text = dialog.FileName;

                    CheckConfig ();
                }
            }
        }



        private void tbXXX_LostFocus (object sender, RoutedEventArgs e)
        {
            CheckConfig ();
        }



        private bool CheckConfig ()
        {
            bool settingsOk = true;


            if (Directory.Exists (tbWatchPath.Text) != true)
            {
                tbWatchPath.Foreground = Brushes.Red;
                tbWatchPath.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbWatchPath.Foreground = _CommonForeground;
                tbWatchPath.BorderBrush = _CommonBorderBrush;
            }

            if (String.IsNullOrEmpty (tbFilenameToSearch.Text) || tbFilenameToSearch.Text.Length == 0)
            {
                tbFilenameToSearch.Foreground = Brushes.Red;
                tbFilenameToSearch.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbFilenameToSearch.Foreground = _CommonForeground;
                tbFilenameToSearch.BorderBrush = _CommonBorderBrush;
            }

            if (Directory.Exists (tbExtractPath.Text) != true)
            {
                tbExtractPath.Foreground = Brushes.Red;
                tbExtractPath.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbExtractPath.Foreground = _CommonForeground;
                tbExtractPath.BorderBrush = _CommonBorderBrush;
            }

            if (String.IsNullOrEmpty (tbTempFolderPrefix.Text) || tbTempFolderPrefix.Text.Length == 0)
            {
                tbTempFolderPrefix.Foreground = Brushes.Red;
                tbTempFolderPrefix.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbTempFolderPrefix.Foreground = _CommonForeground;
                tbTempFolderPrefix.BorderBrush = _CommonBorderBrush;
            }

            int value;
            bool isValid = int.TryParse (tbBackupRetentionPeriod.Text, out value) && value >= 0 && value <= 366;
            if (!isValid)
            {
                tbBackupRetentionPeriod.Foreground = Brushes.Red;
                tbBackupRetentionPeriod.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbBackupRetentionPeriod.Foreground = _CommonForeground;
                tbBackupRetentionPeriod.BorderBrush = _CommonBorderBrush;
            }

            if (Directory.Exists (tbBackupPath.Text) != true)
            {
                tbBackupPath.Foreground = Brushes.Red;
                tbBackupPath.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbBackupPath.Foreground = _CommonForeground;
                tbBackupPath.BorderBrush = _CommonBorderBrush;
            }

            if (File.Exists (tbQuickSortApp.Text) != true)
            {
                tbQuickSortApp.Foreground = Brushes.Red;
                tbQuickSortApp.BorderBrush = Brushes.Red;

                settingsOk = false;
            }
            else
            {
                tbQuickSortApp.Foreground = _CommonForeground;
                tbQuickSortApp.BorderBrush = _CommonBorderBrush;
            }

            return settingsOk;
        }
    }
}
