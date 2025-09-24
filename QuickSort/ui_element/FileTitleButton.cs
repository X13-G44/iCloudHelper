using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;



namespace QuickSort.ui_element
{
    public class FileTitleButton : System.Windows.Controls.Button
    {
        public string DisplayName
        {
            get { return (string)GetValue (DisplayNamePropertyProperty); }
            set { SetValue (DisplayNamePropertyProperty, value); }
        }

        public static readonly DependencyProperty DisplayNamePropertyProperty =
            DependencyProperty.Register ("DisplayName", typeof (string), typeof (FileTitleButton), new PropertyMetadata (string.Empty));



        public ImageSource Thumbnail
        {
            get { return (ImageSource)GetValue (ThumbnailPropertyProperty); }
            set { SetValue (ThumbnailPropertyProperty, value); }
        }

        public static readonly DependencyProperty ThumbnailPropertyProperty =
            DependencyProperty.Register ("Thumbnail", typeof (ImageSource), typeof (FileTitleButton), new PropertyMetadata (null));



        public bool IsSelected
        {
            get { return (bool)GetValue (IsSelectedPropertyProperty); }
            set { SetValue (IsSelectedPropertyProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedPropertyProperty =
            DependencyProperty.Register ("IsSelected", typeof (bool), typeof (FileTitleButton), new PropertyMetadata (false));



        public bool HideFilenameText
        {
            get { return (bool)GetValue (HideFilenameTextProperty); }
            set { SetValue (HideFilenameTextProperty, value); }
        }

        public static readonly DependencyProperty HideFilenameTextProperty =
            DependencyProperty.Register ("HideFilenameText", typeof (bool), typeof (FileTitleButton), new PropertyMetadata (false));



        public int SizeLevel
        {
            get { return (int)GetValue (SizeLevelProperty); }
            set { SetValue (SizeLevelProperty, value); }
        }
       
        public static readonly DependencyProperty SizeLevelProperty =
            DependencyProperty.Register ("SizeLevel", typeof (int), typeof (FileTitleButton), new PropertyMetadata (1));
    }
}
