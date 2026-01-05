using System.Collections.Generic;

namespace MarbleHero
{
    public abstract partial class ADungeon
    {
        public static List<AGameEffect> topLevelEffects = new();
        public static List<AGameEffect> topLevelEffectsQueue = new();
        public static List<AGameEffect> effectList = new();
        public static List<AGameEffect> effectsQueue = new();

        protected static void updateTopLevelEffects(float dt)
        {
            for (var i = 0; i < topLevelEffects.Count;)
            {
                var e = topLevelEffects[i];
                if (e.update(dt))
                {
                    topLevelEffects.RemoveAt(i);
                    UN_CLASS(e);
                }
                else
                    i++;
            }
        }

        protected static void clearTopLevelEffects()
        {
            for (var i = topLevelEffects.Count - 1; i >= 0; i--)
            {
                var e = topLevelEffects[i];
                topLevelEffects.RemoveAt(i);
                UN_CLASS(e);
            }
            
            topLevelEffectsQueue.Clear();
        }

        protected static void updateEffects(float dt)
        {
            for (var i = 0; i < effectList.Count;)
            {
                var e = effectList[i];
                if (e.update(dt))
                {
                    effectList.RemoveAt(i);
                    UN_CLASS(e);
                }
                else
                    i++;
            }
        }

        protected static void clearEffects()
        {
            for (var i = effectList.Count - 1; i >= 0; i--)
            {
                var e = effectList[i];
                effectList.RemoveAt(i);
                UN_CLASS(e);
            }
            
            effectsQueue.Clear();
        }
    }
}