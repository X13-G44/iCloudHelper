using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;



namespace QuickSort.model
{
    public class VirtualDirectoryModel : INotifyPropertyChanged
    {
        private string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; OnPropertyChanged (nameof (DisplayName)); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged (nameof (IsSelected)); }
        }

        public string ToolTip
        {
            get
            {
                try
                {
                    return $"Name:        \t{this.DisplayName}\n" +
                           $"Ordner:      \t{this.Path}\n" +
                           $"Dateien:     \t{System.IO.Directory.GetFiles (this.Path).Length}\n" +
                           $"Unter-Ordner:\t{System.IO.Directory.GetDirectories (this.Path).Length}";
                }
                catch
                {
                    return $"Name:        \t{this.DisplayName}\n" +
                           $"Ordner:      \t{this.Path}\n" +
                           $"Dateien:     \t?\n" +
                           $"Unter-Ordner:\t?";
                }
            }
        }

        public bool HasSubDirectories
        {
            get
            {
                try
                {
                    return System.IO.Directory.GetDirectories (this.Path).Length != 0;
                }
                catch
                {
                    return false;
                }
            }
        }

        private string _Path;
        public string Path
        {
            get { return _Path; }
            set { _Path = value; OnPropertyChanged (nameof (Path)); OnPropertyChanged (nameof (ToolTip)); OnPropertyChanged (nameof (HasSubDirectories)); }
        }



        public ICommand Cmd_AddToListCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.
        public ICommand Cmd_RemoveItemFromListCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.
        public ICommand Cmd_MoveImagesCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.
        public ICommand Cmd_ShowSubDirsCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
