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
        [Tooltip("飞弹飞行计时的两种模式：\n" +
                 "• Duration - 用 MissileFlightDuration(秒) 固定飞行时间\n" +
                 "• Speed    - 用 MissileFlightSpeed(米/秒) 按弧线长度反推飞行时间")]
        public FlightTimingMode FlightTiming = FlightTimingMode.Duration;

        [Tooltip("当 FlightTiming = Duration 时生效：飞弹从出现到命中的固定飞行时间（秒）。")]
        [Min(0.05f)]
        public float MissileFlightDuration = 0.75f;

        [Tooltip("当 FlightTiming = Speed 时生效：飞弹的飞行速度（米/秒）。\n" +
                 "实际飞行时间 ≈ 弧线长度 / 此速度。")]
        [Min(0.1f)]
        public float MissileFlightSpeed = 20f;

        [Header("Arc / Outgoing Direction")]
        [Tooltip("控制点沿「目标 → 玩家」方向的反向延伸距离(米)。值越大,飞弹先飞离目标的距离越远,弧线越夸张。")]
        [Min(0f)]
        public float OutgoingDistance = 4f;

        [Tooltip("控制点水平方向上的随机偏转角度(度),会叠加到「目标→玩家」反向上,每枚飞弹独立随机。0 表示不偏转,30 表示左右各 30° 随机。")]
        [Min(0f)]
        public float OutgoingYawSpreadMax = 45f;
        public float OutgoingYawSpreadMin = 30f;

        [Tooltip("控制点的垂直抬升(米),让飞弹的弧线整体偏上,避免贴着地面飞。")]
        public float OutgoingVerticalLift = 1.5f;

        [Tooltip("命中点相对目标的水平随机散布半径(米),保证多枚飞弹不会堆在同一个像素。")]
        [Min(0f)]
        public float ImpactScatterRadius = 0.35f;

        [Tooltip("弧线高度随机抖动幅度(米),避免视觉上完全雷同。")]
        [Min(0f)]
        public float ArcHeightJitter = 0.5f;

        [Tooltip("相邻飞弹弧线高度交错幅度(米),让多枚飞弹在空中呈交错的弧形轨迹。")]
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

        /// <summary>
        /// 根据当前 <see cref="FlightTiming"/> 模式返回本枚飞弹的实际飞行时间（秒）。
        /// </summary>
        /// <param name="caster">施法者位置,用于计算 P0（生成点）</param>
        /// <param name="target">目标位置,用于计算 P2（命中点）</param>
        /// <param name="p1Override">本枚飞弹已经在 Weapon 层算好的 P1 控制点。武器层在算 P1 时通常依赖 random,所以这里由调用方提供而不是在 Definition 里再算一次,保证飞弹飞行用的 P1 跟视觉轨迹完全一致。</param>
        public float ResolveFlightDuration(Vector2 caster, Vector2 target, Vector2 p1Override)
        {
            if (FlightTiming == FlightTimingMode.Speed)
            {
                // 估算弧线长度：用 P0→P1 + P1→P2 的折线作为近似下界
                var len = Vector2.Distance(caster, p1Override) + Vector2.Distance(p1Override, target);
                // 二次贝塞尔曲线比折线略长（约 5~15%），给一个保守系数让速度看起来更"匀"
                var arcLen = len * 1.1f;
                var speed = Mathf.Max(0.01f, MissileFlightSpeed);
                // 兜底：弧线长度几乎为 0 时退化为 MissileFlightDuration,避免 duration 爆炸
                var duration = arcLen > 0.01f ? arcLen / speed : MissileFlightDuration;
                return Mathf.Max(0.05f, duration);
            }

            return Mathf.Max(0.05f, MissileFlightDuration);
        }
    }

    /// <summary>
    /// 飞弹飞行计时的模式。
    /// </summary>
    public enum FlightTimingMode
    {
        /// <summary>固定飞行时间（秒），用 <see cref="MissileProjectileWeaponDefinition.MissileFlightDuration"/>。</summary>
        Duration,

        /// <summary>固定飞行速度（米/秒），用 <see cref="MissileProjectileWeaponDefinition.MissileFlightSpeed"/>，按估算弧线长度反推飞行时间。</summary>
        Speed,
    }
}