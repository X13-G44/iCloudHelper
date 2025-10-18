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
    public class Int2GridLengthConverter : IValueConverter
    {
        /// <summary>
        /// Converter from integer (count) to GridLength.
        /// 
        /// Note: A parameter string must be defined and have the format "X;Y":
        ///     X = GridLength if count == 0
        ///     Y = GridLength if count != 0
        ///     
        /// GridLength can be "Auto", "100", "100*", "2*", "0.5*", etc.
        /// 
        /// </summary>
        /// <param name="value">Integer value</param>
        /// <param name="targetType">Don't care</param>
        /// <param name="parameter">Parameter string must be have the format "X;Y".
        /// X = GridLength if count == 0
        /// Y = GridLength if count != 0
        /// GridLength can be "Auto", "100", "100*", "2*", "0.5*", etc.</param>
        /// <param name="culture">Don't care</param>
        /// <returns>A GridLength value</returns>
        /// <exception cref="ArgumentException"></exception>
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;
            String[] parameters;
            GridLength x;
            GridLength y;


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

            x = (GridLength) new GridLengthConverter ().ConvertFromString (parameters[0]);
            y = (GridLength) new GridLengthConverter ().ConvertFromString (parameters[1]);


            return count == 0 ? x : y;
        }



        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }
}
