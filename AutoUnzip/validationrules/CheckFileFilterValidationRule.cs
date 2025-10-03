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
    public class CheckFileFilterValidationRule : ValidationRule
    {
        private CheckFileFilterValidationRuleWrapper _Wrapper = null;
        public CheckFileFilterValidationRuleWrapper Wrapper
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

        private bool _AllowStarChar = true;
        public bool AllowStarChar
        {
            get { return _AllowStarChar; }
            set { _AllowStarChar = value; }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            char[] invalidChars;
            var fileFilter = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            bool allowStarChar = this.Wrapper != null ? this.Wrapper.AllowStarChar : this.AllowStarChar;


            if (allowStarChar)
            {
                invalidChars = Path.GetInvalidFileNameChars ().Where (c => c != '*').ToArray (); // Remove '*' from array.
            }
            else
            {
                invalidChars = Path.GetInvalidFileNameChars ();
            }

            if (string.IsNullOrWhiteSpace (fileFilter))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No text." : errorText);
            }

            if (fileFilter.StartsWith (" ") || fileFilter.EndsWith (" "))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Prefix starts / ends with invalid chars." : errorText);
            }

            if (fileFilter.Length <= 3)
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Prefix must have at least 4 or more chars." : errorText);
            }

            return fileFilter.Any (c => invalidChars.Contains (c)) ? new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "There are invalid chars." : errorText) : ValidationResult.ValidResult;
        }
    }



    public class CheckFileFilterValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckFileFilterValidationRuleWrapper), new PropertyMetadata (""));



        public bool AllowStarChar
        {
            get { return (bool) GetValue (AllowStarCharProperty); }
            set { SetValue (AllowStarCharProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AllowStarChar.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowStarCharProperty =
            DependencyProperty.Register ("AllowStarChar", typeof (bool), typeof (CheckFileFilterValidationRuleWrapper), new PropertyMetadata (true));
    }
}
