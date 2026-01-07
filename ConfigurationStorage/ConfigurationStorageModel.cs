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



using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;



namespace ConfigurationStorage
{

    static public class ConfigurationStorageModel
    {
        public const string APP_TITLE = "iCloudHelper";



        static public int ColorThemeId { get; set; } = 0;
        static public int LanguageId { get; set; } = 0;

        static public string MonitoringFilename { get; set; } = "iCloud*.zip";
        static public string MonitoringPath { get; set; } = SpecialFolders.GetDownloadPath ();
        static public string ExtractImagePath { get; set; } = Path.Combine (MonitoringPath, "iCloudHelper");

        static public bool BackupEnabled { get; set; } = true;
        static public string BackupPath { get; set; } = Path.Combine (MonitoringPath, "iCloudHelper\\Backup");
        static public bool BackupRetentionCheckEnabled { get; set; } = true;
        static public int BackupRetentionPeriod { get; set; } = 365;

        static public string LastUsedPath { get; set; } = ExtractImagePath;

        static public bool ShowMoveDlg { get; set; } = true;
        static public bool ShowImageFileName { get; set; } = true;

        static private StringCollection VirtualRootDirectoryCollection = new StringCollection ();

        static private StringCollection FavoriteTargetFolderCollection = new StringCollection ();
        static public int FavoriteTargetFolderCollectionLimit { get; set; } = 15;
        static public bool FavoriteTargetFolderCollectionAutoInsert { get; set; } = true;

        static public int FileTitleSizeLevel { get; set; } = 1;
        static public int FileTitleSortOrder { get; set; } = 0;
        static public int FileTitleImageColorGroupMode { get; set; } = 0;



        static public bool LoadConfiguration ()
        {
            try
            {
                RegistryKey keyRoot = Registry.CurrentUser.OpenSubKey ("Software", false);

                RegistryKey keyApp = keyRoot.OpenSubKey (APP_TITLE, false);
                if (keyApp == null)
                {
                    return false;
                }

                RegistryKey keyVer = keyApp.OpenSubKey ("V1.4.0", false);
                if (keyVer == null)
                {
                    return false;
                }

                ColorThemeId = int.Parse ((string) keyVer.GetValue (nameof (ColorThemeId)));
                LanguageId = int.Parse ((string) keyVer.GetValue (nameof (LanguageId)));

                MonitoringFilename = (string) keyVer.GetValue (nameof (MonitoringFilename));
                MonitoringPath = (string) keyVer.GetValue (nameof (MonitoringPath));
                ExtractImagePath = (string) keyVer.GetValue (nameof (ExtractImagePath));

                BackupEnabled = bool.Parse ((string) keyVer.GetValue (nameof (BackupEnabled)));
                BackupPath = (string) keyVer.GetValue (nameof (BackupPath));
                BackupRetentionCheckEnabled = bool.Parse ((string) keyVer.GetValue (nameof (BackupRetentionCheckEnabled)));
                BackupRetentionPeriod = int.Parse ((string) keyVer.GetValue (nameof (BackupRetentionPeriod)));
               
                LastUsedPath = (string) keyVer.GetValue (nameof (LastUsedPath));

                ShowMoveDlg = bool.Parse ((string) keyVer.GetValue (nameof (ShowMoveDlg)));
                ShowImageFileName = bool.Parse ((string) keyVer.GetValue (nameof (ShowImageFileName)));

                VirtualRootDirectoryCollection = LoadList (keyVer, nameof (VirtualRootDirectoryCollection));

                FavoriteTargetFolderCollection = LoadList (keyVer, nameof (FavoriteTargetFolderCollection));
                FavoriteTargetFolderCollectionLimit = int.Parse ((string) keyVer.GetValue (nameof (FavoriteTargetFolderCollectionLimit)));
                FavoriteTargetFolderCollectionAutoInsert = bool.Parse ((string) keyVer.GetValue (nameof (FavoriteTargetFolderCollectionAutoInsert)));

                FileTitleSizeLevel = int.Parse ((string) keyVer.GetValue (nameof (FileTitleSizeLevel)));
                FileTitleSortOrder = int.Parse ((string) keyVer.GetValue (nameof (FileTitleSortOrder)));
                FileTitleImageColorGroupMode = int.Parse ((string) keyVer.GetValue (nameof (FileTitleImageColorGroupMode)));


                keyVer?.Close ();
                keyApp?.Close ();
                keyRoot?.Close ();

                return true;
            }
            catch
            {
                return false;
            }
        }



