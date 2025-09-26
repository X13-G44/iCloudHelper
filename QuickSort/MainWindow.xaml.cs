using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuickSort.viewmodel;
using QuickSort.model;
using System.Diagnostics;



namespace QuickSort
{
    public partial class MainWindow : Window
    {
        public MainWindow ()
        {
            App app = Application.Current as App;


            InitializeComponent ();

            this.DataContext = new viewmodel.MainViewModel (Dispatcher, app.StartPfad);
            this.Closing += (s, ev) => { (this.DataContext as IDisposable)?.Dispose (); };
        }



        private void Grid_MouseLeftButtonDown (object sender, MouseButtonEventArgs e)
        {
            DragMove ();
        }
    }
}
