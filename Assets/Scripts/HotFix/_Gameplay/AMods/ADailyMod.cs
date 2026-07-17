using UnityEngine;

namespace MoreMountains
{
    public class ADailyMod
    {
        public string name;
        public string description;
        public string modID;
        public Texture img;
        public bool positive;
        public APlayer.PlayerClass classToExclude;
        private static string IMG_DIR = "images/ui/run_mods/";

        public ADailyMod(string setId, string name, string description, string imgUrl, bool positive):this(setId, name, description, imgUrl, positive, default)
        {
        }

        public ADailyMod(string setId, string name, string description, string imgUrl, bool positive, APlayer.PlayerClass exclusion)
        {
            this.modID = setId;
            this.name = name;
            this.description = description;
            this.positive = positive;
            // this.img = ImageMaster.loadImage("images/ui/run_mods/" + imgUrl);
            this.classToExclude = exclusion;
        }

        public void effect()
        {
        }

        public static string gameDataUploadHeader()
        {
            var sb = new GameDataStringBuilder();
            sb.addFieldData("name");
            sb.addFieldData("text");
            return sb.toString();
        }

        public string gameDataUploadData()
        {
            var sb = new GameDataStringBuilder();
            sb.addFieldData(name);
            sb.addFieldData(description);
            return sb.toString();
        }
    }
}