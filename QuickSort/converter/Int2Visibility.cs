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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.Converter
{
    public class Int2Visibility : IValueConverter
    {
        /// <summary>
        /// Converter from integer (count) to GridLength.
        /// 
        /// Note: A parameter string must be defined and have the format "X;Y":
        ///     X = Visibility Mode if count == 0
        ///     Y = Visibility Mode if count != 0
        ///     
        /// Visibility Mode can be "visible", "hidden" or "collapsed"
        /// 
        /// </summary>
        /// <param name="value">Integer value</param>
        /// <param name="targetType">Don't care</param>
        /// <param name="parameter">Parameter string must be have the format "X;Y".
        /// X = Visibility Mode if count == 0
        /// Y = Visibility Mode if count != 0
        /// Visibility Mode can be "visible", "hidden" or "collapsed"</param>
        /// <param name="culture">Don't care</param>
        /// <returns>Visibility</returns>
        /// <exception cref="ArgumentException"></exception>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            String[] parameters;
            Visibility x;
            Visibility y;


            if (value is int)
            {
                count = (int) value;
            }

            if (parameter == null || !(parameter is String))
            {
                throw new ArgumentException ("parameter must be a parameter string");
            }

            parameters = (parameter as string).Split (';');
            if (parameters.Length != 2)
            {
                throw new ArgumentException ("parameter string must have the format \"X;Y\" where X and Y are valid GridLength strings");
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

            return count == 0 ? x : y;
        }



        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }
}
