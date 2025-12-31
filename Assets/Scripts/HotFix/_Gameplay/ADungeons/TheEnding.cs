using System.Collections.Generic;
using MoreMountains.AutoBattleEngine.Gameplay.Saves;

namespace MarbleHero
{
    public class TheEnding : ADungeon
    {
        public TheEnding(string name, string levelId, APlayer p, List<string> newSpecialOneTimeEventList) : base(DungeonData.Get("Exordium"), p, newSpecialOneTimeEventList)
        {
        }

        public TheEnding(string name, APlayer p, SaveFile saveFile) : base(DungeonData.Get("Exordium"), p, saveFile)
        {
        }

        public override void Initialize(int seed)
        {
            base.Initialize(seed);
        }
    }
}