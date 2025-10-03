using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



namespace AutoUnzip.view
{
    public partial class ConfigView : Window
    {
        public ConfigView ()
        {
            InitializeComponent ();

            this.DataContext = new viewmodel.ConfigViewModel (Dispatcher, this);
        }
    }
}
