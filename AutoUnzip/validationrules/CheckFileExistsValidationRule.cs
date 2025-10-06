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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



namespace AutoUnzip.validationrules
{
    public class CheckFileExistsValidationRule : ValidationRule
    {
        private CheckFileExistsValidationRuleWrapper _Wrapper = null;
        public CheckFileExistsValidationRuleWrapper Wrapper
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

        private bool _IgnoreFileExtension = false;
        public bool IgnoreFileExtension
        {
            get { return _IgnoreFileExtension; }
            set { _IgnoreFileExtension = value; }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var file = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            bool ignoreFileExtension = this.Wrapper != null ? this.Wrapper.IgnoreFileExtension : this.IgnoreFileExtension;


            if (string.IsNullOrWhiteSpace (file))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No text." : errorText);
            }

            if (!File.Exists (file))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Application file does not exists." : errorText);
            }

            if (!ignoreFileExtension && !file.ToLower ().EndsWith (".exe"))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No application file selected." : errorText);
            }

            return ValidationResult.ValidResult;
        }
    }



    public class CheckFileExistsValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckFileExistsValidationRuleWrapper), new PropertyMetadata (""));



        public bool IgnoreFileExtension
        {
            get { return (bool) GetValue (IgnoreFileExtensionProperty); }
            set { SetValue (IgnoreFileExtensionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for _IgnoreFileExtension.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IgnoreFileExtensionProperty =
            DependencyProperty.Register ("IgnoreFileExtension", typeof (bool), typeof (CheckFileExistsValidationRuleWrapper), new PropertyMetadata (false));


    }
}
