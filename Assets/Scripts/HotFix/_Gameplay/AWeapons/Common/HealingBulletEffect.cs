using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

public class HealingBulletEffect : ALogicEffect, IArgs<Transform, Transform, Heal>
{
    protected const string fx_path = $"{GAMEPLAY_PATH}/Effects/Fx_MissileProjectile_Green.prefab";

    Transform start, end;
    Heal heal;
    GameEffect effect;
    Vector3 startPos;

    BezierMover mover;

    public void onCreate(Transform _start, Transform _end, Heal _heal)
    {
        start = _start;
        end = _end;
        heal = _heal;
        var flyDuration = randomFloat(0.8F, 1.6F);
        duration = flyDuration;
        startPos = start.position;
        effect = mEffectManager.createEffect(fx_path);
        effect.setWorldPosition(startPos);
        effect.clearTrail();

        CLASS(out mover);

        var p0 = (Vector2)start.position;
        var p2 = (Vector2)end.position;
        var p1 = MissileProjectile.ComputeOutgoingControlPoint(
            casterPos: p0,
            targetPos: p2,
            outgoingDistance: 4F,
            outgoingYawSpreadMax: 45F,
            outgoingYawSpreadMin: 30F,
            outgoingVerticalLift: 4F);

        BezierSegments segments = BezierSegments.One;
        if (randomHit(0.5F))
        {
            segments = BezierSegments.Two;
        }

        // === 4. Launch ===
        mover.launch(effect.transform, end, p0, p1, flyDuration, segments);
    }

    public override bool fixedUpdate(float dt)
    {
        return base.fixedUpdate(dt);
    }

    public override bool update(float dt)
    {
        var f = MMTween.Evaluate(duration.pct, MMTween.MMTweenCurve.EaseOutCubic);
        var curPos = Vector3.Lerp(startPos, end.position, f);
        effect.setWorldPosition(curPos);
        mover.movement(dt);
        return base.update(dt);
    }

    public override void onFinished()
    {
        if (end.TryGetComponent(out Health health))
        {
            health.ReceiveHealth(heal, source: health.Character);
            sound.play(SoundDefine.HEAL_EFFECT);
        }

        base.onFinished();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        start = null;
        end = null;
        heal = default;
        startPos = default;
        if (effect)
        {
            effect.clearTrail();
            mEffectManager.destroyEffect(ref effect);
        }

        if (mover)
        {
            UN_CLASS(ref mover);
        }
    }
}