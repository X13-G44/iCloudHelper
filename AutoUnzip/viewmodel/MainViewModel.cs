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



using AutoUnzip.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;



namespace AutoUnzip.viewmodel
{
    public class MainViewModel : ViewModelBase
    {
        const int MAX_FILES_TO_SHOW = 5;



        public ObservableCollection<BitmapImage> ExtractedFilesPreview { get; set; } = new ObservableCollection<BitmapImage> ();



        private string _ExtractedFileText;
        public string ExtractedFileText
        {
            get { return _ExtractedFileText; }
            set { _ExtractedFileText = value; OnPropertyChanged (nameof (ExtractedFileText)); }
        }



        public RelayCommand Cmd_FadeAnimationEnded
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
#warning For testing inactive!
                        //_View.Close ();
                    },
                    param => true
                );
            }
        }

        public RelayCommand Cmd_RunQuickSortApp
        {
            get
            {
                return new RelayCommand (
                    _ =>
                    {
                        try
                        {
                            if (File.Exists (Properties.Settings.Default.QuickSortApp))
                            {
                                Process.Start (Properties.Settings.Default.QuickSortApp, "\"" + Properties.Settings.Default.ExtractPath + "\"");

                                _View.Close ();
                            }
                        }
                        catch
                        {
                        }
                    },
                    param => File.Exists (Properties.Settings.Default.QuickSortApp)
                );
            }
        }



        private readonly Dispatcher _Dispatcher;
        private readonly Window _View;
        private readonly List<String> _ExtractedFiles;



        public MainViewModel (Dispatcher dispatcher, Window view, List<String> extractedFiles)
        {
            _Dispatcher = dispatcher;
            _View = view;
            _ExtractedFiles = extractedFiles;

            this.ExtractedFileText = LocalizedStrings.GetFormattedString ("tbMain_ExtractedFileText", extractedFiles.Count);
            this.ExtractedFilesPreview.Clear ();

            SetColorTheme ();
            LoadExtractedFilesPreviewListAsync ();
        }



        private Task LoadExtractedFilesPreviewListAsync ()
        {
            return Task.Run (() =>
            {
                if (_ExtractedFiles.Count > MAX_FILES_TO_SHOW)
                {
                    Random rnd = new Random ();


                    for (int counter = 0; counter < MAX_FILES_TO_SHOW; counter++)
                    {
                        int rndFileIndex = rnd.Next (_ExtractedFiles.Count);
                        BitmapImage bi = LoadImageFile (_ExtractedFiles[rndFileIndex]);


                        if (bi != null)
                        {
                            _Dispatcher.Invoke (() =>
                            {
                                this.ExtractedFilesPreview.Add (bi);
                            });
                        }
                    }
                }
                else
                {
                    foreach (string file in _ExtractedFiles)
                    {
                        BitmapImage bi = LoadImageFile (file);


                        if (bi != null)
                        {
                            _Dispatcher.Invoke (() =>
                            {
                                this.ExtractedFilesPreview.Add (bi);
                            });
                        }
                    }
                }
            });
        }



        private BitmapImage LoadImageFile (string file)
        {
            try
            {
                // This approach locks the file until the application is closed.
                //BitmapImage bi = new BitmapImage (new Uri (file));

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

                return bi;


            }
            catch
            {
                return null;
            }
        }



        private void SetColorTheme ()
        {
            string themeFile;
            switch (AutoUnzip.Properties.Settings.Default.ColorThemeId)
            {
                default:
                case 0:
                    {
                        themeFile = "/view/theme/ColorThemeLightMode.xaml";
                        break;
                    }

                case 1:
                    {
                        themeFile = "/view/theme/ColorThemeDarkMode.xaml";
                        break;
                    }
            }
            var dict = new ResourceDictionary { Source = new Uri (themeFile, UriKind.Relative) };


            // Remove the old (initial) theme.
            var oldDict = System.Windows.Application.Current.Resources.MergedDictionaries.FirstOrDefault (d => d.Source != null && (d.Source.OriginalString.Contains ("/view/theme/ColorThemeDarkMode.xaml") ||
                                                                                                               d.Source.OriginalString.Contains ("/view/theme/ColorThemeLightMode.xaml")));
            if (oldDict != null)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Remove (oldDict);
            }

            // Add new theme.
            System.Windows.Application.Current.Resources.MergedDictionaries.Add (dict);
        }
    }
}
