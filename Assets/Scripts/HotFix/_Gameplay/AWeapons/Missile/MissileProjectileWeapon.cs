using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 卡莎 Q 技能「艾卡西亚暴雨」武器层。
    /// 用法：把组件挂在施法角色上,配置好 <see cref="Definition"/>,
    /// 然后通过武器的常规输入(CharacterHandleWeapon)或直接调用 WeaponInputStart 触发。
    ///
    /// 飞行逻辑(在 <see cref="MissileProjectile"/>):
    /// - 飞弹从角色中心点 P0 直接出现;
    /// - 控制点 P1 = (target→caster) 方向的反向延伸 + ±OutgoingYawSpread 随机偏角 + 垂直抬升;
    /// - 终点 P2 = target.position + ImpactScatterRadius 随机散布。
    /// 这样飞弹会先沿"目标反方向"斜飞出去,再绕回来命中目标,呈现 LoL 卡莎 Q 的轨迹。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MissileProjectileWeapon : ProjectileWeapon
    {
        [Header("Configuration")]
        [Tooltip("技能参数定义。必须赋值,否则不会发射飞弹。")]
        public MissileProjectileWeaponDefinition Definition;

        [Tooltip("Living Weapon 升级档开关。打开后会发射 UpgradedMissileCount 枚飞弹。")]
        public bool Upgraded;

        Ball _ball;

        public void SetBallOwner(Ball ball)
        {
            _ball = ball;
        }
        
        public override GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
        {
            if (Definition == null || Owner == null)
                return null;

            // 让基类负责对象池/位置/方向/owner/target/damage 的初始化
            var go = base.SpawnProjectile(spawnPosition, projectileIndex, totalProjectiles, triggerObjectActivation);
            if (go == null)
                return null;

            if (!go.TryGetComponent<MissileProjectile>(out var missile))
                return go;

            // === 1. 先把"不变"的随机分量算好 ===

            // arcHeightOffset = jitter + interleave
            var jitter = Random.Range(-Definition.ArcHeightJitter, Definition.ArcHeightJitter);
            float interleave = 0f;
            if (totalProjectiles > 1)
            {
                var t = (float)projectileIndex / (totalProjectiles - 1);
                interleave = Mathf.Lerp(-1f, 1f, t) * Definition.ArcInterleaveAmplitude;
            }
            var arcHeightOffset = jitter + interleave;

            // === 2. 算 P1 ===
            // P1 完全由目标/角色相对位置 + 随机偏角决定,跟飞弹本身无关。
            // 武器层先用静态方法算一次,用来估算弧线长度。
            var p0 = (Vector2)Owner.transform.position;
            var p2 = (Vector2)missile.GetTargetPosition();
            var p1 = MissileProjectile.ComputeOutgoingControlPoint(
                casterPos: p0,
                targetPos: p2,
                outgoingDistance: Definition.OutgoingDistance,
                outgoingYawSpreadMax: Definition.OutgoingYawSpreadMax,
                outgoingYawSpreadMin: Definition.OutgoingYawSpreadMin,
                outgoingVerticalLift: Definition.OutgoingVerticalLift);

            // === 3. 算飞行时间 ===
            var flightDuration = Definition.ResolveFlightDuration(p0, p2, p1);

            // === 4. Launch ===
            missile.Launch(
                def: Definition,
                caster: Owner,
                ball: _ball,
                arcHeightOffset: arcHeightOffset,
                p1: p1,
                flightDuration: flightDuration);

            return go;
        }
    }
}