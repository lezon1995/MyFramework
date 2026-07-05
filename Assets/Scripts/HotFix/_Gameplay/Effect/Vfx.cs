using UnityEngine;

namespace MarbleHero
{
    public class Vfx
    {
        static int idGenerator;
        string path;
        int id;
        float fxLifeTime;

        public Vfx(string _path)
        {
            id = ++idGenerator;
            path = _path;
        }

        public int play(Vector3 pos, float lifeTime = 0F)
        {
            var effect = mEffectManager.createEffect(path, pos, lifeTime);
            if (lifeTime == 0F)
            {
                var maxLifeTime = effect.getMaxLifeTime();
                if (maxLifeTime < float.MaxValue)
                {
                    effect.setLifeTime(maxLifeTime);
                }
            }

            return id;
        }
    }
}