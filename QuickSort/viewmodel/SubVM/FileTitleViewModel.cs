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



using QuickSort.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace QuickSort.ViewModel
{
    public class FileTitleViewModel : ViewModelBase
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
            set { _IsSysIconImage = value; OnPropertyChanged (nameof (IsSysIconImage)); }
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
            get
            {
                /*
                    Name:            \t[0]\n
                    Ordner:          \t[1]\n
                    Größe:           \t[2]kB\n
                    Aufnahmedatum:   \t[3]\n
                    Erstelldatum:    \t[4]

                    Name:            \t[0]\n
                    Folder:          \t[1]\n
                    Size:            \t[2]kB\n
                    Capture date:    \t[3]\n
                    Creation date:   \t[4]
                */

                return LocalizedStrings.GetFormattedString ("ttFileTitleSec_FileTitleItem", this.Filename, this.Filepath, this.Filesize, this.TakenDate, this.CreationTime);
            }
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
        public string File
        {
            get { return _File; }
            set
            {
                _File = value;
                OnPropertyChanged (nameof (File));

                OnPropertyChanged (nameof (Filename));
                OnPropertyChanged (nameof (Filepath));
                OnPropertyChanged (nameof (Filesize));
                OnPropertyChanged (nameof (CreationTime));
                OnPropertyChanged (nameof (TakenDate));

                OnPropertyChanged (nameof (ToolTip));
            }
        }
        public string Filename
        {
            get
            {
                try
                {
                    return Path.GetFileName (this.File);
                }
                catch
                {
                    return this.File;
                }
            }
        }
        public string Filepath
        {
            get
            {
                try
                {
                    return Path.GetFullPath (this.File);
                }
                catch
                {
                    return this.File;
                }
            }
        }
        public long Filesize
        {
            get
            {
                try
                {
                    return new FileInfo (this.File).Length / 1024; // Convert site from byte to kB.
                }
                catch
                {
                    return 0;
                }
            }
        }

        private DateTime _TakenDate;
        public DateTime TakenDate
        {
            get { return _TakenDate; }
            set { _TakenDate = value; OnPropertyChanged (nameof (TakenDate)); }
        }

        private DateTime _CreationTime;
        public DateTime CreationTime
        {
            get { return _CreationTime; }
            set { _CreationTime = value; OnPropertyChanged (nameof (CreationTime)); }
        }


        public bool FileExists
        {
            get
            {
                try
                {
                    return System.IO.File.Exists (this.File);
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
