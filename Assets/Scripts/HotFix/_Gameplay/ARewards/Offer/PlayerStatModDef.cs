using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/PlayerStatModDef")]
    public sealed class PlayerStatModDef : StatModDef
    {
        public Character.Stat stat;

        void OnValidate()
        {
            statKey = stat.Key();
        }
    }
}