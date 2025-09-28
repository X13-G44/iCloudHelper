using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;



namespace QuickSort.validationrules
{
    public class CheckDirectoryNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
           var newDirectoryName = value as string;


            if (IsValidDirectoryName (newDirectoryName) == false)
            {
                return new ValidationResult (false, "Invalid directory name.");
            }

            return ValidationResult.ValidResult;
        }



        static public bool IsValidDirectoryName (string directoryNameToCheck)
        {
            if (string.IsNullOrWhiteSpace (directoryNameToCheck))
            {
                return false;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars ();
            return !directoryNameToCheck.Any (c => invalidChars.Contains (c));
        }
    }
}
