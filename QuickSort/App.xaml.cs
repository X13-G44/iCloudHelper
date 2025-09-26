using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;



namespace QuickSort
{
    public partial class App : Application
    {
        public const string APP_TITLE = "iCloudHelper";



        public string StartPfad { get; private set; }



        public App ()
        {
            ;
        }



        protected override void OnStartup (StartupEventArgs e)
        {
            string startPfad = e.Args.Length > 0 ? e.Args[0] : null;


            base.OnStartup (e);

            if (string.IsNullOrEmpty (startPfad))
            {
                startPfad = QuickSort.Properties.Settings.Default.StartPath;
            }

            if (string.IsNullOrEmpty (startPfad) || Directory.Exists(startPfad) == false)
            {
                startPfad = Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
            }

            this.StartPfad = startPfad;
        }



        protected override void OnExit (ExitEventArgs e)
        {
            QuickSort.Properties.Settings.Default.Save ();

            base.OnExit (e);
        }

    }
}
