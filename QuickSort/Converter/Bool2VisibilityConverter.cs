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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.Converter
{
    public class Bool2VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converter from Boolean to Visibility.
        /// 
        /// Note: A parameter string must be defined and have the format "X;Y":
        ///     X = Visibility Mode when boolean value is == FALSE
        ///     Y = Visibility Mode boolean value is == TRUE
        ///     
        /// Visibility Mode can be "visible", "hidden" or "collapsed"
        /// 
        /// </summary>
        /// <param name="value">Integer value</param>
        /// <param name="targetType">Don't care</param>
        /// <param name="parameter">Parameter string must be have the format "X;Y". See comment above!
        /// <param name="culture">Don't care</param>
        /// <returns>Visibility</returns>
        /// <exception cref="ArgumentException"></exception>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            String[] parameters;
            Visibility x;
            Visibility y;


            if (parameter == null || !(parameter is String))
            {
                throw new ArgumentException ("A parameter string must be set");
            }

            parameters = (parameter as string).Split (';');
            if (parameters.Length != 2)
            {
                throw new ArgumentException ("Parameter string must have the format \"X;Y\"");
            }

            switch (parameters[0].Trim ().ToLower ())
            {
                case "visible":
                    {
                        x = Visibility.Visible;
                        break;
                    }

                case "hidden":
                    {
                        x = Visibility.Hidden;
                        break;
                    }

                case "collapsed":
                    {
                        x = Visibility.Collapsed;
                        break;
                    }

                default:
                    {
                        throw new ArgumentException ($"Invalid visibility value: {parameters[0]}");
                    }
            }

            switch (parameters[1].Trim ().ToLower ())
            {
                case "visible":
                    {
                        y = Visibility.Visible;
                        break;
                    }

                case "hidden":
                    {
                        y = Visibility.Hidden;
                        break;
                    }

                case "collapsed":
                    {
                        y = Visibility.Collapsed;
                        break;
                    }

                default:
                    {
                        throw new ArgumentException ($"Invalid visibility value: {parameters[1]}");
                    }
            }

            return ((value is bool) && ((bool) value)) ? y : x;
        }



        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }
}
