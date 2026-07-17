using System;
using UnityEngine;

namespace MoreMountains
{
    [Serializable]
    public struct MapDot
    {
        public Vector2 pos;
        public float angle;
        static int RAW_W = 16;
        static float DIST_JITTER = 4.0F * Settings.scale;
        static float OFFSET_Y = 172.0F * Settings.scale;

        public MapDot(float x, float y, float rotation, bool jitter)
        {
            if (jitter)
            {
                pos.x = x + MathUtils.random(-DIST_JITTER, DIST_JITTER);
                pos.y = y + MathUtils.random(-DIST_JITTER, DIST_JITTER);
                angle = rotation + MathUtils.random(-20.0F, 20.0F);
            }
            else
            {
                pos.x = x;
                pos.y = y;
                angle = rotation;
            }
        }

        // public void render(SpriteBatch sb)
        // {
        //     sb.draw(ImageMaster.MAP_DOT_1, this.x - 8.0F, this.y - 8.0F + DungeonMapScreen.offsetY + OFFSET_Y, 8.0F, 8.0F, RAW_W, RAW_W, Settings.scale, Settings.scale, this.rotation, 0, 0, RAW_W, RAW_W, false, false);
        // }
    }
}