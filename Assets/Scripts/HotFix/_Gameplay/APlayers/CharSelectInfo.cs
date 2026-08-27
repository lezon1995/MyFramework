using System.Collections.Generic;

namespace MoreMountains
{
    public class CharSelectInfo
    {
        public PlayerDef playerDef;
        public string name;
        public string flavorText;
        public int gold;
        public int floorNum;
        public string levelName;
        public long saveDate;
        public List<BallItem> balls = new();
        public List<RelicDef> relics = new();
        public bool resumeGame;
        public int difficulty;
    }
}