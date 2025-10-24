using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;



namespace ConfigurationStorage
{
    public static class SpecialFolders
    {
        public static class KnownFolder
        {
            public static readonly Guid Downloads = new Guid ("374DE290-123F-4565-9164-39C4925E467B");
            public static readonly Guid Contacts = new Guid ("56784854-C6CB-462B-8169-88E350ACB882");
            public static readonly Guid Favorites = new Guid ("1777F761-68AD-4D8A-87BD-30B759FA33DD");
            public static readonly Guid Links = new Guid ("BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968");
            public static readonly Guid SavedGames = new Guid ("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4");
            public static readonly Guid SavedSearches = new Guid ("7D1D3A04-DEBB-4115-95CF-2F29DA2920DA");
        }



        // https://stackoverflow.com/questions/7672774/how-do-i-determine-the-windows-download-folder-path
        static public string GetDownloadPath ()
        {
            string downloads;


            SHGetKnownFolderPath (KnownFolder.Downloads, 0, IntPtr.Zero, out downloads);

            return downloads;
        }



        static public string GetPicturePath ()
        {
            return Environment.GetFolderPath (Environment.SpecialFolder.MyPictures);
        }



        [DllImport ("shell32.dll", CharSet = CharSet.Unicode)]
        static extern int SHGetKnownFolderPath ([MarshalAs (UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out string pszPath);
    }
}
