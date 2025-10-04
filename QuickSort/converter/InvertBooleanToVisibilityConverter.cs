using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;



namespace QuickSort.converter
{
    public class InvertBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;


            if (value is bool)
            {
                flag = (bool) value;
            }
            else if (value is bool?)
            {
                bool? flag2 = (bool?) value;


                flag = flag2.HasValue && flag2.Value;
            }

            return (!flag) ? Visibility.Visible : Visibility.Collapsed;
        }



        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException ();
        }
    }
}
