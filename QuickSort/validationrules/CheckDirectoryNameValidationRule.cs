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
        public enum CheckDirectoryNameResult
        {
            Success,
            InvalidDirectoryName,
            DirectoryAlreadyExists,
        }



        public String RootDirectory { get; set; } = null;   // Optional root directory path, to check for existing directories within this root.



        public override ValidationResult Validate (object value, CultureInfo cultureInfo)
        {
            var newDirectoryName = value as string;


            switch (IsValidDirectoryName (newDirectoryName, this.RootDirectory))
            {
                case CheckDirectoryNameResult.Success:
                    return ValidationResult.ValidResult;

                case CheckDirectoryNameResult.InvalidDirectoryName:
                    return new ValidationResult (false, "Invalid directory name.");

                case CheckDirectoryNameResult.DirectoryAlreadyExists:
                    return new ValidationResult (false, "Directory already exists.");

                default:
                    return new ValidationResult (false, "Invalid directory name.");
            }
        }



        static public CheckDirectoryNameResult IsValidDirectoryName (string directoryNameToCheck, string rootDirectory = null)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars ();


            if (string.IsNullOrWhiteSpace (directoryNameToCheck))
            {
                return CheckDirectoryNameResult.InvalidDirectoryName;
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
}
