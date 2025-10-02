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
    public class CheckPathExistsValidationRule : ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var path = value as string;


            if (string.IsNullOrWhiteSpace (path))
            {
                return new ValidationResult (false, "Field must be filled with a valid path.");
            }

            if (!Directory.Exists (path))
            {
                return new ValidationResult (false, "Path does not exists.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
