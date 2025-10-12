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



using QuickSort.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



namespace QuickSort.validationrules
{
    public class CheckIntValidationRule : ValidationRule
    {
        private CheckIntValidationRuleWrapper _Wrapper = null;
        public CheckIntValidationRuleWrapper Wrapper
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

        private int _MinValue = 0;
        public int MinValue
        {
            get { return _MinValue; }
            set
            {
                if (this.MaxValue < this.MinValue)
                {
                    throw new ArgumentException ("MaxValue must be greater or equal than MinValue.");
                }
                _MinValue = value;
            }
        }

        private int _MaxValue = 1;
        public int MaxValue
        {
            get { return _MaxValue; }
            set
            {
                if (this.MaxValue < this.MinValue)
                {
                    throw new ArgumentException ("MaxValue must be greater or equal than MinValue.");
                }
                _MaxValue = value;
            }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            int number = 0;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            int minValue = this.Wrapper != null ? this.Wrapper.MinValue : this.MinValue;
            int maxValue = this.Wrapper != null ? this.Wrapper.MaxValue : this.MaxValue;


            if (value == null || !(value is string))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetString("lNoText") : errorText);
            }

            if (int.TryParse (value.ToString (), out number) == false)
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetString("lTextMustBeANumber") : errorText);
            }

            if (number < minValue || number > maxValue)
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? LocalizedStrings.GetFormattedString("lNumberRangeError", minValue, maxValue) : errorText);
            }

            return ValidationResult.ValidResult;
        }
    }



    public class CheckIntValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckIntValidationRuleWrapper), new PropertyMetadata (""));



        public int MinValue
        {
            get { return (int) GetValue (MinValueProperty); }
            set { SetValue (MinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register ("MinValue", typeof (int), typeof (CheckIntValidationRuleWrapper), new PropertyMetadata (0, OnMinMaxValue_PropertyChangedCallback));



        public int MaxValue
        {
            get { return (int) GetValue (MaxValueProperty); }
            set { SetValue (MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register ("MaxValue", typeof (int), typeof (CheckIntValidationRuleWrapper), new PropertyMetadata (1, OnMinMaxValue_PropertyChangedCallback));



        private static void OnMinMaxValue_PropertyChangedCallback (DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CheckIntValidationRuleWrapper source = (CheckIntValidationRuleWrapper) d;


            if (source.MaxValue < source.MinValue)
            {
                throw new ArgumentException ("MaxValue must be greater or equal than MinValue.");
            }
        }
    }
}

