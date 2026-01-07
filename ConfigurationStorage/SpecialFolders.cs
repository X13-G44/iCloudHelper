/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				07.01.2026
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace ConfigurationStorage
{
    public static class SpecialFolders
    {
        public static class KnownFolder
        {
            public static readonly Guid Downloads = new Guid ("374DE290-123F-4565-9164-39C4925E467B");
            public static readonly Guid Contacts = new Guid ("56784854-C6CB-462B-8169-88E350ACB882");
            public static readonly Guid Favorites = new Guid ("1777F761-68AD-4D8A-87BD-30B759FA33DD");
            public static readonly Guid Links = new Guid ("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968");
            public static readonly Guid SavedGames = new Guid ("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4");
            public static readonly Guid SavedSearches = new Guid ("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA");
        }



        // https://stackoverflow.com/questions/7672774/how-do-i-determine-the-windows-download-folder-path
        static public string GetDownloadPath ()
        {
            string downloads;


            SHGetKnownFolderPath (KnownFolder.Downloads, 0, IntPtr.Zero, out downloads);

            return downloads;
        }



        static public string GetPicturePath ()
        {
            return Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
        }



        [DllImport ("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath ([MarshalAs (UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);
    }
}
