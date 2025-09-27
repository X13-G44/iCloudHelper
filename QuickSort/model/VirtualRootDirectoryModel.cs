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
    public class VirtualRootDirectoryModel : INotifyPropertyChanged
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
                return $"Name: \t{this.DisplayName}\n" +
                       $"Ordner: \t{this.RealPath}\n" +
                       $"Dateien: \t{System.IO.Directory.GetFiles (this.RealPath).Length}";
            }
        }

        public bool HasSubDirectories
        {
            get
            {
                return System.IO.Directory.GetDirectories (this.RealPath).Length != 0;
            }
        }

        private bool _HideFDirectoryText = false;
        public bool HideDirectoryText
        {
            get { return _HideFDirectoryText; }
            set { _HideFDirectoryText = value; OnPropertyChanged (nameof (HideDirectoryText)); }
        }

        private string _RealPath;
        public string RealPath
        {
            get { return _RealPath; }
            set { _RealPath = value; OnPropertyChanged (nameof (RealPath)); OnPropertyChanged (nameof (ToolTip)); OnPropertyChanged (nameof (HasSubDirectories)); }
        }



        public ICommand Cmd_AddToListCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddVirtualRootDirectoryItem" and "LoadTargetFolder".
        public ICommand Cmd_RemoveItemFromListCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddVirtualRootDirectoryItem" and "LoadTargetFolder".



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
