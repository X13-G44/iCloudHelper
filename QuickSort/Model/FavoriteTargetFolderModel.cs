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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace QuickSort.Model
{
    public static class FavoriteTargetFolderModel
    {
        static private List<FavoriteTargetFolderSettingItem> _Storage;



        static FavoriteTargetFolderModel ()
        {
            _Storage = new List<FavoriteTargetFolderSettingItem> ();
        }



        static public void LoadStorage (bool filterAndSortEn)
        {
            // Check if the Properties.Settings.VirtualRootDirectoryCollection setting exists, if not, create it.
            if (Properties.Settings.Default.FavoriteTargetFolderCollection == null)
            {
                Properties.Settings.Default.FavoriteTargetFolderCollection = new System.Collections.Specialized.StringCollection ();
            }
            else
            {
                _Storage.Clear ();
            }

            // Transfer the Properties.Settings.Default.VirtualRootDirectoryCollection entries into the virtRootDirList for linq operations.
            foreach (var favTargetDirItemString in Properties.Settings.Default.FavoriteTargetFolderCollection)
            {
                _Storage.Add (FavoriteTargetFolderSettingItem.Parse (favTargetDirItemString));
            }

            if (filterAndSortEn)
            {
                // Order and sort the list.
                // Only entries that are younger / newer then 30 days or pinned entries are shown.

                long dayLimitThreshold = DateTime.UtcNow.AddDays (-30).ToFileTimeUtc ();


                _Storage = _Storage
                    .Where (x => Directory.Exists (x.Path) && (x.Date > dayLimitThreshold) || (x.IsPinned))
                    .OrderByDescending (x => x.Date).ToList ();
            }
        }


        static public void SaveStorage (bool callAppSettingSave = false)
        {
            // Check if the Properties.Settings.VirtualRootDirectoryCollection setting exists, if not, create it.
            if (Properties.Settings.Default.FavoriteTargetFolderCollection == null)
            {
                Properties.Settings.Default.FavoriteTargetFolderCollection = new System.Collections.Specialized.StringCollection ();
            }
            else
            {
                Properties.Settings.Default.FavoriteTargetFolderCollection.Clear ();
            }

            foreach (var favTargetDirItemString in _Storage)
            {
                var item = new FavoriteTargetFolderSettingItem (favTargetDirItemString.Path, favTargetDirItemString.Date, favTargetDirItemString.DisplayName, favTargetDirItemString.IsPinned);

                Properties.Settings.Default.FavoriteTargetFolderCollection.Add (item.ToString ());
            }

            if (callAppSettingSave)
            {
                Properties.Settings.Default.Save ();
            }
        }



        static public void ClearStorage ()
        {
            _Storage.Clear ();
        }



        static public void AddItemToStorage (string path, long date, string displayName, bool isPinned)
        {
            var item = new FavoriteTargetFolderSettingItem (path, date, displayName, isPinned);


            _Storage.Add (item);
        }



        static public int CountStorageItems ()
        {
            return _Storage.Count ();
        }



        static public bool GetStorageItem (int index, out string path, out long date, out string displayName, out bool isPinned)
        {
            path = string.Empty;
            date = 0;
            displayName = string.Empty;
            isPinned = false;


            if (index < _Storage.Count)
            {
                path = _Storage[index].Path;
                date = _Storage[index].Date;
                displayName = _Storage[index].DisplayName;
                isPinned = _Storage[index].IsPinned;

                return true;
            }

            return false;
        }
    }



    /// <summary>
    /// Help class to convert "Favorite Target Folder Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    internal class FavoriteTargetFolderSettingItem
    {
        public string Path { get; set; } = "";
        public long Date { get; set; } = 0;
        public string DisplayName { get; set; } = "";
        public bool IsPinned { get; set; } = false;



        public FavoriteTargetFolderSettingItem ()
        {
            ;
        }



        public FavoriteTargetFolderSettingItem (string path, long date, string displayName, bool isPinned)
        {
            this.Path = path;
            this.Date = date;
            this.DisplayName = displayName;
            this.IsPinned = isPinned;
        }



        public override string ToString ()
        {
            return $"{Path}?{Date}?{DisplayName}?{IsPinned}";
        }



        public static FavoriteTargetFolderSettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 4)
                {
                    return new FavoriteTargetFolderSettingItem ()
                    {
                        Path = elements[0],
                        Date = long.Parse (elements[1]),
                        DisplayName = elements[2],
                        IsPinned = bool.Parse (elements[3]),
                    };
                }

                throw new FormatException ("Invalid format for FavoriteTargetFolderSettingItem. Expected format: Path?Date?DisplayName?IsPinned");
            }
            catch
            {
                throw;
            }
        }
    }
}
