/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				06.10.2025
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2025 Christian Harscher (alias X13-G44)
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the Apache License as
/// published by the Free Software Foundation, either version 2 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// Apache License for more details.
///
/// You should have received a copy of the Apache License
/// along with this program. If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
///      
/// ////////////////////////////////////////////////////////////////////////
 


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
    public class FavoriteTargetFolderModel : INotifyPropertyChanged
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



        public ICommand Cmd_AddFolderFromListCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddFavoriteTargetFolderItem" and "LoadTargetFolder".
        public ICommand Cmd_RemoveFolderFromListCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddFavoriteTargetFolderItem" and "LoadTargetFolder".
        public ICommand Cmd_MoveImagesCommand { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddFavoriteTargetFolderItem" and "LoadTargetFolder".



        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
