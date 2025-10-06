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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickSort.help
{
    /// <summary>
    /// Help class to convert "Virtual Root Directory Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    public class VirtualRootDirectorySettingItem
    {
        public string Path { get; set; } = "";
        public string DisplayName { get; set; } = "";



        public VirtualRootDirectorySettingItem ()
        {
            ;
        }



        public VirtualRootDirectorySettingItem (string path, string displayName)
        {
            this.Path = path;
            this.DisplayName = displayName;
        }



        public VirtualRootDirectorySettingItem (VirtualDirectoryModel model)
        {
            this.Path = model.Path;
            this.DisplayName = model.DisplayName;
        }



        public override string ToString ()
        {
            return $"{Path}?{DisplayName}";
        }



        public static VirtualRootDirectorySettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 2)
                {
                    return new VirtualRootDirectorySettingItem ()
                    {
                        Path = elements[0],
                        DisplayName = elements[1],
                    };
                }

                throw new FormatException ("Invalid format for VirtualRootDirectorySettingItem. Expected format: Path?DisplayName");
            }
            catch
            {
                throw;
            }
        }
    }
}
