using System.Collections.Generic;

namespace MoreMountains
{
    public class TheBeyond : ADungeon
    {
        public TheBeyond(string name, string levelId, APlayer p, List<string> newSpecialOneTimeEventList) : base("Exordium", "Exordium", p, newSpecialOneTimeEventList)
        {
        }

        public TheBeyond(string name, APlayer p, SaveFile saveFile) : base("Exordium", p, saveFile)
        {
        }

        public override void Initialize(int seed)
        {
            base.Initialize(seed);
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