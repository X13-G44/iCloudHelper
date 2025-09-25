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

        private bool _UseDarkModeColorTheme = false;

        public bool UseDarkModeColorTheme
        {
            get { return _UseDarkModeColorTheme; }
            set { _UseDarkModeColorTheme = value; OnPropertyChanged (nameof (UseDarkModeColorTheme)); }
        }



        public RelayCommand Cmd_BrowseStartFolder
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        using (var fbd = new FolderBrowserDialog ())
                        {
                            if (Directory.Exists (this.StartPath))
                            {
                                fbd.SelectedPath = this.StartPath;
                            }

                            if (fbd.ShowDialog () == System.Windows.Forms.DialogResult.OK)
                            {
                                this.StartPath = fbd.SelectedPath;

                                CheckConfig ();
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

                        QuickSort.Properties.Settings.Default.StartPath = this.StartPath;
                        QuickSort.Properties.Settings.Default.ShowMoveDlg = this.ShowMoveDlg;
                        QuickSort.Properties.Settings.Default.ColorThemeId = (uint) (this.UseDarkModeColorTheme == true ? 1 : 0);

                        QuickSort.Properties.Settings.Default.Save ();

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

            this.StartPath = QuickSort.Properties.Settings.Default.StartPath;
            this.ShowMoveDlg = QuickSort.Properties.Settings.Default.ShowMoveDlg;
            this.UseDarkModeColorTheme = QuickSort.Properties.Settings.Default.ColorThemeId == 1 ? true : false;
        }



        private bool CheckConfig ()
        {
            bool settingsOk = true;


            if (Directory.Exists (this.StartPath) != true)
            {
                settingsOk = false;
            }

            return settingsOk;
        }



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
