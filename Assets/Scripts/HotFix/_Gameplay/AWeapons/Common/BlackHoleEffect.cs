using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public class BlackHoleEffect : ALogicEffect, IArgs<Ball, Brick>
{
    protected const string spot_path = $"{GAMEPLAY_PATH}/Effects/Fx_BlackHole.prefab";

    protected Ball ball;
    Vector2 spotPos;
    GameEffect fxBlackHole;
    HashSet<Brick> bricks = new();

    // 黑洞吸引参数
    [Tooltip("吸引范围半径")]
    float pullRadius;
    [Tooltip("基础吸引力(每秒速度)")]
    float basePullForce;
    [Tooltip("距离越近, 吸引力倍率上限(避免无限加速)")]
    float maxPullMultiplier;
    [Tooltip("进入吸引的最小距离, 低于该值停止施力(防止抖动)")]
    float stopDistance;

    public void onCreate(Ball b1, Brick b2)
    {
        duration = 3.2F;
        ball = b1;
        fxBlackHole = mEffectManager.createEffect(spot_path, duration);
        spotPos = b1.getWorldPosition();
        fxBlackHole.setWorldPosition(spotPos);
        
        pullRadius = 1.5F;
        basePullForce = 0.1F;
        maxPullMultiplier = 1F;
        stopDistance = 0.15F;
    }

    public override bool fixedUpdate(float dt)
    {
        bricks.Clear();
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        var count = Physics2D.OverlapCircle(spotPos, pullRadius, filter, colliders);
        for (var i = 0; i < count; i++)
        {
            var collider = colliders[i];
            if (collider.TryGetComponent(out Brick b))
            {
                if (b.IsDead())
                    continue;

                bricks.add(b);
            }
        }

        // 对每个砖块施加指向黑洞中心的吸引力
        // 吸引力随距离衰减(越近力越大, 但有上限), 防止砖块无限加速和中心点抖动
        foreach (var brick in bricks)
        {
            var health = brick.Health;
            Vector2 brickPos = brick.getCenterPosition();
            Vector2 toCenter = spotPos - brickPos;
            float distance = toCenter.magnitude;

            // 已经非常接近中心, 不再施力(避免抖动)
            if (distance <= stopDistance)
                continue;

            Vector2 direction = toCenter / distance;

            // 距离越近倍率越高, 封顶在 maxPullMultiplier
            float t = 1F - Mathf.Clamp01(distance / pullRadius);
            float multiplier = Mathf.Lerp(1F, maxPullMultiplier, t);

            // 每秒的速度增量(force 单位是"速度/秒", 配合 KnockbackDecay 形成稳定速度)
            var force = basePullForce * multiplier * direction.normalized;
            
            health.ApplyKnockback(force);
        }

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        spotPos = default;
        pullRadius = 0;
        basePullForce = 0;
        maxPullMultiplier = 0;
        stopDistance = 0;
        if (fxBlackHole)
        {
            mEffectManager.destroyEffect(ref fxBlackHole);
        }

        bricks.Clear();
    }
}