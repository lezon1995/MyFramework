using UnityEngine;

namespace MoreMountains.Gravity
{
    /// <summary>
    /// 行星引力源：只定义引力范围和坠落时间区间。
    /// </summary>
    public class GravitySource : MonoBehaviour
    {
        [Header("引力范围")]
        [Tooltip("引力生效的范围半径。物体进入此半径内才开始被吸引。")]
        public float gravityRange = 15f;

        [Header("坠落时间区间（秒）")]
        [Tooltip("夹角为 0°（垂直射入）时的最小坠落时间")]
        public float minDuration = 1f;

        [Tooltip("夹角为 90°（切向进入）时的最大坠落时间")]
        public float maxDuration = 5f;

        public Vector3 Position => transform.position;

        /// <summary>物体是否在引力范围内</summary>
        public bool IsWithinRange(Vector3 position)
        {
            return (position - transform.position).sqrMagnitude <= gravityRange * gravityRange;
        }
    }
}
