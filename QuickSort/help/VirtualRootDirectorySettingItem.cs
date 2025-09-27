using QuickSort.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickSort.help
{
    /// <summary>
    /// Help class to convert "Virtual Root Directory Model" object settings into a string and back.
    /// It is used to store the date in application settings (string collection).
    /// </summary>
    public class VirtualRootDirectorySettingItem
    {
        public string Path { get; set; } = "";
        public string DisplayName { get; set; } = "";



        public VirtualRootDirectorySettingItem ()
        {
            ;
        }



        public VirtualRootDirectorySettingItem (string path, string displayName)
        {
            this.Path = path;
            this.DisplayName = displayName;
        }



        public VirtualRootDirectorySettingItem (VirtualDirectoryModel model)
        {
            this.Path = model.Path;
            this.DisplayName = model.DisplayName;
        }



        public override string ToString ()
        {
            return $"{Path}?{DisplayName}";
        }



        public static VirtualRootDirectorySettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 2)
                {
                    return new VirtualRootDirectorySettingItem ()
                    {
                        Path = elements[0],
                        DisplayName = elements[1],
                    };
                }

                throw new FormatException ("Invalid format for VirtualRootDirectorySettingItem. Expected format: Path?DisplayName");
            }
            catch
            {
                throw;
            }
        }
    }
}
