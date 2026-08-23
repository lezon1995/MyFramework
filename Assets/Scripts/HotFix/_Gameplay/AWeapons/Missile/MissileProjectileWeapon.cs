using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 卡莎 Q 技能「艾卡西亚暴雨」施法者身上的 MonoBehaviour 控制器。
    /// 用法：挂在施法角色（一般是 Player）的某个 GameObject 上，并在合适时机调用
    /// <see cref="Cast"/> / <see cref="Cast(Vector2)"/>。
    ///
    /// 设计要点：
    /// - 纯 MonoBehaviour：内部生成的是裸 GameObject + IcathianRainMissile，不依赖 Ball 体系；
    /// - 自动锁定施法瞬间最近的敌人（LoL 行为），多枚飞弹全部冲向同一个目标；
    /// - 飞弹在 CastDuration 内交错发射，弧线高度按序号交错，形成「若干枚飞弹从身后飞出，
    ///   在空中以交错弧线的轨迹命中敌人」的视觉效果；
    /// - 不需要编辑 BallDef、不需要管 BallPool、不需要管 DamageOnTouch，
    ///   伤害由 IcathianRainMissile 自己调用 Brick.Health.Damage 完成。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MissileProjectileWeapon : ProjectileWeapon
    {
        [Header("Configuration")]
        [Tooltip("技能参数定义。必须赋值，否则 Cast 不会执行。")]
        public MissileProjectileWeaponDefinition Definition;

        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            if (Definition == null || Owner == null) 
                return null;
        
            var go = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);
            go.TryGetComponent<MissileProjectile>(out var  missile);

            var spawnBase = Owner.transform.position + -missile.Direction * Definition.SpawnOffsetBehind.y;

            // 在水平/垂直方向上散布（圆内随机点），但保持主要落在「身后」一侧
            var scatter = Random.insideUnitCircle * Definition.SpawnScatterRadius;
            var spawnPos = spawnBase + (Vector3)scatter;

            // 3. 计算弧线高度：基准 + 抖动 + 交错幅度
            var arcBase = Definition.ArcHeight + Random.Range(-Definition.ArcHeightJitter, Definition.ArcHeightJitter);
            // 交错：把多枚飞弹均匀分布在 [-amplitude, +amplitude] 上
            // 当只有 1 枚时，amplitude 不影响
            float interleave = 0f;
            if (totalProjectiles > 1)
            {
                var t = (float)projectileIndex / (totalProjectiles - 1); // 0..1
                interleave = Mathf.Lerp(-1f, 1f, t) * Definition.ArcInterleaveAmplitude;
            }

            var finalArcHeight = Mathf.Max(0.1f, arcBase + interleave);

            missile.Launch(
                def: Definition,
                spawn: spawnPos,
                arcHeight: finalArcHeight,
                flightDuration: Definition.MissileFlightDuration);

            return go;
        }
    }
}