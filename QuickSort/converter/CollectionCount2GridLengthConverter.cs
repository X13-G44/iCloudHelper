using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.converter
{
    public class CollectionCount2GridLengthConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count = 0;


            if (value is int)
            {
                count = (int) value;
            }

            // If count == 0 => GridLength = 0* (0%)
            // If count != 0 => GridLength = 1* (100%)
            return count == 0 ? new GridLength (0, GridUnitType.Star) : new GridLength (1, GridUnitType.Star);
        }



        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }



    public class CollectionCount2GridLengthConverter2 : IValueConverter
    {
        /// <summary>
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
