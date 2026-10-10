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

    public void onCreate(Transform p1, Transform p2, Heal p3)
    {
        start = p1;
        end = p2;
        heal = p3;
        duration = randomFloat(0.3F, 0.6F);
        startPos = start.position;
        effect = mEffectManager.createEffect(fx_path);
        effect.setWorldPosition(startPos);
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
        mEffectManager.destroyEffect(ref effect);
    }
}