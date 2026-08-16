using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/PlayerStatModDef")]
    public sealed class PlayerStatDef : StatDef
    {
        public Character.Stat stat;

        void OnValidate()
        {
            statKey = stat.Key();
        }
    }
}