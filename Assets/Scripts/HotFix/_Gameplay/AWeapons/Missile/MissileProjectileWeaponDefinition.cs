using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 配置卡莎 Q 技能「艾卡西亚暴雨」的参数。
    /// 通过 ScriptableObject 配置，方便在 Inspector 里调参并在不同英雄/等级复用同一逻辑。
    /// </summary>
    [CreateAssetMenu(fileName = "MissileProjectileWeaponDefinition", menuName = "MoreMountains/MissileProjectileWeaponDefinition", order = 0)]
    public sealed class MissileProjectileWeaponDefinition : ScriptableObject
    {
        [Header("Cast")]
        [Tooltip("施法总时长（秒）。所有飞弹会在这段时间内交错发射完毕。")]
        [Min(0.05f)]
        public float CastDuration = 0.4f;

        [Tooltip("每波发射多少枚飞弹。LoL 中 Q = 6，升级后 = 12。")]
        [Min(1)]
        public int MissileCount = 6;

        [Tooltip("升级后的飞弹数量（Living Weapon）。0 表示禁用升级档位。")]
        [Min(0)]
        public int UpgradedMissileCount = 12;

        [Header("Missile Flight")]
        [Tooltip("飞弹从出现到命中的飞行时间（秒）。")]
        [Min(0.05f)]
        public float MissileFlightDuration = 0.75f;

        [Tooltip("飞弹生成点距离角色身后的偏移（米）。生成时取反朝向，所以飞弹从身后飞出。")]
        public Vector2 SpawnOffsetBehind = new(0f, 0.5f);

        [Tooltip("飞弹生成点相对于角色中心的随机散布半径（米），让多枚飞弹不会从完全相同的点出现。")]
        [Min(0f)]
        public float SpawnScatterRadius = 0.6f;

        [Tooltip("弧线高度（米）。值越大弧线越明显、越像 LoL 的抛物线感。")]
        [Min(0f)]
        public float ArcHeight = 2.5f;

        [Tooltip("每枚飞弹的弧线高度随机抖动幅度（米），避免视觉上完全雷同。")]
        [Min(0f)]
        public float ArcHeightJitter = 0.5f;

        [Tooltip("相邻飞弹弧线高度交错幅度（米），让多枚飞弹在空中呈交错的弧形轨迹。")]
        [Min(0f)]
        public float ArcInterleaveAmplitude = 1.2f;

        [Header("Visuals")]
        [Tooltip("飞弹精灵缩放。")]
        public float MissileScale = 1.0f;

        [Tooltip("飞弹颜色。")]
        public Color MissileColor = new(0.45f, 0.85f, 1f, 1f);

        [Header("VFX / SFX")]
        [Tooltip("命中目标时播放的 VFX（FxMaster key）。FxMaster 不存在这个 key 时会安静忽略。")]
        public FxDefine ImpactVfxKey = FxDefine.KAISA_Q_IMPACT;

        [Tooltip("命中目标时播放的 VFX 寿命（秒）。0 表示不显式控制。")]
        [Min(0f)]
        public float ImpactVfxLifetime = 0.5f;

        [Tooltip("命中目标时播放的音效。SoundMaster 不存在这个 key 时会安静忽略。")]
        public string ImpactSoundKey = SoundDefine.BALL_HIT_PASS_THROUGH;

        [Tooltip("施法开始时的音效。")]
        public string CastSoundKey = SoundDefine.KAISA_Q_CAST;

        [Header("Collision")]
        [Tooltip("飞弹碰撞半径（米）。到达目标点时会按这个半径检测 Brick。")]
        [Min(0.01f)]
        public float ImpactHitRadius = 0.6f;

        [Tooltip("飞弹最大存活时间（秒），超过则强制销毁，避免永久飞行。")]
        [Min(0.1f)]
        public float MaxLifetime = 3f;

        /// <summary>
        /// 返回当前档位下应发射的飞弹数。Living Weapon 升级档会被外部开关激活。
        /// </summary>
        public int ResolveMissileCount(bool upgraded)
        {
            return upgraded && UpgradedMissileCount > 0 ? UpgradedMissileCount : MissileCount;
        }
    }
}