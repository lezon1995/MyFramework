using MoreMountains.AutoBattleEngine.Gameplay.Actions;

namespace MarbleHero
{
    public abstract partial class ADungeon
    {
        public static GameActionManager actionManager => ActionManager;
        public static GameActionManager ActionManager = new();
    }
}