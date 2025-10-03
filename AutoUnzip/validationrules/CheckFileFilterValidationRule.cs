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
    public class CheckFileFilterValidationRule: ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var invalidChars = Path.GetInvalidFileNameChars ().Where (c => c != '*').ToArray ();
            var fileFilter = value as string;


            if (string.IsNullOrWhiteSpace (fileFilter))
            {
                return new ValidationResult (false, "Field must be filled with a valid path.");
            }

            if (fileFilter.StartsWith (" ") || fileFilter.EndsWith (" "))
            {
                return new ValidationResult (false, "Prefix starts / ends with invalid chars.");
            }

            if (fileFilter.Length <= 3)
            {
                return new ValidationResult (false, "Prefix must have at least 4 or more chars.");
            }

            return fileFilter.Any (c => invalidChars.Contains (c)) ? new ValidationResult (false, "There are invalid chars.") : ValidationResult.ValidResult;
        }
    }
}
