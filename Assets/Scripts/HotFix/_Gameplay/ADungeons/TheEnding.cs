using System.Collections.Generic;

namespace MoreMountains
{
    public class TheEnding : ADungeon
    {
        public TheEnding(string name, string levelId, APlayer p, List<string> newSpecialOneTimeEventList) : base("Exordium", "Exordium", p, newSpecialOneTimeEventList)
        {
            screen = CurrentScreen.MAP;
            isScreenUp = true;
        }

        public TheEnding(string name, APlayer p, SaveFile saveFile) : base("Exordium", p, saveFile)
        {
        }

        public override void initialize()
        {
            base.initialize();
        }

        protected override void initializeEventList()
        {
        }

        protected override void initializeEventImg()
        {
        }

        protected override void initializeShrineList()
        {
        }

        protected override void generateMonsters()
        {
        }

        protected override void generateWeakEnemies(int paramInt)
        {
        }

        protected override void generateStrongEnemies(int paramInt)
        {
        }

        protected override void generateElites(int paramInt)
        {
        }

        protected override void initializeBoss()
        {
        }
    }
}