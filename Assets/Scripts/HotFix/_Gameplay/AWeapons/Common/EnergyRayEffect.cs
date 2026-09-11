using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 能量射线效果
/// </summary>
public class EnergyRayEffect : ALogicEffect, IArgs<Ball, float, float, Action>
{
    protected const string fx_path1 = $"{GAMEPLAY_PATH}/Effects/Fx_EnergyRay_Loop.prefab";
    protected const string fx_path2 = $"{GAMEPLAY_PATH}/Effects/Fx_EnergyRay_Flash.prefab";
    protected Ball ball;
    protected GameEffect effect1;
    protected GameEffect effect2;
    protected float curAngle;
    protected float rotateAngleSpeed;
    protected float logicDuration;
    protected bool inDisappear;
    protected int fixedUpdateInterval;
    protected Action onFinish;
    const float disappearTime = 0.2666666f;
    Dictionary<Brick, int> brickHitCounters = new();

    public void onCreate(Ball b1, float _duration, float rotateSpeed, Action action)
    {
        ball = b1;
        logicDuration = _duration;
        duration = _duration + disappearTime;
        onFinish = action;
        var pos = b1.getWorldPosition();
        var angle = randomFloat(0F, 360F);
        curAngle = angle;
        rotateAngleSpeed = rotateSpeed;
        fixedUpdateInterval = 5;
        effect1 = mEffectManager.createEffect(fx_path1, _duration);
        effect1.setWorldPosition(pos);
        effect1.setRotationZ(angle);
    }

    void CauseDamage(Vector3 pos, float angle)
    {
        Vector3 dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector2.up;
        using var _ = new ListScope<RaycastHit2D>(out var hits);
        var distance = 40;
        var start = pos;
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        var count = Physics2D.CircleCast(start, 0.1F, dir, filter, hits, distance);
        for (var i = 0; i < count; i++)
        {
            if (hits[i].collider.TryGetComponent(out Brick b))
            {
                bool canDamage = false;
                if (brickHitCounters.TryGetValue(b, out var counter))
                {
                    var nextCounter = (counter + 1) % fixedUpdateInterval;
                    brickHitCounters[b] = nextCounter;
                    if (nextCounter == 0)
                    {
                        canDamage = true;
                    }
                }
                else
                {
                    brickHitCounters[b] = 0;
                    canDamage = true;
                }

                if (canDamage)
                {
                    var dmg = ball.getSkillDmg(b);
                    b.Health.Damage(ref dmg, ball.gameObject, ball.Player);
                }
            }
        }
    }

    public override void onLateUpdate(float dt)
    {
        refreshEffectPosition();
        refreshEffectRotation(dt);

        if (inDisappear && duration.elapsed > (logicDuration + disappearTime * 0.5F) && effect1.isActive())
        {
            effect1.setActive(false);
        }

        if (duration.elapsed > logicDuration && !inDisappear)
        {
            inDisappear = true;
            effect2 = mEffectManager.createEffect(fx_path2);
            refreshEffectPosition();
            refreshEffectRotation(dt);
        }
    }

    void refreshEffectPosition()
    {
        var pos = ball.getWorldPosition();
        effect1?.setWorldPosition(pos);
        effect2?.setWorldPosition(pos);
    }

    void refreshEffectRotation(float dt)
    {
        curAngle += rotateAngleSpeed * dt;
        effect1?.setRotationZ(curAngle);
        effect2?.setRotationZ(curAngle);
    }

    public override bool fixedUpdate(float dt)
    {
        CauseDamage(ball.getWorldPosition(), curAngle);

        return base.fixedUpdate(dt);
    }

    public override void onFinished()
    {
        onFinish?.Invoke();
        base.onFinished();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        if (effect1)
            mEffectManager.destroyEffect(ref effect1);
        if (effect2)
            mEffectManager.destroyEffect(ref effect2);

        onFinish = null;
        curAngle = 0;
        logicDuration = 0;
        rotateAngleSpeed = 0;
        fixedUpdateInterval = 0;
        inDisappear = false;
        brickHitCounters.Clear();
    }
}