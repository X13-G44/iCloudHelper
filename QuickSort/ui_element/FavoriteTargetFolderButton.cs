using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;



namespace QuickSort.ui_element
{
    public class FavoriteTargetFolderButton : System.Windows.Controls.Button
    {
        public string IsPinned
        {
            get { return (string)GetValue (IsPinnedProperty); }
            set { SetValue (IsPinnedProperty, value); }
        }

        public static readonly DependencyProperty IsPinnedProperty =
            System.Windows.DependencyProperty.Register (
                "IsPinned",
                typeof (string),
                typeof (FavoriteTargetFolderButton),
                new PropertyMetadata (string.Empty)
            );
    }
}
