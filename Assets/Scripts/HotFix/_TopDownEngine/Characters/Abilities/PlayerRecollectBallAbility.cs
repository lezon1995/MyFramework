using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class PlayerRecollectBallAbility : PlayerAbility
    {
        public LayerMask BallLayerMask = LayerManager.Ball_Mask;
        Dictionary<Collider2D, Ball> canRecollectedBalls = new();

        List<Ball> capturedBalls = new();
        List<CaptureData> capturedList = new();

        public float minCollectDuration = 0.05f;
        public float maxCollectDuration = 0.15f;

        protected override void Initialization()
        {
            base.Initialization();
        }

        public override void OnUpdate(float dt)
        {
            for (var i = capturedList.Count - 1; i >= 0; i--)
            {
                var data = capturedList[i];
                if (HandleFalling(data, dt))
                {
                    capturedList.RemoveAt(i);
                    capturedBalls.Remove(data.ball);
                    UN_CLASS(data);
                }
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!BallLayerMask.MMContains(other.gameObject.layer))
                return;

            if (!other.TryGetComponent(out Ball ball))
                return;

            if (!ball.Player.equalWith(_player))
                return;
            
            if (ball.SourceWeapon is BallGunWeapon)
                return;

            if (capturedBalls.contains(ball))
            {
                // Debug.LogError("已经捕获的球不再捕获");
                return;
            }

            if (canRecollectedBalls.ContainsKey(other) && ball.inUse)
            {
                canRecollectedBalls.Remove(other);
                RecollectBall(ball);
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!BallLayerMask.MMContains(other.gameObject.layer))
                return;

            if (!other.TryGetComponent(out Ball ball))
                return;

            if (!ball.Player.equalWith(_player))
                return;
            
            if (ball.SourceWeapon is BallGunWeapon)
                return;

            if (ball.inUse)
            {
                if (capturedBalls.contains(ball))
                {
                    // Debug.LogError("已经捕获的球离开范围，忽略");
                    return;
                }

                canRecollectedBalls[other] = ball;
            }
        }

        public class CaptureData : ClassObject
        {
            public Ball ball;

            // 坠落参数（Capture 时固定）
            public Vector3 _velocity;
            public Vector3 _entryPos;
            public Vector3 _planetPos;
            public Vector3 _tangent; // 垂直于 entry 半径方向的切向单位向量
            public float _entryRadius; // 进入时的初始距离
            public float _sinAngle; // 入射角的 sin 值
            public float _fallDuration; // 实际坠落时间（秒）
            public float _fallElapsed;
            public float _fallProgress;
            public bool _immediately;

            public override void resetProperty()
            {
                base.resetProperty();
                ball = null;
                _velocity = default;
                _entryPos = default;
                _planetPos = default;
                _tangent = default;
                _entryRadius = 0;
                _sinAngle = 0;
                _fallDuration = 0;
                _fallElapsed = 0;
                _fallProgress = 0;
                _immediately = false;
            }
        }

        /// <summary>
        /// 进入引力范围的瞬间：计算入射角 → 查表得到坠落时间 → 锁定所有轨迹参数。
        /// </summary>
        public void RecollectBall(Ball ball, float collectDuration = 0F, bool immediately = false)
        {
            if (ball.IsRecollecting)
                return;

            ball.IsRecollecting = true;
            ball.SetColliderEnabled(false);
            ball.setEnabled(false);

            CLASS(out CaptureData data);

            data.ball = ball;
            data._velocity = ball.getVelocity();
            data._planetPos = _character.getWorldPosition();

            // 记录进入点
            data._entryPos = ball.getWorldPosition();
            data._entryRadius = (data._entryPos - data._planetPos).magnitude;

            // entry 点法线（从行星中心指向进入点，即引力方向的反向）
            Vector3 normal = (data._entryPos - data._planetPos).normalized;

            // 入射角：velocity 反方向与法线的夹角
            // velocity 方向指向飞行方向；-velocity 指向"来向"
            // 用 atan2 得到有符号角，再取绝对值得到 [0, 180]，映射到 [0, 90]
            float rawAngleDeg = Vector3.Angle(-data._velocity, normal);
            float angleDeg = Mathf.Clamp(rawAngleDeg, 0f, 90f);

            // sin(angle) 用于轨迹公式
            data._sinAngle = Mathf.Sin(angleDeg * Mathf.Deg2Rad);

            // 坠落时间：angle=0 → minDuration，angle=90 → maxDuration
            float t = angleDeg / 90f;

            if (collectDuration.isZero())
                collectDuration = Mathf.Lerp(minCollectDuration, maxCollectDuration, t);

            data._immediately = immediately;
            data._fallDuration = collectDuration;

            // 切向单位向量：在 XY 平面内与 entry 半径方向垂直
            // -normal 始终指向 entry 点，即物体进入的那一侧（而不是行星中心）
            // velocity 减去它在 -normal 上的投影，剩余部分就是指向行星内侧的切向分量
            Vector3 inwardNormal = -normal;
            Vector3 tangent2D = data._velocity - inwardNormal * Vector3.Dot(data._velocity, inwardNormal);
            data._tangent = tangent2D.normalized;

            data._fallElapsed = 0f;
            data._fallProgress = 0f;

            // Debug.Log($"[GravityBody] 捕获！入射角={angleDeg:F1}°，" + $"sin={data._sinAngle:F3}，坠落时间={data._fallDuration:F2}s，" + $"entry半径={data._entryRadius:F2}，tangent={data._tangent}");
            capturedList.add(data);
            capturedBalls.add(ball);
        }

        /// <summary>
        /// 坠落阶段：用解析螺旋路径插值，每帧推进 _fallElapsed。
        /// </summary>
        bool HandleFalling(CaptureData data, float dt)
        {
            data._planetPos = _character.getWorldPosition();
            data._fallElapsed += dt;
            float t = Mathf.Clamp01(data._fallElapsed / data._fallDuration);
            data._fallProgress = t;

            // 螺旋轨迹：
            //   offset = sin(angle) × R_entry × 4t(1-t)
            //   pos = Lerp(entryPos, planetPos, t) + offset × tangent
            float offset = data._sinAngle * data._entryRadius * 4f * t * (1f - t);
            Vector3 radialLerp = Vector3.Lerp(data._entryPos, data._planetPos, t);
            data.ball.setWorldPosition(radialLerp + data._tangent * offset);

            // 更新速度（用于可视化）
            // 下一帧位置差分
            float nextT = Mathf.Clamp01((data._fallElapsed + dt) / data._fallDuration);
            float nextOffset = data._sinAngle * data._entryRadius * 4f * nextT * (1f - nextT);
            Vector3 nextRadialLerp = Vector3.Lerp(data._entryPos, data._planetPos, nextT);
            Vector3 nextPos = nextRadialLerp + data._tangent * nextOffset;
            data._velocity = (nextPos - data.ball.getWorldPosition()) / dt;

            // 坠毁检测：抵达行星中心附近
            if (t >= 1f || (data._planetPos - data.ball.getWorldPosition()).magnitude <= 0.01F || data._immediately)
            {
                OnCrash(data.ball);
                return true;
            }

            return false;
        }

        /// <summary>坠毁回调，可被子类重写。</summary>
        protected virtual void OnCrash(Ball ball)
        {
            ball.setWorldPosition(_player.getWorldPosition());

            if (_player.BallManagement.Slots.TryGetAlreadyShootSlotByBallInstance(ball, out var slot))
            {
                slot.TryReload(ball);
            }

            foreach (var h in _handleWeaponList)
            {
                h.CurrentWeapon.CurrentAmmoLoaded++;
            }

            if (_testWeapon)
            {
                _testWeapon.CurrentAmmoLoaded++;
            }
        }
    }
}