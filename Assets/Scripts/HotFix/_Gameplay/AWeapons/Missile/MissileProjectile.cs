using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 卡莎 Q 的单枚飞弹。
    /// - 沿二次贝塞尔曲线从生成点飞到目标点；
    /// - 飞行过程中始终面向当前速度方向；
    /// - 到达目标点时按 ImpactHitRadius 检测 Brick 层，对命中目标施加伤害并播特效；
    /// - 超过 MaxLifetime 仍未命中则自毁，避免永久飞行。
    ///
    /// 纯 MonoBehaviour：不依赖 Ball 体系、不依赖物理 Rigidbody，靠 Update 自己推进。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MissileProjectile : Projectile
    {
        // ====== 注入字段（Launch 时填充） ======
        MissileProjectileWeaponDefinition _def;
        Character _caster;
        Vector2 _p0; // 起点（角色身上）
        Vector2 _p1; // 控制点：target→caster 反向 + 随机偏角 + 抬升
        Vector2 _impactScatter; // 命中点散布（生成时随机一次后锁定）
        float _arcHeightOffset; // 弧线高度偏移（jitter + interleave 叠加）
        float _flightDuration; // 飞行时间（秒）

        // 终点（每帧实时读 _target.position,叠加一次性散布）
        Vector2 _p2 => (Vector2)_target.position + _impactScatter;

        float _elapsed;
        bool _exploded;

        protected override void OnStatsSet()
        {
            var damageOnTouch = _damageOnTouch;
            damageOnTouch.DmgGetter = () =>
            {
                if (damageOnTouch.Source && damageOnTouch.Source.GetStat(Character.Stat.AP, out var stat))
                {
                    return Dmg.AP((int)stat.Value + damageOnTouch.Dmg.Value);
                }

                return damageOnTouch.Dmg;
            };
        }

        /// <summary>
        /// 由 MissileProjectileWeapon 在生成飞弹后调用一次。
        /// </summary>
        /// <param name="def">技能定义</param>
        /// <param name="caster">施法者（用于决定 P1 方向：目标→caster 反向 + 随机偏角）</param>
        /// <param name="arcHeightOffset">本枚飞弹的弧线高度偏移量（已由 Weapon 层算 jitter + interleave）</param>
        /// <param name="p1">Weapon 层在 spawn 时已经按相同算法算好的 P1 控制点,保证 duration 和飞行轨迹用的 P1 完全一致</param>
        /// <param name="flightDuration">飞行时间</param>
        public void Launch(
            MissileProjectileWeaponDefinition def,
            Character caster,
            float arcHeightOffset,
            Vector2 p1,
            float flightDuration)
        {
            _def = def;
            _caster = caster;
            _arcHeightOffset = arcHeightOffset;
            _p1 = p1;
            _flightDuration = Mathf.Max(0.01f, flightDuration);

            _elapsed = 0f;
            _exploded = false;

            // 命中点散布一次性随机
            _impactScatter = Random.insideUnitCircle * (_def != null ? _def.ImpactScatterRadius : 0f);

            // 计算 P0（生成点）= 施法者位置
            _p0 = (Vector2)(_caster ? _caster.transform.position : transform.position);

            // 设置贴图
            EnsureSpriteRenderer();
            if (_spriteRenderer)
                _spriteRenderer.color = _def.MissileColor;
            transform.localScale = Vector3.one * _def.MissileScale;

            // 初始位置 = P0
            transform.position = _p0;

            // 启动朝向：沿 P0→P1 方向（让飞弹的初始朝向就是「准备往后飞」的方向）
            UpdateRotation();
        }

        /// <summary>
        /// 获取飞弹当前锁定的目标位置,供 Weapon 层在 Speed 模式下估算弧线长度。
        /// 注意：不含 ImpactScatter（散布是在飞弹 Launch 时随机一次的），
        /// 所以用这个估算的弧长会比实际略短,Resolution 里给了一个 1.1 系数来补偿。
        /// </summary>
        public Vector2 GetTargetPosition() =>
            _target ? (Vector2)_target.position : _p0;

                /// <summary>
        /// 根据 (target→caster) 反向 + 随机偏转 + 抬升,计算 P1 控制点。
        /// 抽成 public static 方法,让武器层在 spawn 时也能复用,保证 duration 计算用到的 P1
        /// 和飞弹飞行用到的 P1 完全一致。
        /// </summary>
        public static Vector2 ComputeOutgoingControlPoint(
            Vector2 casterPos,
            Vector2 targetPos,
            float outgoingDistance,
            float outgoingYawSpreadMax,
            float outgoingYawSpreadMin,
            float outgoingVerticalLift)
        {
            // 基础方向：target → caster 的反向延长线
            var dir = casterPos - targetPos;
            var len = dir.magnitude;
            if (len < 0.0001f)
            {
                dir = Vector2.up;
                len = 1f;
            }
            else
            {
                dir /= len;
            }

            // 左右 ±OutgoingYawSpread 随机偏转
            var yawMin = outgoingYawSpreadMin;
            var yawMax = outgoingYawSpreadMax;
            float yawDeg;
            if (randomHit(0.5F))
                yawDeg = Random.Range(-yawMax, -yawMin);
            else
                yawDeg = Random.Range(yawMin, yawMax);

            dir = RotateVector2(dir, yawDeg);

            // 反向延伸 OutgoingDistance 距离
            var p1 = casterPos + dir * outgoingDistance;

            // 垂直抬升
            p1 += Vector2.up * outgoingVerticalLift;

            return p1;
        }

        Vector2 ComputeOutgoingControlPointForInstance()
        {
            if (_def == null)
                return _p0 + Vector2.up;

            var targetPos = (Vector2)(_target ? _target.position : transform.position);
            return ComputeOutgoingControlPoint(
                casterPos: _p0,
                targetPos: targetPos,
                outgoingDistance: _def.OutgoingDistance,
                outgoingYawSpreadMax: _def.OutgoingYawSpreadMax,
                outgoingYawSpreadMin: _def.OutgoingYawSpreadMin,
                outgoingVerticalLift: _def.OutgoingVerticalLift);
        }

        static Vector2 RotateVector2(Vector2 v, float degrees)
        {
            var rad = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(rad);
            var sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        void EnsureSpriteRenderer()
        {
            if (_spriteRenderer)
                return;

            if (!TryGetComponent(out _spriteRenderer))
                _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        public override void Movement(float dt)
        {
            if (_target == null)
            {
                _health.Kill();
                return;
            }

            if (_exploded || _def == null)
                return;

            _elapsed += dt;

            // 安全网：超过 MaxLifetime 直接销毁
            if (_elapsed >= _def.MaxLifetime)
            {
                Explode();
                return;
            }

            // 计算归一化飞行进度
            var t = Mathf.Clamp01(_elapsed / _flightDuration);

            // 二次贝塞尔曲线：P0 → P1 → P2
            // P1 在 Launch 时已经按 (target→caster) 反向 + 随机偏转 + 抬升算好。
            // 这里再叠加 arcHeightOffset（jitter + interleave）,让相邻飞弹的弧线不完全一样。
            var p1 = _p1 + Vector2.up * _arcHeightOffset;
            var pos = Bezier2(_p0, p1, _p2, t);
            transform.position = pos;

            // 朝向当前运动方向（首尾用切线近似，避免端点速度为 0 时抖动）
            var dt2 = 0.01f;
            var ahead = Mathf.Min(t + dt2, 1f);
            var aheadPos = Bezier2(_p0, p1, _p2, ahead);
            UpdateRotationByVelocity(pos, aheadPos);

            // 到达终点：引爆
            if (t >= 1f)
                Explode();
        }

        static Vector2 Bezier2(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            var u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        void UpdateRotationByVelocity(Vector2 pos, Vector2 aheadPos)
        {
            var dir = aheadPos - pos;
            if (dir.sqrMagnitude < 0.0001f)
            {
                UpdateRotation(); // 退化情况：接近终点时使用上一次角度
                return;
            }

            // 2D 风格：使用 Atan2 求出 Z 旋转
            var angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        void UpdateRotation()
        {
            // 启动朝向：沿 P0→P1 方向(也就是「准备往身后飞出去」的方向)
            var p1 = _p1 + Vector2.up * _arcHeightOffset;
            var dir = p1 - _p0;
            if (dir.sqrMagnitude < 0.0001f)
                return;

            var angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angleDeg);
        }

        void Explode()
        {
            if (_exploded)
                return;

            _exploded = true;

            TryDealDamage();
            PlayImpactEffects();
        }

        void TryDealDamage()
        {
            if (_targetHealth == null || _targetHealth.IsDead())
            {
                _health.Kill();
            }
            else
            {
                _damageOnTouch.SetDamageScriptDirection(transform.right);
                _damageOnTouch.ForceColliding(_target.gameObject);
            }
        }

        void PlayImpactEffects()
        {
            fx?.play(_def.ImpactVfxKey, _p2, _def.ImpactVfxLifetime);

            // SFX
            if (_def && !string.IsNullOrEmpty(_def.ImpactSoundKey))
            {
                sound?.play(_def.ImpactSoundKey);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            TryClearTrails();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            TryClearTrails();
        }

        protected override void FaceMovementDirection(Vector3 newDirection)
        {
        }
    }
}