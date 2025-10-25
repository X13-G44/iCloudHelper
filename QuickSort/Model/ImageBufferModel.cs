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
using QuickSort.ViewModel;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Net.WebRequestMethods;



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
        const int THRESHOLD_MULTI_TASK = 15;
        const int MAX_TASK_COUNT = 3;



        static private readonly List<ImageFileBufferItem> _BufferList;
        public static List<ImageFileBufferItem> Buffer { get { return _BufferList; } }



        static ImageFileBufferModel ()
        {
            _BufferList = new List<ImageFileBufferItem> ();
        }



        /// <summary>
        /// Scan and load image files in a given directory. 
        /// The image file will be only open, while reading the data content. After this, the file will be closed (this is not the WPF default behavior 😉 ).
        /// The load image data are buffed in this class for later use and can be accessed via property "Buffer".       
        /// </summary>
        /// <note>
        /// Depending on the count of HiRes image files (".heic"), one or multiple task will be used to red the image files.
        /// When using multiple tasks are used, the call order of callback "onImageLoad" is not linear; user must sort the images manually.
        /// </note>
        /// <param name="path">Directory to search and load the images files.</param>
        /// <param name="forceFullLoad">Set to true to clear the full buffer and read all image files in path otherwise the buffer will only be updated with new or changed images.</param>
        /// <param name="onImageLoad">Optional callback handler, fired on each load image file.</param>
        /// <param name="onAllImagesLoad">Optional callback handler, fired at operation end (all files in directory processed).</param>
        /// <returns></returns>
        static public Task RefreshBufferAsync (string path, bool forceFullLoad, Action<ImageFileBufferItem, int, int, bool> onImageLoad, Action<List<string>> onAllImagesLoad)
        {
            string[] files;
            int filesWithHiRes = 0;


            if (forceFullLoad == false)
                throw new NotImplementedException ("Feature is currently not controllable, since we always use this function with this parameter set!");

            try
            {
                // Get the count of HiRes pictures in directory.

                files = Directory.GetFiles (path);
                filesWithHiRes = files.Where (x => Path.GetExtension (x).ToLower () == ".heic").Count ();


                if (filesWithHiRes < THRESHOLD_MULTI_TASK)
                {
                    #region SIINGLE TASK VERSION


                    return Task.Run (() =>
                    {
                        List<string> errorMessages = new List<string> ();


                        try
                        {
                            int fileCnt = 1;


                            //if (forceFullLoad)
                            //{
                            _BufferList.Clear ();
                            //}

                            // Load all images from the directory into our buffer list or update them.
                            foreach (var file in files)
                            {
                                try
                                {
                                    String fileExt = Path.GetExtension (file).ToLower ();
                                    bool loadImageMustBeExecute = false;
                                    //ImageFileBufferItem bufferItemToUpdate = null;

                                    ImageFileBufferItem newBufferItem = null;


                                    // ++++++++++++++++++++++++ [START] ++++++++++++++++++++++++
                                    // ++++ Feature is currently not controllable, since we always use this function with this parameter set! ++++
                                    //
                                    //// Check if the image is already in the buffer list and or we have to load the (new) image.
                                    //
                                    //START_IMAGE_IN_BUFFER_CHECK:
                                    //
                                    //var matchingImageBufferList = _BufferList.Where (x => x.File == newBufferItem.File).ToList ();
                                    //if (matchingImageBufferList.Count == 0)
                                    //{
                                    loadImageMustBeExecute = true;
                                    //}
                                    //else if (matchingImageBufferList.Count == 1)
                                    //{
                                    //    // Image is in our buffer list, but has the file content been changed?
                                    //    if (newBufferItem.IsEqualWith (matchingImageBufferList[0]) == false)
                                    //    {
                                    //        loadImageMustBeExecute = true;
                                    //        bufferItemToUpdate = matchingImageBufferList[0];
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    // There must be only one image in the buffer list.
                                    //    // If there are more images (it is an error), remove all of them and reload the image.
                                    //    matchingImageBufferList.ForEach (x => _BufferList.Remove (x));
                                    //
                                    //    goto START_IMAGE_IN_BUFFER_CHECK;
                                    //}
                                    //
                                    // ++++++++++++++++++++++++ [END] ++++++++++++++++++++++++

                                    if (loadImageMustBeExecute)
                                    {
                                        if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png" || fileExt == ".bmp" || fileExt == ".heic")
                                        {
                                            // This approach locks the file until the application is closed.
                                            ////BitmapImage bi = new BitmapImage (new Url (file));

                                            BitmapImage bi = null;
                                            DateTime takenDate;


                                            using (var fstream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read))
                                            {
                                                bi = new BitmapImage ();
                                                bi.BeginInit ();
                                                bi.CacheOption = BitmapCacheOption.OnLoad;
                                                bi.StreamSource = fstream;
                                                bi.StreamSource.Flush ();
                                                bi.EndInit ();
                                                bi.Freeze ();

                                                takenDate = GetTakenDate (fstream, file);

                                                bi.StreamSource.Dispose ();
                                            }

                                            newBufferItem = new ImageFileBufferItem (file, bi, false, takenDate, System.IO.File.GetCreationTime (file));
                                        }
                                        else
                                        {
                                            // Get windows default icon.
                                            ImageSource imageSource = IconHelper.GetFileIcon (file);
                                            BitmapImage bi = IconHelper.ConvertImageSourceToBitmapImage (imageSource);
                                            DateTime takenDate = System.IO.File.GetCreationTime (file);


                                            newBufferItem = new ImageFileBufferItem (file, bi, true, takenDate, takenDate);
                                        }
                                    }

                                    // ++++++++++++++++++++++++ [START] ++++++++++++++++++++++++
                                    // ++++ Feature is currently not controllable, since we always use this function with this parameter set! ++++
                                    //
                                    //if (bufferItemToUpdate == null)
                                    //{
                                    //    if (loadImageMustBeExecute)
                                    //    {
                                    //      // Add new image item to buffer list.
                                    _BufferList.Add (newBufferItem);
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    // Update existing item in buffer.
                                    //    bufferItemToUpdate = newBufferItem;
                                    //}
                                    //
                                    // ++++++++++++++++++++++++ [END] ++++++++++++++++++++++++

                                    // Execute / fire the onImageLoad callback handler.
                                    onImageLoad?.Invoke (newBufferItem, fileCnt++, files.Length, false);
                                }
                                catch (Exception ex)
                                {
                                    errorMessages.Add ($"{file} -> {ex.Message}");
                                }
                            }

                            // Remove none existing files from the buffer list.
                            _BufferList.RemoveAll (x => !x.FileExists);

                            // Execute / fire the onAllImageLoad callback handler.
                            onAllImagesLoad?.Invoke (errorMessages);
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add (ex.Message);

                            // Execute / fire the onAllImageLoad callback handler.
                            onAllImagesLoad?.Invoke (errorMessages);
                        }
                    });


                    #endregion
                }
                else
                {
                    #region MULTIBLE TASK VERSION


                    return Task.Run (() =>
                    {
                        try
                        {
                            Semaphore semaphoreBufferList = new Semaphore (initialCount: MAX_TASK_COUNT - 1, maximumCount: MAX_TASK_COUNT);
                            ConcurrentQueue<string> errorMessages = new ConcurrentQueue<string> ();
                            int fileCnt = 1;


                            //if (forceFullLoad)
                            //{
                            _BufferList.Clear ();
                            //}

                            // Load all images from the directory into our buffer list or update them.
                            Parallel.ForEach (files,
                                new ParallelOptions { MaxDegreeOfParallelism = MAX_TASK_COUNT },
                                file =>
                                {
                                    try
                                    {
                                        String fileExt = Path.GetExtension (file).ToLower ();
                                        bool loadImageMustBeExecute = false;
                                        //ImageFileBufferItem bufferItemToUpdate = null;

                                        ImageFileBufferItem newBufferItem = null;



                                        // ++++++++++++++++++++++++ [START] ++++++++++++++++++++++++
                                        // ++++ Feature is currently not controllable, since we always use this function with this parameter set! ++++
                                        //
                                        ////Check if the image is already in the buffer list and or we have to load the (new) image.
                                        //  
                                        //semaphoreBufferList.WaitOne (10000);
                                        //
                                        //START_IMAGE_IN_BUFFER_CHECK:
                                        //
                                        //var matchingImageBufferList = _BufferList.Where (x => x.File == newBufferItem.File).ToList ();
                                        //if (matchingImageBufferList.Count == 0)
                                        //{
                                        loadImageMustBeExecute = true;
                                        //}
                                        //else if (matchingImageBufferList.Count == 1)
                                        //{
                                        //    // Image is in our buffer list, but has the file content changed?
                                        //    if (newBufferItem.IsEqualWith (matchingImageBufferList[0]) == false)
                                        //    {
                                        //        loadImageMustBeExecute = true;
                                        //        bufferItemToUpdate = matchingImageBufferList[0];
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    // There must be only one image in the buffer list.
                                        //    // If there are more images (it is an error), remove all of them and reload the image.
                                        //    matchingImageBufferList.ForEach (x => _BufferList.Remove (x));
                                        //
                                        //    goto START_IMAGE_IN_BUFFER_CHECK;
                                        //}
                                        //
                                        //semaphoreBufferList.Release ();
                                        //
                                        // ++++++++++++++++++++++++ [END] ++++++++++++++++++++++++

                                        if (loadImageMustBeExecute)
                                        {
                                            if (fileExt == ".jpg" || fileExt == ".jpeg" || fileExt == ".png" || fileExt == ".bmp" || fileExt == ".heic")
                                            {
                                                // This approach locks the file until the application is closed.
                                                //BitmapImage bi = new BitmapImage (new Url (file));

                                                BitmapImage bi = null;
                                                DateTime takenDate;


                                                using (var fstream = new FileStream (file, FileMode.Open, FileAccess.Read, FileShare.Read))
                                                {
                                                    bi = new BitmapImage ();
                                                    bi.BeginInit ();
                                                    bi.CacheOption = BitmapCacheOption.OnLoad;
                                                    bi.StreamSource = fstream;
                                                    bi.StreamSource.Flush ();
                                                    bi.EndInit ();
                                                    bi.Freeze ();

                                                    takenDate = GetTakenDate (fstream, file);

                                                    bi.StreamSource.Dispose ();
                                                }

                                                newBufferItem = new ImageFileBufferItem (file, bi, false, takenDate, System.IO.File.GetCreationTime (file));
                                            }
                                            else
                                            {
                                                // Get windows default icon.
                                                ImageSource imageSource = IconHelper.GetFileIcon (file);
                                                BitmapImage bi = IconHelper.ConvertImageSourceToBitmapImage (imageSource);
                                                DateTime takenDate = System.IO.File.GetCreationTime (file);


                                                newBufferItem = new ImageFileBufferItem (file, bi, true, takenDate, takenDate);
                                            }
                                        }

                                        semaphoreBufferList.WaitOne (10000);

                                        // ++++++++++++++++++++++++ [START] ++++++++++++++++++++++++
                                        // ++++ Feature is currently not controllable, since we always use this function with this parameter set! ++++
                                        //
                                        //if (bufferItemToUpdate == null)
                                        //{
                                        //    if (loadImageMustBeExecute)
                                        //    {
                                        //      // Add new image item to buffer list.
                                        _BufferList.Add (newBufferItem);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    // Update existing item in buffer.
                                        //    bufferItemToUpdate = newBufferItem;
                                        //}
                                        //
                                        // ++++++++++++++++++++++++ [END] ++++++++++++++++++++++++

                                        semaphoreBufferList.Release ();


                                        // Execute / fire the onImageLoad callback handler.
                                        onImageLoad?.Invoke (newBufferItem, fileCnt++, files.Length, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        errorMessages.Enqueue ($"{file} -> {ex.Message}");
                                    }
                                });


                            // Remove none existing files from the buffer list.
                            _BufferList.RemoveAll (x => !x.FileExists);


                            // Sorted list of ImageFileBuffer items.
                            var sortBuffer = _BufferList.OrderBy (x => x.Filename).ToList ();

                            _BufferList.Clear ();
                            foreach (var sortBufferItem in sortBuffer)
                            {
                                _BufferList.Add (sortBufferItem);
                            }


                            // Read all data from error message queue.
                            var errorMessageList = new List<string> ();
                            var errorMessage = String.Empty;
                            for (int i = 0; i < errorMessages.Count (); i++)
                            {
                                errorMessages.TryDequeue (out errorMessage);
                                errorMessageList.Add (errorMessage);
                            }


                            // Execute / fire the onAllImageLoad callback handler.
                            onAllImagesLoad?.Invoke (errorMessageList);
                        }
                        catch (Exception ex)
                        {
                            // Execute / fire the onAllImageLoad callback handler.
                            onAllImagesLoad?.Invoke (new List<string> { ex.Message });
                        }
                    });


                    #endregion
                }
            }
            catch
            {
                return null;
            }
        }



        /// <summary>
        /// Try to extract the taken time for a image file.
        /// It tries to use 2 various variants.
        /// </summary>
        /// <param name="bi">A BitmapImage instance (for Variant #1)</param>
        /// <param name="fs">A open file stream (for Variant #2)</param>
        /// <param name="file">For fallback Variant #3</param>
        /// <returns></returns>
        static private DateTime GetTakenDate (FileStream fs, string file)
        {
#warning "HEIF files are currently not supported. Try his lib to support them https://github.com/0xC0000054/libheif-sharp"


            try
            {
                // Retrieves the datetime WITHOUT loading the whole image.
                // By using FileStream, you can tell GDI + not to load the whole image for verification.It runs over 10 × as faster.
                // https://stackoverflow.com/a/7713780

                fs.Position = 0;

                using (System.Drawing.Image myImage = System.Drawing.Image.FromStream (fs, false, false))
                {
                    PropertyItem propItem = myImage.GetPropertyItem (36867);
                    Regex r = new Regex (":");


                    string dateTaken = r.Replace (Encoding.UTF8.GetString (propItem.Value), "-", 2);

                    return DateTime.Parse (dateTaken);
                }
            }
            catch
            {
                try
                {
                    return System.IO.File.GetCreationTime (file);
                }
                catch
                {
                    return DateTime.Today;
                }
            }
        }

    }



    public class ImageFileBufferItem
    {
        public string File { get; private set; }
        public BitmapImage Thumbnail { get; private set; }
        public bool IsSysIconImage { get; private set; }
        public DateTime TakenDate { get; private set; }
        public DateTime CreationTime { get; private set; }


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
        public string Filename { get { return Path.GetFileName (this.File); } }



        public ImageFileBufferItem (string file, BitmapImage thumbnail, bool isSysIconImage, DateTime takenDate, DateTime creationTime)
        {
            File = file;
            Thumbnail = thumbnail;
            IsSysIconImage = isSysIconImage;
            TakenDate = takenDate;
            CreationTime = creationTime;
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
