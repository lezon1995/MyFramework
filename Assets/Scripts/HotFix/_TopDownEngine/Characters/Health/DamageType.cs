using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A scriptable object you can create assets from, to identify damage types
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/TopDownEngine/DamageType", fileName = "DamageType")]
    public class DamageType : ScriptableObject
    {
        public enum Modes
        {
            Base,
            Typed
        }
    }
}