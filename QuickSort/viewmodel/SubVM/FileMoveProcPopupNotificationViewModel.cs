/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace QuickSort.ViewModel
{
    public class FileMoveProcPopupNotificationViewModel : ViewModelBase
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

        private bool _HasFileProcessError = false;
        public bool HasFileProcessError
        {
            get { return _HasFileProcessError; }
            set { _HasFileProcessError = value; OnPropertyChanged (nameof (HasFileProcessError)); }
        }



        public RelayCommand Cmd_Abort { get; set; }  // Property will be setup during creating a new instance of this object in function "MoveFiles".
    }
}
