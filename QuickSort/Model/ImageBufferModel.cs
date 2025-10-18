/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
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



using QuickSort.Help;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace QuickSort.Model
{
    /// <summary>
    /// Class is used to load image files from a directory into our ram (heap).
    /// 
    /// The special thing here is, that the image file will be closed after the file content 
    /// was read (WPF default behavior is to let the file handle open -> blocked).
    /// </summary>
    static public class ImageFileBufferModel
    {
        static private readonly List<ImageFileBufferItem> _BufferList;

        public static List<ImageFileBufferItem> Buffer { get { return _BufferList; } }



        static ImageFileBufferModel ()
        {
            _BufferList = new List<ImageFileBufferItem> ();
        }



        /// <summary>
        /// Scan and load image files in a given directory. 
        /// The image file will only be open, while reading the data content; then the file will be closed (sit is not the WPF default behavior 😉 ).
        /// The load image data are buffed in this class for later use.
        /// </summary>
        /// <param name="path">Directory to search and load the images files.</param>
        /// <param name="forceFullLoad">Set to true to clear the full buffer and read all image files in path otherwise the buffer will only be updated with new or changed images.</param>
        /// <param name="onImageLoad">Optional callback handler, fired on each load image file.</param>
        /// <param name="onAllImagesLoad">Optional callback handler, fired at operation end (all files in directory processed).</param>
        /// <returns></returns>
        static public Task RefreshBufferAsync (string path, bool forceFullLoad, Action<ImageFileBufferItem, int, int> onImageLoad, Action<bool, List<string>> onAllImagesLoad)
        {
            return Task.Run (() =>
            {
                bool errorOccured = false;
                List<string> errorMessages = new List<string> ();


                try
                {
                    var files = Directory.GetFiles (path);
                    int fileCnt = 1;


                    if (forceFullLoad)
                    {
                        _BufferList.Clear ();
                    }

                    // Load all images from the directory into our buffer list or update them.
                    foreach (var file in files)
                    {
                        try
                        {
                            String fileExt = Path.GetExtension (file).ToLower ();
                            bool loadImageMustBeExecute = false;
                            ImageFileBufferItem bufferItemToUpdate = null;

                            ImageFileBufferItem newBufferItem = new ImageFileBufferItem () { File = file, Thumbnail = null };


                            // Check if the image is already in the buffer list and or we have to load the (new) image.

                            START_IMAGE_IN_BUFFER_CHECK:

                            var matchingImageBufferList = _BufferList.Where (x => x.File == newBufferItem.File).ToList ();
                            if (matchingImageBufferList.Count == 0)
                            {
                                loadImageMustBeExecute = true;
                            }
                            else if (matchingImageBufferList.Count == 1)
                            {
                                // Image is in our buffer list, but has the file content been changed?
                                if (newBufferItem.IsEqualWith (matchingImageBufferList[0]) == false)
                                {
                                    loadImageMustBeExecute = true;
                                    bufferItemToUpdate = matchingImageBufferList[0];
                                }
                            }
                            else
                            {
                                // There must be only one image in the buffer list.
                                // If there are more images (it is an error), remove all of them and reload the image.
                                matchingImageBufferList.ForEach (x => _BufferList.Remove (x));

                                goto START_IMAGE_IN_BUFFER_CHECK;
                            }

                            if (loadImageMustBeExecute)
                            {
                                if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png" || fileExt == ".bmp" || fileExt == ".heic")
                                {
                                    // This approach locks the file until the application is closed.
                                    //thumb = new BitmapImage (new Uri (file));

                                    BitmapImage bi = null;


                                    using (var fstream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read))
                                    {
                                        bi = new BitmapImage ();
                                        bi.BeginInit ();
                                        bi.CacheOption = BitmapCacheOption.OnLoad;
                                        bi.StreamSource = fstream;
                                        bi.StreamSource.Flush ();
                                        bi.EndInit ();
                                        bi.Freeze ();

                                        bi.StreamSource.Dispose ();
                                    }

                                    newBufferItem.Thumbnail = bi;
                                    newBufferItem.IsSysIconImage = false;
                                }
                                else
                                {
                                    // Get windows default icon.
                                    ImageSource imageSource = IconHelper.GetFileIcon (file);
                                    newBufferItem.Thumbnail = IconHelper.ConvertImageSourceToBitmapImage (imageSource);
                                    newBufferItem.IsSysIconImage = true;
                                }
                            }

                            if (bufferItemToUpdate == null)
                            {
                                if (loadImageMustBeExecute)
                                {
                                    // Add new image item to buffer list.
                                    _BufferList.Add (newBufferItem);
                                }
                            }
                            else
                            {
                                // Update existing item in buffer.
                                bufferItemToUpdate = newBufferItem;
                            }

                            // Execute / fire the onImageLoad callback handler.
                            onImageLoad?.Invoke (newBufferItem, fileCnt++, files.Length);
                        }
                        catch (Exception ex)
                        {
                            errorOccured = true;
                            errorMessages.Add ($"{file} -> {ex.Message}");
                        }
                    }

                    // Remove none existing files from the buffer list.
                    _BufferList.RemoveAll (x => !x.FileExists);

                    // Execute / fire the onAllImageLoad callback handler.
                    onAllImagesLoad?.Invoke (errorOccured, errorMessages);
                }
                catch (Exception ex)
                {
                    errorOccured = true;
                    errorMessages.Add (ex.Message);

                    // Execute / fire the onAllImageLoad callback handler.
                    onAllImagesLoad?.Invoke (errorOccured, errorMessages);
                }
            });
        }
    }



    public class ImageFileBufferItem
    {
        public string File { get; set; }
        public BitmapImage Thumbnail { get; set; }
        public bool IsSysIconImage { get; set; }

        public bool FileExists
        {
            get
            {
                try
                {
                    return System.IO.File.Exists (this.File);
                }
                catch
                {
                    return false;
                }
            }
        }



        public bool IsEqualWith (ImageFileBufferItem imageFile)
        {
            try
            {
                using (var sha256 = SHA256.Create ())
                using (var stream1 = System.IO.File.OpenRead (this.File))
                using (var stream2 = System.IO.File.OpenRead (imageFile.File))
                {
                    var hash1 = sha256.ComputeHash (stream1);
                    var hash2 = sha256.ComputeHash (stream2);


                    return StructuralComparisons.StructuralEqualityComparer.Equals (hash1, hash2);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
