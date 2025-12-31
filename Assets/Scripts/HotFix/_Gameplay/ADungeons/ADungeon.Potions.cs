using MoreMountains.AutoBattleEngine.Gameplay.Helpers;

namespace MarbleHero
{
    public partial class ADungeon
    {
        public void initializePotions()
        {
            PotionHelper.initialize(player.chosenClass);
        }
    }
}