using System.Collections.Generic;

namespace MoreMountains
{
    public class TheCity : ADungeon
    {
        public TheCity(string name, string levelId, List<string> newSpecialOneTimeEventList) : base("Exordium", "Exordium", newSpecialOneTimeEventList)
        {
            screen = CurrentScreen.MAP;
            isScreenUp = true;
        }

        public TheCity(string name, SaveFile saveFile) : base("Exordium", saveFile)
        {
        }

        protected override void initializePhases()
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