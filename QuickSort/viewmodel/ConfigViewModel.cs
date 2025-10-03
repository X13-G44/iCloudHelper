using QuickSort.view;
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



namespace QuickSort.viewmodel
{
    public class ConfigViewModel : INotifyPropertyChanged
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

                            dialog.Description = "Standard Startordner auswählen";

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
                        QuickSort.Properties.Settings.Default.StartPath = this.StartPath;
                        QuickSort.Properties.Settings.Default.ShowMoveDlg = this.ShowMoveDlg;
                        QuickSort.Properties.Settings.Default.ShowImageFileName = this.ShowImgFileName;
                        QuickSort.Properties.Settings.Default.ColorThemeId = (uint) (this.UseDarkModeColorTheme == true ? 1 : 0);
                        QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionLimit = this.MaxFavoriteTargetFolderCollectionItems;
                        QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionAutoInsert = this.AutoInsertFavoriteTargetFolderCollectionItems;

                        QuickSort.Properties.Settings.Default.Save ();

                        viewModel._View.DialogResult = true;
                        viewModel._View.Close ();
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

            this.StartPath = QuickSort.Properties.Settings.Default.StartPath;
            this.ShowMoveDlg = QuickSort.Properties.Settings.Default.ShowMoveDlg;
            this.ShowImgFileName = QuickSort.Properties.Settings.Default.ShowImageFileName;
            this.UseDarkModeColorTheme = QuickSort.Properties.Settings.Default.ColorThemeId == 1 ? true : false;
            this.MaxFavoriteTargetFolderCollectionItems = QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionLimit;
            this.AutoInsertFavoriteTargetFolderCollectionItems = QuickSort.Properties.Settings.Default.FavoriteTargetFolderCollectionAutoInsert;
        }



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
