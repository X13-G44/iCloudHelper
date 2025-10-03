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
    public class CheckDirPrefixValidationRule : ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            char[] invalidChars = Path.GetInvalidPathChars ();
            var filename = value as string;


            if (string.IsNullOrWhiteSpace (filename))
            {
                return new ValidationResult (false, "Field must be filled with a prefix string");
            }

            if (filename.StartsWith (" ") || filename.EndsWith (" "))
            {
                return new ValidationResult (false, "Prefix starts / ends with invalid chars.");
            }

            if (filename.Length <= 3)
            {
                return new ValidationResult (false, "Prefix must have at least 4 or more chars.");
            }

            return filename.Any (c => invalidChars.Contains (c)) ? new ValidationResult (false, "There are invalid chars.") : ValidationResult.ValidResult;
        }
    }
}
