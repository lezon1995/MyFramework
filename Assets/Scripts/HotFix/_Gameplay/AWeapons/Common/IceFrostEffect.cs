using System;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 冰霜效果
/// </summary>
public class IceFrostEffect : ALogicEffect
    , IArgs<Ball, Brick, Action<Brick>>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_Slowdown.prefab";
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_IceFrost.prefab";

    protected Ball ball;
    protected Brick brick;
    protected Buff buffPrototype;
    protected ParticleSystem buffFx;
    protected GameEffect buffEffect;
    Action<Brick> onRemoved;
    Action onBuffRemoved;
    Action<int, int> onStackChanged;

    public IceFrostEffect()
    {
        onBuffRemoved = OnBuffRemoved;
        onStackChanged = OnBuffStackChanged;
    }

    public void onCreate(Ball b1, Brick b2, Action<Brick> a)
    {
        duration = float.MaxValue;
        ball = b1;
        brick = b2;
        onRemoved = a;

        var effect = mEffectManager.createEffect(path);
        buffEffect = effect;
        effect.tryGetUnityComponent(out buffFx);

        refreshEffectPosition();

        var o = prefabPool.createObject(buff_path, moveToHide: true);
        if (o.TryGetComponent(out buffPrototype))
        {
            buffPrototype.OnRemoved = onBuffRemoved;
            buffPrototype.OnStackChanged = onStackChanged;
            brick.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
        }

        var emission = buffFx.emission;
        emission.enabled = true;
        buffFx.Play();
    }

    void refreshEffectPosition()
    {
        var pos = brick.getCenterPosition();
        buffEffect.setWorldPosition(pos);
    }

    void OnBuffRemoved()
    {
        isDone = true;
        onRemoved?.Invoke(brick);
    }

    void OnBuffStackChanged(int oldStack, int newStack)
    {
        var f = newStack * 0.2F;
        brick.brickRenderer.setFrostEffect(f);
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
        if (brick)
        {
            brick.brickRenderer.setFrostEffect(0F);
        }

        brick = null;
        onRemoved = null;
        if (buffFx)
        {
            var emission = buffFx.emission;
            emission.enabled = false;
        }

        buffFx = null;
        // onBuffRemoved = null;
        // onBuffPeriodDamage = null;
        // onStackChanged = null;
        if (buffPrototype)
        {
            prefabPool.destroyObject(buffPrototype);
        }

        buffPrototype = null;

        if (buffEffect)
        {
            mEffectManager.destroyEffect(ref buffEffect);
        }
    }
}