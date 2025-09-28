using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;



namespace QuickSort.ui
{
    public class VirtualDirectoryButtonUi : System.Windows.Controls.Button
    {
        public bool IsSelected
        {
            get { return (bool) GetValue (IsSelectedPropertyProperty); }
            set { SetValue (IsSelectedPropertyProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedPropertyProperty =
            DependencyProperty.Register ("IsSelected", typeof (bool), typeof (VirtualDirectoryButtonUi), new PropertyMetadata (false));

    }
}
