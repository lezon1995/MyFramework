using System.Collections.Generic;
using MoreMountains.AutoBattleEngine.Gameplay.Vfx;

namespace MarbleHero
{
    public abstract partial class ADungeon
    {
        public static List<AGameEffect> topLevelEffects = new();
        public static List<AGameEffect> topLevelEffectsQueue = new();
        public static List<AGameEffect> effectList = new();
        public static List<AGameEffect> effectsQueue = new();
    }
}