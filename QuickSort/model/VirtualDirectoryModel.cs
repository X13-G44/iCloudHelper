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
        public ICommand Cmd_CreateSubDirsCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.
        public ICommand Cmd_DeleteSubDirsCommand { get; set; } // Property will be setup during creating a new instance of this object in various functions.




        public event PropertyChangedEventHandler PropertyChanged;



        public void OnPropertyChanged (string propertyName)
        {
            PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
        }
    }
}
