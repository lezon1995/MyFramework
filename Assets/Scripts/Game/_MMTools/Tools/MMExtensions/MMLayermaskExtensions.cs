using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Layermask Extensions
    /// </summary>
    public static class LayermaskExtensions
    {
        /// <summary>
        /// Returns bool if layer is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static bool MMContains(this LayerMask mask, int layer)
        {
            return (mask.value & (1 << layer)) > 0;
        }

        /// <summary>
        /// Returns true if gameObject is within layermask
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool MMContains(this LayerMask mask, GameObject obj)
        {
            return (mask.value & (1 << obj.layer)) > 0;
        }
    }
}