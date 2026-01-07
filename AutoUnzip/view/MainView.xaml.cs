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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;



namespace AutoUnzip.View
{
    public partial class MainWindow : Window
    {
        const int MARGIN = 20;



        public MainWindow (List<String> extractedFiles)
        {
            InitializeComponent ();

            this.DataContext = new ViewModel.MainViewModel (Dispatcher, this, extractedFiles);
            this.Opacity = 0;   // Start hidden

            this.Loaded += (s, e) =>
            {
                // Relocate the window after it has been loaded and its size known.
                RelocateWindow ();
            };
        }



        private void RelocateWindow ()
        {
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen;
            var workingArea = primaryScreen.WorkingArea;

            double newLeft = workingArea.Right - this.Width - MARGIN;
            double newTop = workingArea.Bottom - this.Height - MARGIN;

            this.Top = newTop;
            this.Left = newLeft;
        }
    }
}
