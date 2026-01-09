/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				09.01.2026
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



using QuickSort.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;



namespace QuickSort.ViewModel
{
    public class FavoriteTargetFolderViewModel : ViewModelBase
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
            get
            {
                /*
                                Name:    \t[0]\n
                                Ordner:  \t[1]

                                Name:    \t[0]\n
                                Folder:  \t[1]
                */

                return LocalizedStrings.GetFormattedString ("ttFavTargFolder_FavTargetItem", this.DisplayName, this.Path);
            }
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
        public ICommand Cmd_ChangeDisplayName { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddFavoriteTargetFolderItem" and "LoadTargetFolder".
        public ICommand Cmd_OpenDirectoryInExplorer { get; set; } // Property will be setup during creating a new instance of this object in function "Cmd_ContextMenu_AddFavoriteTargetFolderItem" and "LoadTargetFolder".
    }
}
