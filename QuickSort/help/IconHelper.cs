using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;



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
            ////////////////////////////////////
            // KI

            //SHFILEINFO shinfo = new SHFILEINFO ();
            //uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);



            //SHGetFileInfo (filePath, 0, ref shinfo, (uint) Marshal.SizeOf (shinfo), flags);

            //if (shinfo.hIcon == IntPtr.Zero)
            //{
            //    return null;
            //}

            //ImageSource img = Imaging.CreateBitmapSourceFromHIcon (
            //    shinfo.hIcon,
            //    System.Windows.Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions ());

            //DestroyIcon (shinfo.hIcon);

            //return img;



            ////////////////////////////////////
            // https://supportcenter.devexpress.com/ticket/details/t1149600/getting-default-windows-icon-for-file-type-extension

            //SHFILEINFO shinfo = new SHFILEINFO ();
            //uint flags = SHGFI_ICON | (largeIcon ? SHGFI_LARGEICON : SHGFI_SMALLICON);
            //uint fileAttributes = 256;


            //SHGetFileInfo (
            //    filePath,
            //    fileAttributes, 
            //    ref shinfo, 
            //    (uint) Marshal.
            //    SizeOf (shinfo),
            //    flags);

            //using (var icon = System.Drawing.Icon.FromHandle (shinfo.hIcon))
            //{
            //    var result = Imaging.CreateBitmapSourceFromHIcon (
            //                    icon.Handle,
            //                    new Int32Rect (0, 0, icon.Width, icon.Height),
            //                    BitmapSizeOptions.FromEmptyOptions ());

            //    DestroyIcon (shinfo.hIcon);

            //    return result;
            //}



            //////////////////////////////////////

            // https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-extract-the-icon-associated-with-a-file-in-windows-forms
            // https://developers.de/blogs/bahro/archive/2014/05/28/how-to-extract-default-file-icon-in-wpf-application.aspx

            var sysicon = System.Drawing.Icon.ExtractAssociatedIcon (filePath);
            var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon (sysicon.Handle, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions ());


            return bmpSrc;

        }



        static public BitmapImage ConvertImageSourceToBitmapImage (ImageSource imageSource)
        {
            if (imageSource is BitmapImage bitmapImage)
            {
                return bitmapImage;
            }

            var drawingVisual = new DrawingVisual ();
            using (var drawingContext = drawingVisual.RenderOpen ())
            {
                drawingContext.DrawImage (imageSource, new Rect (0, 0, imageSource.Width, imageSource.Height));
            }

            var renderTarget = new RenderTargetBitmap (
                (int) imageSource.Width, (int) imageSource.Height, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render (drawingVisual);

            var encoder = new PngBitmapEncoder ();
            encoder.Frames.Add (BitmapFrame.Create (renderTarget));
            using (var ms = new MemoryStream ())
            {
                encoder.Save (ms);
                ms.Position = 0;
                var bmp = new BitmapImage ();
                bmp.BeginInit ();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.StreamSource = ms;
                bmp.EndInit ();
                bmp.Freeze ();
                return bmp;
            }
        }
    }
}
