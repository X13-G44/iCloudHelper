using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.converter
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
