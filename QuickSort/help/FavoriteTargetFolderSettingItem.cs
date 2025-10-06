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



using QuickSort.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace QuickSort.help
{
    /// <summary>
    /// Help class to convert "Favorite Target Folder Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    public class FavoriteTargetFolderSettingItem
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



        public FavoriteTargetFolderSettingItem (FavoriteTargetFolderModel model)
        {
            this.Path = model.Path;
            this.Date = model.AddDate;
            this.DisplayName = model.DisplayName;
            this.IsPinned = model.IsPinned;
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
                        IsPinned = bool.Parse(elements[3]),
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
