using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Vector2 extensions
    /// </summary>
    public static class MMVector2Extensions
    {
        /// <summary>
        /// Rotates a vector2 by angleInDegrees
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="angleInDegrees"></param>
        /// <returns></returns>
        public static Vector2 MMRotate(this Vector2 vector, float angleInDegrees)
        {
            float sin = Mathf.Sin(angleInDegrees * Mathf.Deg2Rad);
            float cos = Mathf.Cos(angleInDegrees * Mathf.Deg2Rad);
            float tx = vector.x;
            float ty = vector.y;
            vector.x = (cos * tx) - (sin * ty);
            vector.y = (sin * tx) + (cos * ty);
            return vector;
        }

        /// <summary>
        /// Sets the X part of a Vector2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static Vector2 MMSetX(this Vector2 vector, float newValue)
        {
            vector.x = newValue;
            return vector;
        }

        /// <summary>
        /// Sets the Y part of a Vector2
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static Vector2 MMSetY(this Vector2 vector, float newValue)
        {
            vector.y = newValue;
            return vector;
        }

        public static Vector2 Clamp(this Vector2 vector, float min, float max)
        {
            float len2 = vector.sqrMagnitude;
            if (len2 == 0.0F)
                return vector;
            float max2 = max * max;
            if (len2 > max2)
                return vector * Mathf.Sqrt(max2 / len2);
            float min2 = min * min;
            if (len2 < min2)
                return vector * Mathf.Sqrt(min2 / len2);
            return vector;
        }

        public static float Angle(this Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) *Mathf.Rad2Deg;
            if (angle < 0.0F)
                angle += 360.0F;
            return angle;
        }

        public static Vector2 setAngle(this Vector2 vector, float degrees)
        {
            return vector.setAngleRad(degrees * Mathf.Deg2Rad);
        }

        public static Vector2 setAngleRad(this Vector2 vector, float radians)
        {
            vector.x = vector.magnitude;
            vector.y = 0F;
            return vector.rotateRad(radians);
        }

        public static Vector2 rotateRad(this Vector2 vector, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float newX = vector.x * cos - vector.y * sin;
            float newY = vector.x * sin + vector.y * cos;
            vector.x = newX;
            vector.y = newY;
            return vector;
        }
    }
}