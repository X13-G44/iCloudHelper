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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;



namespace QuickSort.Model
{

    static public class VirtualDirectoryModel
    {
        static private List<VirtualDirectorySettingItem> _Storage;



        static VirtualDirectoryModel ()
        {
            _Storage = new List<VirtualDirectorySettingItem> ();
        }



        static public void LoadStorage (bool filterEn)
        {
            // Check if the Properties.Settings.VirtualRootDirectoryCollection setting exists, if not, create it.
            if (Properties.Settings.Default.VirtualRootDirectoryCollection == null)
            {
                Properties.Settings.Default.VirtualRootDirectoryCollection = new System.Collections.Specialized.StringCollection ();
            }
            else
            {
                _Storage.Clear ();
            }

            // Transfer the Properties.Settings.Default.VirtualRootDirectoryCollection entries into the virtRootDirList for linq operations.
            foreach (var virtRootDirItemString in Properties.Settings.Default.VirtualRootDirectoryCollection)
            {
                _Storage.Add (VirtualDirectorySettingItem.Parse (virtRootDirItemString));
            }

            if (filterEn)
            {
                _Storage.RemoveAll (x => !Directory.Exists (x.Path));
            }
        }



        static public void SaveStorage (bool callAppSettingSave = false)
        {
            // Check if the Properties.Settings.VirtualRootDirectoryCollection setting exists, if not, create it.
            if (Properties.Settings.Default.VirtualRootDirectoryCollection == null)
            {
                Properties.Settings.Default.VirtualRootDirectoryCollection = new System.Collections.Specialized.StringCollection ();
            }
            else
            {
                Properties.Settings.Default.VirtualRootDirectoryCollection.Clear ();
            }

            foreach (var virtDirItem in _Storage)
            {
                var item = new VirtualDirectorySettingItem (virtDirItem.Path, virtDirItem.DisplayName);

                Properties.Settings.Default.VirtualRootDirectoryCollection.Add (item.ToString ());
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



        static public void AddItemToStorage (string path, string displayName)
        {
            var item = new VirtualDirectorySettingItem (path, displayName);


            _Storage.Add (item);
        }



        static public int CountStorageItems ()
        {
            return _Storage.Count ();
        }



        static public bool GetStorageItem (int index, out string path, out string displayName)
        {
            path = string.Empty;
            displayName = string.Empty;


            if (index < _Storage.Count)
            {
                path = _Storage[index].Path;
                displayName = _Storage[index].DisplayName;

                return true;
            }

            return false;
        }
    }



    /// <summary>
    /// Help class to convert "Virtual Root Directory Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    internal class VirtualDirectorySettingItem
    {
        public string Path { get; set; } = "";
        public string DisplayName { get; set; } = "";



        public VirtualDirectorySettingItem ()
        {
            ;
        }



        public VirtualDirectorySettingItem (string path, string displayName)
        {
            Path = path;
            DisplayName = displayName;
        }



        public override string ToString ()
        {
            return $"{Path}?{DisplayName}";
        }



        public static VirtualDirectorySettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 2)
                {
                    return new VirtualDirectorySettingItem ()
                    {
                        Path = elements[0],
                        DisplayName = elements[1],
                    };
                }

                throw new FormatException ("Invalid format for VirtualDirectorySettingItem. Expected format: Path?DisplayName");
            }
            catch
            {
                throw;
            }
        }
    }
}
