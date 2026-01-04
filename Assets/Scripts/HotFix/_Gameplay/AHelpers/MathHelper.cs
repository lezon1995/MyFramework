using UnityEngine;

namespace MarbleHero
{
    public static class MathHelper
    {
        public static Vector2 cardLerpSnap(Vector2 start, Vector2 target, float dt, float speed)
        {
            if (start != target)
            {
                start.x = MathUtils.lerp(start.x, target.x, dt * 6.0F * speed);
                start.y = MathUtils.lerp(start.y, target.y, dt * 6.0F * speed);
                if (Mathf.Abs(start.x - target.x) < Settings.CARD_SNAP_THRESHOLD)
                    start.x = target.x;

                if (Mathf.Abs(start.y - target.y) < Settings.CARD_SNAP_THRESHOLD)
                    start.y = target.y;
            }

            return start;
        }

        public static float cardLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 6.0F);
                if (Mathf.Abs(startX - targetX) < Settings.CARD_SNAP_THRESHOLD)
                    startX = targetX;
            }

            return startX;
        }

        public static float cardScaleLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 7.5F);
                if (Mathf.Abs(startX - targetX) < 0.003F)
                    startX = targetX;
            }

            return startX;
        }

        public static float cardAlphaLerpSnap(float start, float target, float dt)
        {
            if (start != target)
            {
                start = MathUtils.lerp(start, target, dt * 7.5F);
                if (Mathf.Abs(start - target) < 0.003F)
                    start = target;
            }

            return start;
        }

        public static float uiLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 9.0F);
                if (Mathf.Abs(startX - targetX) < Settings.UI_SNAP_THRESHOLD)
                    startX = targetX;
            }

            return startX;
        }

        public static float orbLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 6.0F);
                if (Mathf.Abs(startX - targetX) < Settings.UI_SNAP_THRESHOLD)
                    startX = targetX;
            }

            return startX;
        }

        public static float mouseLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 20.0F);
                if (Mathf.Abs(startX - targetX) < Settings.UI_SNAP_THRESHOLD)
                    startX = targetX;
            }

            return startX;
        }

        public static float scaleLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 8.0F);
                if (Mathf.Abs(startX - targetX) < 0.003F)
                    startX = targetX;
            }

            return startX;
        }

        public static float fadeLerpSnap(float start, float target, float dt)
        {
            if (start != target)
            {
                start = MathUtils.lerp(start, target, dt * 12.0F);
                if (Mathf.Abs(start - target) < 0.01F)
                    start = target;
            }

            return start;
        }

        public static float popLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 8.0F);
                if (Mathf.Abs(startX - targetX) < 0.003F)
                    startX = targetX;
            }

            return startX;
        }

        public static float angleLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 12.0F);
                if (Mathf.Abs(startX - targetX) < 0.003F)
                    startX = targetX;
            }

            return startX;
        }

        public static float slowColorLerpSnap(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 3.0F);
                if (Mathf.Abs(startX - targetX) < 0.01F)
                    startX = targetX;
            }

            return startX;
        }

        public static float scrollSnapLerpSpeed(float startX, float targetX, float dt)
        {
            if (startX != targetX)
            {
                startX = MathUtils.lerp(startX, targetX, dt * 10.0F);
                if (Mathf.Abs(startX - targetX) < Settings.UI_SNAP_THRESHOLD)
                    startX = targetX;
            }

            return startX;
        }

        public static float valueFromPercentBetween(float min, float max, float percent)
        {
            float diff = max - min;
            return min + diff * percent;
        }

        public static float percentFromValueBetween(float min, float max, float value)
        {
            float diff = max - min;
            float offset = value - min;
            return offset / diff;
        }
    }
}