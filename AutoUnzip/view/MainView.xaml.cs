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



using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;



namespace AutoUnzip.View
{
    public partial class MainWindow : Window
    {
        const int MARGIN = 20;



        Storyboard _Storyboard = new Storyboard ();



        public MainWindow ()
        {
            InitializeComponent ();

            this.DataContext = new ViewModel.MainViewModel (Dispatcher);
            (this.DataContext as ViewModel.MainViewModel).StartFadeingAnimation += StartFadeing;
            (this.DataContext as ViewModel.MainViewModel).StopFadeingAnimation += StopFadeing;

            this.Loaded += (s, e) =>
            {
                // Relocate the window after it has been loaded and it size is known.
                RelocateWindow ();

                this.Opacity = 0.0;
                this.Visibility = Visibility.Hidden;
            };

            #region Prepare fading animation

            DoubleAnimation fadeInAnimation = new DoubleAnimation (0.0, 1.0, new Duration (TimeSpan.FromSeconds (1.5)));
            DoubleAnimation fadeOutAnimation = new DoubleAnimation (1.0, 0.0, new Duration (TimeSpan.FromSeconds (1.5)))
            {
                BeginTime = TimeSpan.FromSeconds (20)
            };

            _Storyboard.Children.Add (fadeInAnimation);
            _Storyboard.Children.Add (fadeOutAnimation);
            _Storyboard.Completed += (s, e) => { this.Visibility = Visibility.Hidden; };

            // Set the target property for the animation
            Storyboard.SetTarget (fadeInAnimation, this);
            Storyboard.SetTarget (fadeOutAnimation, this);
            Storyboard.SetTargetProperty (fadeInAnimation, new PropertyPath (OpacityProperty));
            Storyboard.SetTargetProperty (fadeOutAnimation, new PropertyPath (OpacityProperty));

            #endregion
        }



        public void OnNewExtractedImages (List<String> extractedFiles)
        {
            (this.DataContext as ViewModel.MainViewModel).OnNewExtractedImages (extractedFiles);
        }



        public void OnColorTheme ()
        {
            (this.DataContext as ViewModel.MainViewModel).OnColorTheme ();
        }



        private void RelocateWindow ()
        {
            int pos = ConfigurationStorage.ConfigurationStorageModel.NewImagesExtractedNotifyWinPos;

            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            var workingArea = primaryScreen.WorkingArea;


            switch (pos)
            {
                default:
                case 3:
                    {
                        // Bottom-Right

                        double newLeft = workingArea.Right - this.Width - MARGIN;
                        double newTop = workingArea.Bottom - this.Height - MARGIN;

                        this.Top = newTop;
                        this.Left = newLeft;

                        break;
                    }

                case 2:
                    {
                        // Bottom-Left

                        double newLeft = workingArea.Left + MARGIN;
                        double newTop = workingArea.Bottom - this.Height - MARGIN;

                        this.Top = newTop;
                        this.Left = newLeft;

                        break;
                    }

                case 1:
                    {
                        // Top-Right

                        double newLeft = workingArea.Right - this.Width - MARGIN;
                        double newTop = workingArea.Top + MARGIN;

                        this.Top = newTop;
                        this.Left = newLeft;

                        break;
                    }

                case 0:
                    {
                        // Top-Left

                        double newLeft = workingArea.Left + MARGIN;
                        double newTop = workingArea.Top + MARGIN;

                        this.Top = newTop;
                        this.Left = newLeft;

                        break;
                    }
            }
        }



        private void StartFadeing ()
        {
            this.Visibility = Visibility.Visible;

            _Storyboard.Begin ();
        }



        private void StopFadeing ()
        {                
            _Storyboard.Stop ();

            this.Visibility = Visibility.Hidden;
        }
    }
}
