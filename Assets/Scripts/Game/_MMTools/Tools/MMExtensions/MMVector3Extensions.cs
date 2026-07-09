using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Vector3 Extensions
    /// </summary>
    public static class MMVector3Extensions
    {
        public static Vector3 X(this Vector3 v, float value)
        {
            v.x = value;
            return v;
        }

        public static Vector3 Y(this Vector3 v, float value)
        {
            v.y = value;
            return v;
        }

        public static Vector3 Z(this Vector3 v, float value)
        {
            v.z = value;
            return v;
        }

        /// <summary>
        /// Inverts a vector
        /// </summary>
        /// <param name="newValue"></param>
        /// <returns></returns>
        public static Vector3 MMInvert(this Vector3 newValue)
        {
            return new Vector3
            (
                1.0f / newValue.x,
                1.0f / newValue.y,
                1.0f / newValue.z
            );
        }

        /// <summary>
        /// Projects a vector on another
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="projectedVector"></param>
        /// <returns></returns>
        public static Vector3 MMProject(this Vector3 vector, Vector3 projectedVector)
        {
            float _dot = Vector3.Dot(vector, projectedVector);
            return _dot * projectedVector;
        }

        /// <summary>
        /// Rejects a vector on another
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="rejectedVector"></param>
        /// <returns></returns>
        public static Vector3 MMReject(this Vector3 vector, Vector3 rejectedVector)
        {
            return vector - vector.MMProject(rejectedVector);
        }

        /// <summary>
        /// Rounds all components of a vector
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Round(this Vector3 vector)
        {
            vector.x = Mathf.Round(vector.x);
            vector.y = Mathf.Round(vector.y);
            vector.z = Mathf.Round(vector.z);
            return vector;
        }
    }
}