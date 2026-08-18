using UnityEngine;

namespace MoreMountains
{
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/BrickDef")]
    public sealed class BrickDef : ScriptableObject
    {
        public SpawnEnemyType Type;
        public Vector2Int Size = new(1, 1);
        public Sprite UnitIcon;
        public Sprite BlockIcon;
        public BrickStatsTemplate StatsTemplate;

        public float BonusHealthPerWave = 5;
        public float BonusDamagePerWave = 0.5F;
        public float BonusMoveSpeedPerWave = 10F;
        public float BonusArmorPerWave = 3F;
        public float BonusKnockbackResistPerWave = 0.03F;
    }
}