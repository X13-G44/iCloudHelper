using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;



namespace QuickSort.ui
{
    public class VirtualRootDirectoryButtonUi : System.Windows.Controls.Button
    {
        public string DisplayName
        {
            get { return (string) GetValue (DisplayNamePropertyProperty); }
            set { SetValue (DisplayNamePropertyProperty, value); }
        }

        public static readonly DependencyProperty DisplayNamePropertyProperty =
            DependencyProperty.Register ("DisplayName", typeof (string), typeof (VirtualRootDirectoryButtonUi), new PropertyMetadata (string.Empty));



        public bool IsSelected
        {
            get { return (bool) GetValue (IsSelectedPropertyProperty); }
            set { SetValue (IsSelectedPropertyProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedPropertyProperty =
            DependencyProperty.Register ("IsSelected", typeof (bool), typeof (VirtualRootDirectoryButtonUi), new PropertyMetadata (false));



        public bool HideDirectoryText
        {
            get { return (bool) GetValue (HideDirectoryTextProperty); }
            set { SetValue (HideDirectoryTextProperty, value); }
        }

        public static readonly DependencyProperty HideDirectoryTextProperty =
            DependencyProperty.Register ("HideDirectoryText", typeof (bool), typeof (VirtualRootDirectoryButtonUi), new PropertyMetadata (false));
    }
}
