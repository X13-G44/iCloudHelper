/// ////////////////////////////////////////////////////////////////////////
///
/// Project:			iCloudHelper
/// Project Source:	https://github.com/X13-G44/iCloudHelper
///
/// Author: 			Christian Harscher <info@x13-g44.com>
/// Date:				14.03.2026
///
/// ////////////////////////////////////////////////////////////////////////
/// 
/// SPDX-License-Identifier: Apache-2.0
/// Copyright (c) 2026 Christian Harscher (alias X13-G44)
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



namespace QuickSort.ValidationRules
{
    public class TextLengthValidationRule : ValidationRule
    {
        private TextLengthValidationRuleWrapper _Wrapper = null;
        public TextLengthValidationRuleWrapper Wrapper
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

        private uint _MinLength = 0;

        public uint MinLength
        {
            get { return _MinLength; }
            set
            {
                if (this.MaxLength < this.MinLength)
                {
                    throw new ArgumentException ("MaxLength must be greater or equal than MinLength.");
                }
                _MinLength = value;
            }
        }

        private uint _MaxLength = 100;

        public uint MaxLength
        {
            get { return _MaxLength; }
            set
            {
                if (this.MaxLength < this.MinLength)
                {
                    throw new ArgumentException ("MaxLength must be greater or equal than MinLength.");
                }
                _MaxLength = value;
            }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var text = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            uint minLength = this.Wrapper != null ? this.Wrapper.MinLength : this.MinLength;
            uint maxLength = this.Wrapper != null ? this.Wrapper.MaxLength : this.MaxLength;

            if (text == null)
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetString ("lNoText") : errorText);
            }
            else
            {
                int length = text.Length;


                if (length < minLength || length > maxLength)
                {
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetFormattedString ("lTextLengthError", minLength, maxLength) : errorText);
                }
                else
                {
                    return ValidationResult.ValidResult;
                }
            }
        }
    }



    public class TextLengthValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckPathExistsValidationRuleWrapper), new PropertyMetadata (""));



        public uint MinLength
        {
            get { return (uint) GetValue (MinLengthProperty); }
            set { SetValue (MinLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinLengthProperty =
            DependencyProperty.Register (nameof (MinLength), typeof (uint), typeof (TextLengthValidationRuleWrapper), new PropertyMetadata (0, OnMinMaxValue_PropertyChangedCallback));



        public uint MaxLength
        {
            get { return (uint) GetValue (MaxLengthProperty); }
            set { SetValue (MaxLengthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxLength.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxLengthProperty =
            DependencyProperty.Register (nameof (MaxLength), typeof (uint), typeof (TextLengthValidationRuleWrapper), new PropertyMetadata (100, OnMinMaxValue_PropertyChangedCallback));



        private static void OnMinMaxValue_PropertyChangedCallback (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextLengthValidationRuleWrapper source = (TextLengthValidationRuleWrapper) d;


            if (source.MaxLength < source.MinLength)
            {
                throw new ArgumentException ("MaxLength must be greater or equal than MinLength.");
            }
        }
    }
}
