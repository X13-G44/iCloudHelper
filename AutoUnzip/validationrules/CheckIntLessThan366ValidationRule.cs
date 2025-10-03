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
    public class CheckIntLessThan366ValidationRule : ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            int number = 0;


            if (value == null || !(value is string))
            {
                return new ValidationResult (false, "Value must be a integer string.");
            }

            if (int.TryParse (value.ToString (), out number) == false)
            {
                return new ValidationResult (false, "Value must be a integer.");
            }

            if (number > 366 || number < 0)
            {
                return new ValidationResult (false, "Value must be in range of 0 to 366.");
            }

            return ValidationResult.ValidResult;
        }
    }
}

