using System;
using UnityEngine;

namespace MoreMountains;

public class ElectrifiedEffect : ALogicEffect
    , IArgs<Ball, Brick, Action<Brick>>
{
    protected const string fx_path = $"{GAMEPLAY_PATH}/Effects/Fx_Electrified.prefab";
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_Electrified.prefab";

    protected Ball ball;
    protected Brick brick;
    protected Buff buffPrototype;
    protected ParticleSystem buffFx;
    protected GameEffect buffEffect;
    Action<Brick> onRemoved;
    Action onBuffRemoved;
    Action<int, int> onBuffStackChanged;
    Func<Dmg> dmgGetter;

    public ElectrifiedEffect()
    {
        onBuffRemoved = OnBuffRemoved;
        onBuffStackChanged = OnBuffStackChanged;
        dmgGetter = DmgGetter;
    }

    public void onCreate(Ball b1, Brick b2, Action<Brick> a)
    {
        duration = float.MaxValue;
        ball = b1;
        brick = b2;
        onRemoved = a;

        var effect = mEffectManager.createEffect(fx_path);
        buffEffect = effect;
        effect.tryGetUnityComponent(out buffFx);
        refreshEffectPosition();

        var o = prefabPool.createObject(buff_path, moveToHide: true);
        if (o.TryGetComponent(out buffPrototype))
        {
            buffPrototype.OnRemoved = onBuffRemoved;
            buffPrototype.OnStackChanged = onBuffStackChanged;
            buffPrototype.DmgGetter = dmgGetter;
            brick.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
        }
    }

    Dmg DmgGetter()
    {
        return ball.getSkillDmg(null);
    }

    void refreshEffectPosition()
    {
        var pos = brick.getCenterPosition();
        buffEffect.setWorldPosition(pos);
    }

    void OnBuffStackChanged(int oldStacked, int newStacked)
    {
        SetFxRateOverTime(newStacked);
    }

    void SetFxRateOverTime(int count)
    {
        var emission = buffFx.emission;
        var e = emission.rateOverTime;
        e.constant = count;
        emission.rateOverTime = e;
    }

    void OnBuffRemoved()
    {
        isDone = true;
        onRemoved?.Invoke(brick);
    }

    public bool tryApply()
    {
        brick.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
        return true;
    }

    public override bool update(float dt)
    {
        refreshEffectPosition();
        return base.update(dt);
    }

    public override bool fixedUpdate(float dt)
    {
        if (duration.unstarted)
        {
            // sound.play(SoundDefine.LASER_BEAM);
        }

        if (isDone)
            return true;

        return base.fixedUpdate(dt);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        brick = null;
        onRemoved = null;
        // onBuffRemoved = null;
        // onBuffStackChanged = null;
        // dmgGetter = null;
        if (buffPrototype)
        {
            prefabPool.destroyObject(buffPrototype);
        }

        buffPrototype = null;

        if (buffEffect)
        {
            mEffectManager.destroyEffect(ref buffEffect);
        }

        buffFx = null;
    }
}