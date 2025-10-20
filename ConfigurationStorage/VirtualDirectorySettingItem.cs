using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace ConfigurationStorage
{
    /// <summary>
    /// Help class to convert "Virtual Root Directory Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    internal class VirtualDirectorySettingItem
    {
        public string Path { get; set; } = "";
        public string DisplayName { get; set; } = "";



        public VirtualDirectorySettingItem ()
        {
            ;
        }



        public VirtualDirectorySettingItem (string path, string displayName)
        {
            Path = path;
            DisplayName = displayName;
        }



        public override string ToString ()
        {
            return $"{Path}?{DisplayName}";
        }



        public static VirtualDirectorySettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 2)
                {
                    return new VirtualDirectorySettingItem ()
                    {
                        Path = elements[0],
                        DisplayName = elements[1],
                    };
                }

                throw new FormatException ("Invalid format for VirtualDirectorySettingItem. Expected format: Path?DisplayName");
            }
            catch
            {
                throw;
            }
        }
    }
}
