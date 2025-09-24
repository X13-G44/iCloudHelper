using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using QuickSort.viewmodel;



namespace QuickSort.model
{
    public class TargetFolder : INotifyPropertyChanged
    {
        private string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set { _DisplayName = value; OnPropertyChanged (nameof (DisplayName)); }
        }

        private string _Path;
        public string Path
        {
            get { return _Path; }
            set { _Path = value; OnPropertyChanged (nameof (Path)); }
        }

        private long _AddDate;
        public long AddDate
        {
            get { return _AddDate; }
            set { _AddDate = value; OnPropertyChanged (nameof (AddDate)); }
        }

        public string ToolTip
        {
            get { return $"Name: \t{this.DisplayName}\nOrdner: \t{this.Path}"; }
        }

        private bool _IsPinned;

        public bool IsPinned
        {
            get { return _IsPinned; }
            set { _IsPinned = value; OnPropertyChanged (nameof (IsPinned)); }
        }



        public RelayCommand Cmd_SetTargetFolderAsFavorite
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.IsPinned = true;
                    },
                    _ =>
                    {
                        return !this.IsPinned;
                    }
                );
            }
        }

        public RelayCommand Cmd_ClearTargetFolderFavorite
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        this.IsPinned = false;
                    },
                    _ =>
                    {
                        return this.IsPinned;
                    }
                );
            }
        }

        public ICommand Cmd_OpenFolderCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_AddNewTargetFolder".

        public ICommand Cmd_RemoveFolderFromListCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_AddNewTargetFolder".



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
