using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/BallStatModDef")]
    public sealed class BallStatDef : StatDef
    {
        public Ball.Stat stat;

        void OnValidate()
        {
            statKey = stat.Key();
        }
    }
}