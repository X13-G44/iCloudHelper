using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using QuickSort.viewmodel;



namespace QuickSort.model
{
    public class FileMoveProcPopupNotificationModel : INotifyPropertyChanged
    {
        private string _TargetPath = "";
        public string TargetPath
        {
            get { return _TargetPath; }
            set { _TargetPath = value; OnPropertyChanged (nameof (TargetPath)); }
        }

        private string _CurrentFileName = "";
        public string CurrentFileName
        {
            get { return _CurrentFileName; }
            set { _CurrentFileName = value; OnPropertyChanged (nameof (CurrentFileName)); }
        }

        private int _FileCount = 0;
        public int FileCount
        {
            get { return _FileCount; }
            set { _FileCount = value; OnPropertyChanged (nameof (FileCount)); }
        }

        private int _FileProcessed = 0;
        public int FileProcessed
        {
            get { return _FileProcessed; }
            set { _FileProcessed = value; OnPropertyChanged (nameof (FileProcessed)); }
        }

        private bool _IsFadingOut;
        public bool IsFadingOut
        {
            get => _IsFadingOut;
            set
            {
                if (_IsFadingOut != value)
                {
                    _IsFadingOut = value; OnPropertyChanged (nameof (IsFadingOut));
                }
            }
        }



        public RelayCommand Cmd_ClosePopup { get; set; }  // Property will be setup during creating a new instance of this object in function "MoveFiles".



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
