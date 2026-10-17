using System;
using UnityEngine;

namespace MoreMountains;

public class BezierMover : ClassObject
{
    public override void resetProperty()
    {
        base.resetProperty();
        transform = null;
        _target = null;
        _p0 = default;
        _p1 = default;
        _impactScatter = default;
        _flightDuration = 0;
        _elapsed = 0;
        _maxLifeTime = 0;
        _exploded = false;
        _segments = default;
        _onExplode = null;
        _onLostTarget = null;
    }

    Transform transform;
    Transform _target;

    Vector2 _p0; // 起点（角色身上）
    Vector2 _p1; // 控制点：target→caster 反向 + 随机偏角 + 抬升

    Vector2 _p_mid => (_p0 + _p_end) * 0.5f;
    Vector2 _p2 => _p_mid + (_p_mid - _p1).normalized * 2;

    // 终点（每帧实时读 _target.position,叠加一次性散布）
    Vector2 _p_end => (Vector2)_target.position + _impactScatter;
    Vector2 _impactScatter; // 命中点散布（生成时随机一次后锁定）
    float _flightDuration; // 飞行时间（秒）
    float _elapsed;
    float _maxLifeTime;
    bool _exploded;
    BezierSegments _segments;
    Action _onExplode;
    Action _onLostTarget;

    public void setOnExplode(Action onExplode) => _onExplode = onExplode;
    public void setOnLostTarget(Action onLostTarget) => _onLostTarget = onLostTarget;

    public void launch(
        Transform t,
        Transform target,
        Vector2 p0,
        Vector2 p1,
        float flightDuration,
        BezierSegments segments,
        float maxLifeTime = 3F,
        Vector2 impactScatter = default
    )
    {
        transform = t;
        _target = target;
        _p0 = p0;
        _p1 = p1;
        _flightDuration = Mathf.Max(0.01f, flightDuration);
        _segments = segments;

        _elapsed = 0f;
        _exploded = false;
        _maxLifeTime = maxLifeTime;

        // 命中点散布一次性随机
        _impactScatter = impactScatter;

        // 初始位置 = P0
        transform.position = _p0;

        // 启动朝向：沿 P0→P1 方向（让飞弹的初始朝向就是「准备往后飞」的方向）
        UpdateRotation();
    }

    public void movement(float dt)
    {
        if (_target == null)
        {
            _onLostTarget?.Invoke();
            return;
        }

        if (_exploded)
            return;

        _elapsed += dt;

        // 安全网：超过 MaxLifetime 直接销毁
        if (_elapsed >= _maxLifeTime)
        {
            Explode();
            return;
        }

        // 计算归一化飞行进度
        var t = Mathf.Clamp01(_elapsed / _flightDuration);
        switch (_segments)
        {
            case BezierSegments.One:
            {
                // 二次贝塞尔曲线：P0 → P1 → P_End
                // P1 在 Launch 时已经按 (target→caster) 反向 + 随机偏转 + 抬升算好。
                // 这里再叠加 arcHeightOffset（jitter + interleave）,让相邻飞弹的弧线不完全一样。
                var p1 = _p1;
                var pos = Bezier2(_p0, p1, _p_end, t);
                transform.position = pos;

                // 朝向当前运动方向（首尾用切线近似，避免端点速度为 0 时抖动）
                var dt2 = 0.01f;
                var ahead = Mathf.Min(t + dt2, 1f);
                var aheadPos = Bezier2(_p0, p1, _p_end, ahead);
                UpdateRotationByVelocity(pos, aheadPos);
            }
                break;
            case BezierSegments.Two:
            {
                if (t < 0.5F)
                {
                    var f = Mathf.InverseLerp(0F, 0.5F, t);
                    // 二次贝塞尔曲线：P0 → P1 → P_Mid
                    // P1 在 Launch 时已经按 (target→caster) 反向 + 随机偏转 + 抬升算好。
                    // 这里再叠加 arcHeightOffset（jitter + interleave）,让相邻飞弹的弧线不完全一样。
                    var p1 = _p1;
                    var pos = Bezier2(_p0, p1, _p_mid, f);
                    transform.position = pos;

                    // 朝向当前运动方向（首尾用切线近似，避免端点速度为 0 时抖动）
                    var dt2 = 0.01f;
                    var ahead = Mathf.Min(f + dt2, 1f);
                    var aheadPos = Bezier2(_p0, p1, _p_mid, ahead);
                    UpdateRotationByVelocity(pos, aheadPos);
                }
                else
                {
                    var f = Mathf.InverseLerp(0.5F, 1F, t);
                    // 二次贝塞尔曲线：P_Mid → P2 → P_End
                    var p2 = _p2;
                    var pos = Bezier2(_p_mid, p2, _p_end, f);
                    transform.position = pos;

                    // 朝向当前运动方向（首尾用切线近似，避免端点速度为 0 时抖动）
                    var dt2 = 0.01f;
                    var ahead = Mathf.Min(f + dt2, 1f);
                    var aheadPos = Bezier2(_p_mid, p2, _p_end, ahead);
                    UpdateRotationByVelocity(pos, aheadPos);
                }
            }
                break;
        }

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
        var dir = _p1 - _p0;
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
        _onExplode?.Invoke();
    }
}