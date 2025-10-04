using QuickSort.viewmodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace QuickSort.model
{
    public class FileTileModel : INotifyPropertyChanged
    {
        private string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; OnPropertyChanged (nameof (DisplayName)); }
        }

        private BitmapImage _Thumbnail;
        public BitmapImage Thumbnail
        {
            get { return _Thumbnail; }
            set { _Thumbnail = value; OnPropertyChanged (nameof (Thumbnail)); }
        }

        private bool _IsSysIconImage;
        public bool IsSysIconImage
        {
            get { return _IsSysIconImage; }
            set { _IsSysIconImage = value; OnPropertyChanged (nameof(IsSysIconImage)); }
        }


        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged (nameof (IsSelected)); }
        }

        private int _Height = 100;
        public int Height
        {
            get { return _Height; }
            set { _Height = value; OnPropertyChanged (nameof (Height)); }
        }

        private int _Width = 64;
        public int Width
        {
            get { return _Width; }
            set { _Width = value; OnPropertyChanged (nameof (Width)); }
        }

        public string ToolTip
        {
            get { return $"Name: \t{this.Filename}\n" +
                         $"Ordner: \t{this.Filepath}\n" +
                         $"Größe: \t{this.Filesize}kB\n" +
                         $"Erstelldatum: \t{this.CreationTime}\n" +
                         $"Letzter Zugriff: \t{this.LastAccessTime}"; } 
        }

        private bool _HideFilenameText = false;
        public bool HideFilenameText
        {
            get { return _HideFilenameText; }
            set { _HideFilenameText = value; OnPropertyChanged (nameof (HideFilenameText)); }
        }

        private int _SizeLevel = 1;
        public int SizeLevel
        {
            get { return _SizeLevel; }
            set { _SizeLevel = value; OnPropertyChanged (nameof (SizeLevel)); }
        }

        private string _File = "";
        public string Filename
        {
            get { return System.IO.Path.GetFileName (this.File); }
        }
        public string Filepath
        {
            get { return System.IO.Path.GetFullPath(this.File); }
        }
        public string File
        {
            get { return _File; }
            set { _File = value; OnPropertyChanged (nameof (File)); OnPropertyChanged (nameof (ToolTip)); }
        }

        private long _Filesize = 0;
        public long Filesize
        {
            get { return _Filesize; }
            set { _Filesize = value; OnPropertyChanged (nameof (Filesize)); OnPropertyChanged (nameof (ToolTip)); }
        }

        private DateTime _CreationTime = new DateTime();
        public DateTime CreationTime
        {
            get { return _CreationTime; }
            set { _CreationTime = value; OnPropertyChanged (nameof (CreationTime)); OnPropertyChanged (nameof (ToolTip)); }
        }

        private DateTime _LastAccessTime = new DateTime ();
        public DateTime LastAccessTime
        {
            get { return _LastAccessTime; }
            set { _LastAccessTime = value; OnPropertyChanged (nameof (LastAccessTime)); OnPropertyChanged (nameof (ToolTip)); }
        }



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
