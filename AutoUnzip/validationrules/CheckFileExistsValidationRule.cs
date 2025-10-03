using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;



namespace AutoUnzip.validationrules
{
    public  class CheckFileExistsValidationRule: ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var file = value as string;


            if (string.IsNullOrWhiteSpace (file))
            {
                return new ValidationResult (false, "Field must be filled with an application file path.");
            }

            if (!File.Exists (file))
            {
                return new ValidationResult (false, "Application file does not exists.");
            }

            if (!file.ToLower().EndsWith(".exe"))
            {
                return new ValidationResult (false, "No application file selected.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
