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
    public class CheckDirPrefixValidationRule : ValidationRule
    {
        private CheckDirPrefixValidationRuleWrapper _Wrapper = null;
        public CheckDirPrefixValidationRuleWrapper Wrapper
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



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            char[] invalidChars = Path.GetInvalidPathChars ();
            var filename = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;


            if (string.IsNullOrWhiteSpace (filename))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No text" : errorText);
            }

            if (filename.StartsWith (" ") || filename.EndsWith (" "))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Prefix starts / ends with invalid chars." : errorText);
            }

            if (filename.Length <= 3)
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Prefix must have at least 4 or more chars." : errorText);
            }

            return filename.Any (c => invalidChars.Contains (c)) ? new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "There are invalid chars." : errorText) : ValidationResult.ValidResult;
        }
    }



    public class CheckDirPrefixValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckDirPrefixValidationRuleWrapper), new PropertyMetadata (""));
    }
}