        static public bool SaveConfiguration ()
        {
            try
            {
                RegistryKey keyRoot = Registry.CurrentUser.OpenSubKey ("Software", true);

                keyRoot.CreateSubKey (APP_TITLE, true);
                RegistryKey keyApp = keyRoot.OpenSubKey (APP_TITLE, true);

                keyApp.CreateSubKey ("V1.4.0", true);
                RegistryKey keyVer = keyApp.OpenSubKey ("V1.4.0", true);


                keyVer.SetValue (nameof (ColorThemeId), ColorThemeId, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (LanguageId), LanguageId, RegistryValueKind.ExpandString);

                keyVer.SetValue (nameof (MonitoringFilename), MonitoringFilename, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (MonitoringPath), MonitoringPath, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (ExtractImagePath), ExtractImagePath, RegistryValueKind.ExpandString);

                keyVer.SetValue (nameof (BackupEnabled), BackupEnabled, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (BackupPath), BackupPath, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (BackupRetentionCheckEnabled), BackupRetentionCheckEnabled, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (BackupRetentionPeriod), BackupRetentionPeriod, RegistryValueKind.ExpandString);

                keyVer.SetValue (nameof (LastUsedPath), LastUsedPath, RegistryValueKind.ExpandString);

                keyVer.SetValue (nameof (ShowMoveDlg), ShowMoveDlg, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (ShowImageFileName), ShowImageFileName, RegistryValueKind.ExpandString);

                SaveList (keyVer, nameof (VirtualRootDirectoryCollection), VirtualRootDirectoryCollection);

                SaveList (keyVer, nameof (FavoriteTargetFolderCollection), FavoriteTargetFolderCollection);
                keyVer.SetValue (nameof (FavoriteTargetFolderCollectionLimit), FavoriteTargetFolderCollectionLimit, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (FavoriteTargetFolderCollectionAutoInsert), FavoriteTargetFolderCollectionAutoInsert, RegistryValueKind.ExpandString);

                keyVer.SetValue (nameof (FileTitleSizeLevel), FileTitleSizeLevel, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (FileTitleSortOrder), FileTitleSortOrder, RegistryValueKind.ExpandString);
                keyVer.SetValue (nameof (FileTitleImageColorGroupMode), FileTitleImageColorGroupMode, RegistryValueKind.ExpandString);


                keyVer?.Close ();
                keyApp?.Close ();
                keyRoot?.Close ();

                return true;
            }
            catch
            {
                return false;
            }
        }



        static private StringCollection LoadList (RegistryKey key, string listname)
        {
            StringCollection result = new StringCollection ();
            RegistryKey listkey = key.OpenSubKey (listname);


            if (listkey != null)
            {
                for (int i = 0; i < listkey.ValueCount; i++)
                {
                    result.Add ((string) listkey.GetValue (i.ToString (), ""));
                }

            }

            listkey?.Close ();

            return result;
        }



        static private void SaveList (RegistryKey key, string listname, StringCollection list)
        {
            StringCollection result = new StringCollection ();
            RegistryKey listkey = null;


            // Delete reg keyVer with all content to get clean start conditions.
            if (key.OpenSubKey (listname) != null)
            {
                key.DeleteSubKeyTree (listname);
            }

            // Build an empty key
            key.CreateSubKey (listname);

            // Open the keyVer to till it up with values.
            listkey = key.OpenSubKey (listname, true);

            if (listkey != null)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    listkey.SetValue (i.ToString (), list[i]);
                }
            }

            listkey?.Close ();
        }



        #region VirtualDirectory items



        static public void VirtualDirectory_ClearStorage ()
        {
            VirtualRootDirectoryCollection.Clear ();
        }



        static public void VirtualDirectory_AddItemToStorage (string path, string displayName)
        {
            var item = new VirtualDirectorySettingItem (path, displayName);


            VirtualRootDirectoryCollection.Add (item.ToString ());
        }



        static public int VirtualDirectory_CountStorageItems ()
        {
            return VirtualRootDirectoryCollection.Count;
        }



        static public bool VirtualDirectory_GetStorageItem (int index, out string path, out string displayName)
        {
            path = string.Empty;
            displayName = string.Empty;


            if (index < VirtualRootDirectoryCollection.Count)
            {
                var item = VirtualDirectorySettingItem.Parse (VirtualRootDirectoryCollection[index]);


                path = item.Path;
                displayName = item.DisplayName;

                return true;
            }

            return false;
        }



        #endregion



        #region FavoriteTargetFolder items



        static public void FavoriteTargetFolder_ClearStorage ()
        {
            FavoriteTargetFolderCollection.Clear ();
        }



        static public void FavoriteTargetFolder_AddItemToStorage (string path, long date, string displayName, bool isPinned)
        {
            var item = new FavoriteTargetFolderSettingItem (path, date, displayName, isPinned);


            FavoriteTargetFolderCollection.Add (item.ToString ());
        }



        static public int FavoriteTargetFolder_CountStorageItems ()
        {
            return FavoriteTargetFolderCollection.Count;
        }



        static public bool FavoriteTargetFolder_GetStorageItem (int index, out string path, out long date, out string displayName, out bool isPinned)
        {
            path = string.Empty;
            date = 0;
            displayName = string.Empty;
            isPinned = false;


            if (index < FavoriteTargetFolderCollection.Count)
            {
                var item = FavoriteTargetFolderSettingItem.Parse (FavoriteTargetFolderCollection[index]);


                path = item.Path;
                date = item.Date;
                displayName = item.DisplayName;
                isPinned = item.IsPinned;

                return true;
            }

            return false;
        }



        #endregion
    }
}
