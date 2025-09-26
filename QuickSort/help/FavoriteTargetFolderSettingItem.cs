using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace QuickSort.help
{
    public class FavoriteTargetFolderSettingItem
    {
        public string Path { get; set; } = "";
        public long Date { get; set; } = 0;
        public string DisplayName { get; set; } = "";
        public bool IsPinned { get; set; } = false;



        public FavoriteTargetFolderSettingItem () 
        {
            ;               
        }



        public FavoriteTargetFolderSettingItem (string path, string data, string displayName, bool isPinned)
        {
            this.Path = path;
            this.Date = long.Parse (data);
            this.DisplayName = displayName;
            this.IsPinned = isPinned;
        }



        public FavoriteTargetFolderSettingItem (string path, long data, string displayName, bool isPinned)
        {
            this.Path = path;
            this.Date = data;
            this.DisplayName = displayName;
            this.IsPinned = isPinned;
        }



        public override string ToString ()
        {
            return $"{Path}?{Date}?{DisplayName}?{IsPinned}";
        }



        public static FavoriteTargetFolderSettingItem Parse (string asString)
        {
            try
            {
                string[] elements = asString.Split ('?');


                if (elements.Length == 4)
                {
                    return new FavoriteTargetFolderSettingItem ()
                    {
                        Path = elements[0],
                        Date = long.Parse (elements[1]),
                        DisplayName = elements[2],
                        IsPinned = bool.Parse(elements[3]),
                    };
                }

                throw new FormatException ("Invalid format for FavoriteTargetFolderSettingItem. Expected format: Path?Date?DisplayName");
            }
            catch
            {
                throw;
            }
        }
    }
}
