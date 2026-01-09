/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				09.01.2026
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



using QuickSort.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static QuickSort.ValidationRules.CheckDirectoryNameValidationRule;



namespace QuickSort.ValidationRules
{
    public class CheckInvalidCharsValidationRule : ValidationRule
    {
        private CheckInvalidCharsValidationRuleWrapper _Wrapper = null;
        public CheckInvalidCharsValidationRuleWrapper Wrapper
        {
            get { return _Wrapper; }
            set { _Wrapper = value; }
        }

        private string _ErrorMessage = string.Empty;
        public string ErrorMessage
        {
            get { return _ErrorMessage; }
            set { _ErrorMessage = value; }
        }

        private string _InvalidChares;

        public string InvalidChares
        {
            get { return _InvalidChares; }
            set { _InvalidChares = value; }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var newDisplayName = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            String invalidChares = this.Wrapper != null ? this.Wrapper.InvalidChares : this.InvalidChares;


            for (int i = 0; i < invalidChares.Length; i++)
            {
                if (newDisplayName.Contains (invalidChares[i]))
                {
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetFormattedString ("lInvalidDisplayNameChars", invalidChares[i]) : errorText);
                }
            }

            return ValidationResult.ValidResult;
        }
    }



    public class CheckInvalidCharsValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckPathExistsValidationRuleWrapper), new PropertyMetadata (""));



        public string InvalidChares
        {
            get { return (string) GetValue (InvalidCharesProperty); }
            set { SetValue (InvalidCharesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvalidChares.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvalidCharesProperty =
            DependencyProperty.Register (nameof (InvalidChares), typeof (string), typeof (CheckInvalidCharsValidationRuleWrapper), new PropertyMetadata (""));
    }
}
