using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;



namespace QuickSort.validationrules
{
    public class CheckDirectoryNameValidationRule : ValidationRule
    {
        public enum CheckDirectoryNameResult
        {
            Success,
            NoText,
            InvalidDirectoryName,
            DirectoryAlreadyExists,
        }



        private CheckDirectoryNameValidationRuleWrapper _Wrapper = null;
        public CheckDirectoryNameValidationRuleWrapper Wrapper
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

        private string _RootDirectory = string.Empty;
        public string RootDirectory // Optional root directory path, to check for existing directories within this root.
        {
            get { return _RootDirectory; }
            set { _RootDirectory = value; }
        }



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var newDirectoryName = value as string;

            String errorText = this.Wrapper != null ? this.Wrapper.ErrorMessage : this.ErrorMessage;
            String rootDirectory = this.Wrapper != null ? this.Wrapper.RootDirectory : this.RootDirectory;


            switch (IsValidDirectoryName (newDirectoryName, rootDirectory))
            {
                case CheckDirectoryNameResult.Success:
                    return ValidationResult.ValidResult;

                case CheckDirectoryNameResult.NoText:
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "No Text." : errorText);

                case CheckDirectoryNameResult.InvalidDirectoryName:
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Invalid directory name." : errorText);

                case CheckDirectoryNameResult.DirectoryAlreadyExists:
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Directory already exists." : errorText);

                default:
                    return new ValidationResult (false, String.IsNullOrEmpty (errorText) ? "Invalid directory name." : errorText);
            }
        }



        static public CheckDirectoryNameResult IsValidDirectoryName (string directoryNameToCheck, string rootDirectory = null)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars ();


            if (string.IsNullOrWhiteSpace (directoryNameToCheck))
            {
                return CheckDirectoryNameResult.NoText;
            }

            if (directoryNameToCheck.StartsWith (" "))
            {
                return CheckDirectoryNameResult.InvalidDirectoryName;
            }

            if (directoryNameToCheck.EndsWith (" "))
            {
                return CheckDirectoryNameResult.InvalidDirectoryName;
            }


            if (!string.IsNullOrEmpty (rootDirectory))
            {
                var fullPath = Path.Combine (rootDirectory, directoryNameToCheck);


                if (Directory.Exists (fullPath))
                {
                    return CheckDirectoryNameResult.DirectoryAlreadyExists;
                }
            }


            return directoryNameToCheck.Any (c => invalidChars.Contains (c)) ? CheckDirectoryNameResult.InvalidDirectoryName : CheckDirectoryNameResult.Success;
        }
    }



    public class CheckDirectoryNameValidationRuleWrapper : DependencyObject
    {
        public String ErrorMessage
        {
            get { return (String) GetValue (ErrorMessageProperty); }
            set { SetValue (ErrorMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ErrorMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register ("ErrorMessage", typeof (String), typeof (CheckDirectoryNameValidationRuleWrapper), new PropertyMetadata (""));



        public string RootDirectory
        {
            get { return (string) GetValue (RootDirectoryProperty); }
            set { SetValue (RootDirectoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RootDirectory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RootDirectoryProperty =
            DependencyProperty.Register ("RootDirectory", typeof (string), typeof (CheckDirectoryNameValidationRuleWrapper), new PropertyMetadata (""));
    }
}
