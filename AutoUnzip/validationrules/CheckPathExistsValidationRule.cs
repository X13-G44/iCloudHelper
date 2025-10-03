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
    public class CheckPathExistsValidationRule : ValidationRule
    {
        private CheckPathExistsValidationRuleWrapper _Wrapper = null;
        public CheckPathExistsValidationRuleWrapper Wrapper
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
            var path = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;


            if (string.IsNullOrWhiteSpace (path))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No text." : errorText);
            }

            if (!Directory.Exists (path))
            {
                return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Directory does not exists." : errorText);
            }

            return ValidationResult.ValidResult;
        }
    }



    public class CheckPathExistsValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckPathExistsValidationRuleWrapper), new PropertyMetadata (""));
    }
}
