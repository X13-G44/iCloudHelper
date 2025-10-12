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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFLocalizeExtension.Engine;
using System.Globalization;
using System.Runtime.CompilerServices;



namespace QuickSort.Resources
{
    public class LocalizedStrings
    {
        private LocalizedStrings ()
        {
            ;
        }



        /// <summary>
        /// Set the culture for active / underlying localize dictionary instance.
        /// </summary>
        /// <param name="cultureCode"></param>
        public static void SetCulture (string cultureCode)
        {
            var newCulture = new CultureInfo (cultureCode);
            LocalizeDictionary.Instance.Culture = newCulture;
        }



        /// <summary>
        /// The localized string for the given key with formatted arguments.
        /// Note: Use [ and ] instead of { and } in the localized string to avoid issues with XAML parsing.
        /// </summary>
        /// <param name="key">Key of the string</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static string GetFormattedString (string key, params object[] args)
        {
            string format = GetString (key);


            // Replace [ and ] with { and } for string.Format
            format = format.Replace ('[', '{');
            format = format.Replace (']', '}');

            return string.Format (format, args);
        }



        /// <summary>
        /// The localized string for the given key.
        /// Alternative function to "this[key]" and property "Instance[key]".
        /// </summary>
        /// <param name="key">Key of the string</param>
        /// <returns></returns>
        public static string GetString (string key)
        {
            // Tab-character
            const char tabChar = '\u0009';

            string text = LocalizeDictionary.Instance.GetLocalizedObject ("QuickSort", "Strings", key, LocalizeDictionary.Instance.Culture) as string;

            text = text.Replace ("\\n", Environment.NewLine);
            text = text.Replace ("\\r", Environment.NewLine);
            text = text.Replace ("\\t", tabChar.ToString());

            return text;
        }
    }
}
