using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.converter
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
