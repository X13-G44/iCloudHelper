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
    public class CheckIntLessThan16ValidationRule : ValidationRule
    {
        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            int number = 0;


            if (value == null || !(value is string))
            {
                return new ValidationResult (false, "Value must be a integer string");
            }

            if (int.TryParse (value.ToString (), out number) == false)
            {
                return new ValidationResult (false, "Value must be a integer");
            }

            if (number > 15 || number <= 0)
            {
                return new ValidationResult (false, "Value must between 1 and 15");
            }

            return ValidationResult.ValidResult;
        }
    }
}
