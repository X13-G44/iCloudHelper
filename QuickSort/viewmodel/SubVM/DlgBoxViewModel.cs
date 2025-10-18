/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	    https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				16.10.2025
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



using QuickSort.view.UserControls;
using QuickSort.viewmodel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;



namespace QuickSort.ViewModel
{
    /// <summary>
    /// Class to configure a DlgBoxUserControl instance.
    /// </summary>
    public class DlgBoxViewModel : ViewModelBase
    {
        public DlgBoxType Type { get; private set; } = DlgBoxType.Question;
        public String Title { get; private set; } = String.Empty;
        public String Message { get; private set; } = String.Empty;



        public DlgBoxButton LeftButton { get; private set; } = null;
        public DlgBoxButton RightButton { get; private set; } = null;
        /// <summary>
        /// TextBox config property. Use this to get the entered user text from the TextBox.
        /// </summary>
        public DlgBoxTextBox TextBox { get; private set; } = null;



        /// <summary>
        /// Configure and show a DlgBox message box (DlgBoxUserControl).
        /// 
        /// Put the returned instance to the DlgBoxUserControl.Configuration property to show the dialog box. 
        /// </summary>
        /// <param name="type">Dialog type</param>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <param name="rightButton">Right button configuration</param>
        /// <param name="leftButton">Optional left button configuration</param>
        /// <param name="textBox">Optional TextBox configuration</param>
        /// <returns>Initialized DlgBoxViewModel instance to use by the DlgBoxUserControl.Configuration instance</returns>
        static public DlgBoxViewModel ShowDialog (DlgBoxType type, String title, String message, DlgBoxButton rightButton, DlgBoxButton leftButton = null, DlgBoxTextBox textBox = null)
        {
            if (rightButton == null)
            {
                throw new ArgumentNullException ("Param \"rightButton\" is null");
            }


            return new DlgBoxViewModel
            {
                Type = type,
                Title = title,
                Message = message,
                LeftButton = leftButton,
                RightButton = rightButton,
                TextBox = textBox
            };
        }



        /// <summary>
        /// Configure and show a simply DlgBox message box (DlgBoxUserControl).
        /// Only one "OK" button is displayed.
        /// 
        /// Put the returned instance to the DlgBoxUserControl.Configuration property to show the dialog box. 
        /// </summary>
        /// <param name="type">Dialog type</param>
        /// <param name="title">Dialog title</param>
        /// <param name="message">Dialog message</param>
        /// <returns></returns>
        static public DlgBoxViewModel ShowDialogSimply (DlgBoxType type, String title, String message)
        {
            return new DlgBoxViewModel
            {
                Type = type,
                Title = title,
                Message = message,
                LeftButton = null,
                RightButton = new DlgBoxButton ("OK", DlgBoxButtonSymbol.Check, null, _ => {; }),
                TextBox = null
            };
        }
    }



    /// <summary>
    /// Button configuration class.
    /// </summary>
    public class DlgBoxButton
    {
        public Object Parameter { get; private set; }
        public Action<DlgBoxViewModel> Action { get; private set; }
        public String Text { get; private set; }
        public DlgBoxButtonSymbol Symbol { get; private set; }



        public DlgBoxButton (String text, DlgBoxButtonSymbol symbol, Object parameter, Action<DlgBoxViewModel> onClick)
        {
            if (String.IsNullOrEmpty (text) || onClick == null)
            {
                throw new ArgumentNullException ();
            }

            Text = text;
            Symbol = symbol;
            Parameter = parameter;
            Action = onClick;
        }
    }


    /// <summary>
    /// TextBox configuration class.
    /// </summary>
    public class DlgBoxTextBox : ViewModelBase
    {
        private String _Text;
        /// <summary>
        /// This property holds the entered user text of the DialogBox textbox field.
        /// Use this property also to change the shown text live.
        /// </summary>
        public String Text
        {
            get { return _Text; }
            set { _Text = value; OnPropertyChanged (nameof (Text)); }
        }

        public Collection<ValidationRule> ValidationRules { get; private set; } = new Collection<ValidationRule> ();



        public DlgBoxTextBox (String initialText, Collection<ValidationRule> validationRules = null)
        {
            _Text = initialText;

            if (validationRules != null && validationRules.Count > 0)
            {
                ValidationRules = validationRules;
            }
        }
    }
}
