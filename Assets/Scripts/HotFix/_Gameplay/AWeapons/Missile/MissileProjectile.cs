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
        Vector2 _p0; // 起点（角色身后）
        Vector2 _p2 => _target.position; // 终点（目标附近，含散布）
        float _arcHeight; // 弧线高度（含抖动 + 交错）
        float _flightDuration; // 飞行时间（秒）

        float _elapsed;
        bool _exploded;

        /// <summary>
        /// 由 IcathianRainSkill 在发射时调用一次。
        /// </summary>
        public void Launch(MissileProjectileWeaponDefinition def, Vector2 spawn, float arcHeight, float flightDuration)
        {
            _def = def;
            _p0 = spawn;
            _arcHeight = arcHeight;
            _flightDuration = Mathf.Max(0.01f, flightDuration);

            _elapsed = 0f;
            _exploded = false;

            // 设置贴图
            EnsureSpriteRenderer();
            _spriteRenderer.color = _def.MissileColor;
            transform.localScale = Vector3.one * _def.MissileScale;

            // 初始位置 = 起点
            transform.position = _p0;

            // 启动朝向：沿 P0→P2 方向
            UpdateRotation();
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

            // 二次贝塞尔曲线：P0 → P1 (上方控制点) → P2
            // 控制点取 P0 和 P2 的中点，再向上抬 arcHeight
            var p1 = Vector2.Lerp(_p0, _p2, 0.5f) + Vector2.up * _arcHeight;
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
            // 启动时朝向 P0→P2 方向
            var dir = (Vector2)(_p2 - _p0);
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
    }
}