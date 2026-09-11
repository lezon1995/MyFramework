using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    public class PlayerRecollectBallAbility : PlayerAbility
    {
        // 进入过范围又离开、之后再次进入的球，等价于原 OnTriggerExit 留下的「待回收」集合
        SafeHashSet<Ball> canRecollectedBalls = new();

        // 上一帧位于范围内的球，用于判断本帧的「进入/离开」边沿事件
        HashSet<Ball> lastInRangeBalls = new();

        // 回收引力半径。距离角色位置 ≤ 此半径视为「在范围内」，等价于原先 Trigger 触发器的判定。
        public float collectRadius = 1F;

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
            // 推进捕获坠落阶段
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

        public override void OnFixedUpdate(float dt)
        {
            // 用距离判断替代原先 OnTriggerEnter2D / OnTriggerExit2D
            DetectBallsByDistance();
        }

        void DetectBallsByDistance()
        {
            var player = _player;
            var activeBalls = player.BallManagement.Instance.getActiveBalls();
            if (activeBalls == null)
                return;

            Vector3 planetPos = _character.getWorldPosition();
            float radiusSqr = collectRadius * collectRadius;

            using var _ = new HashSetScope<Ball>(out var inRangeThisFrame);
            foreach (var ball in activeBalls)
            {
                if (!ball.Recollectable)
                    continue;

                if (!ball.Player.equalWith(player))
                    continue;

                if (ball.SourceWeapon is BallGunWeapon)
                    continue;

                if (capturedBalls.contains(ball))
                    continue;

                if (!ball.inUse)
                    continue;

                float distSqr = (ball.getWorldPosition() - planetPos).sqrMagnitude;
                if (distSqr <= radiusSqr)
                    inRangeThisFrame.Add(ball);
            }

            // 刚进入范围：上一帧不在、本帧在，且曾离开过 → 触发回收
            foreach (var ball in inRangeThisFrame)
            {
                if (!lastInRangeBalls.Contains(ball) && canRecollectedBalls.remove(ball))
                {
                    RecollectBall(ball);
                }
            }

            // 刚离开范围：上一帧在、本帧不在 → 加入候选集合
            foreach (var ball in lastInRangeBalls)
            {
                if (!inRangeThisFrame.Contains(ball))
                    canRecollectedBalls.add(ball);
            }

            // 清理已经失效的候选项（已捕获、不再 inUse、或已不在 active 集合）
            using var __ = new SafeHashSetReader<Ball>(canRecollectedBalls, out var reader);
            foreach (var b in reader)
            {
                if (capturedBalls.contains(b) || !b.inUse || !activeBalls.Contains(b))
                {
                    canRecollectedBalls.remove(b);
                }
            }

            lastInRangeBalls.Clear();
            foreach (var ball in inRangeThisFrame)
                lastInRangeBalls.Add(ball);
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
            ball.onRecollected();

            if (_player.Inventory.BallBag.TryGetAlreadyShootSlotByBallInstance(ball, out var slot))
            {
                slot.TryReload(_player, ball);
            }
        }
    }
}