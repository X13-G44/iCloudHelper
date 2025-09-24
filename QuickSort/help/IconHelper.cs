using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace QuickSort.help
{
    public static class IconHelper
    {
        [DllImport ("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo (
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags);



        [DllImport ("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon (IntPtr hIcon);



        [StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs (UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }



        public const uint SHGFI_ICON = 0x100;
        public const uint SHGFI_LARGEICON = 0x0; // 32x32
        public const uint SHGFI_SMALLICON = 0x1; // 16x16

        public static ImageSource GetFileIcon (string filePath, bool largeIcon = true)
        {
            SHFILEINFO shinfo = new SHFILEINFO ();
            uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);



            SHGetFileInfo (filePath, 0, ref shinfo, (uint)Marshal.SizeOf (shinfo), flags);

            if (shinfo.hIcon == IntPtr.Zero)
            {
                return null;
            }

            ImageSource img = Imaging.CreateBitmapSourceFromHIcon (
                shinfo.hIcon,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions ());

            DestroyIcon (shinfo.hIcon);

            return img;
        }
    }
}
