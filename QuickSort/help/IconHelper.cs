/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				06.10.2025
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2025 Christian Harscher (alias X13-G44)
///
/// This program is free software: you can redistribute it and/or modify
/// it under the terms of the Apache License as
/// published by the Free Software Foundation, either version 2 of the
/// License, or (at your option) any later version.
///
/// This program is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
/// Apache License for more details.
///
/// You should have received a copy of the Apache License
/// along with this program. If not, see <http://www.apache.org/licenses/LICENSE-2.0/>.
///      
/// ////////////////////////////////////////////////////////////////////////



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
            // KI / AI

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
