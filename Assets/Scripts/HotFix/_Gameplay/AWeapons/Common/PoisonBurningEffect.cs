using System;
using UnityEngine;

namespace MoreMountains;

public class PoisonBurningEffect : ALogicEffect
    , IArgs<Ball, Brick, Action<Brick>>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_PoisonBurning.prefab";
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_PoisonBurning.prefab";

    protected Ball ball;
    protected Brick brick;
    protected Buff buffPrototype;
    protected ParticleSystem buffFx;
    protected GameEffect buffEffect;
    Action<Brick> onRemoved;
    Action onBuffRemoved;
    Action onBuffPeriodDamage;
    Action<int, int> onBuffStackChanged;

    public PoisonBurningEffect()
    {
        onBuffRemoved = OnBuffRemoved;
        onBuffPeriodDamage = OnBuffPeriodDamage;
        onBuffStackChanged = OnBuffStackChanged;
    }

    public void onCreate(Ball b1, Brick b2, Action<Brick> a)
    {
        duration = float.MaxValue;
        ball = b1;
        brick = b2;
        onRemoved = a;

        var effect = mEffectManager.createEffect(path);
        buffEffect = effect;
        if (effect.tryGetUnityComponent(out buffFx))
        {
            SetFxBurstCount(1);
            SetFxShape(brick.getSize());
            SetFxVelocity(brick.getSize());
        }

        refreshEffectPosition();

        var o = prefabPool.createObject(buff_path, moveToHide: true);
        if (o.TryGetComponent(out buffPrototype))
        {
            buffPrototype.OnRemoved = onBuffRemoved;
            buffPrototype.OnPeriodDamage = onBuffPeriodDamage;
            buffPrototype.OnStackChanged = onBuffStackChanged;
            brick.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
        }
    }

    void refreshEffectPosition()
    {
        var pos = brick.getCenterPosition();
        buffEffect.setWorldPosition(pos);
    }

    void OnBuffPeriodDamage()
    {
        buffFx.Emit(buffPrototype.Stack);
    }

    void OnBuffStackChanged(int oldStacked, int newStacked)
    {
        SetFxBurstCount(newStacked);
    }

    void SetFxBurstCount(int count)
    {
        var burst = buffFx.emission.GetBurst(0);
        burst.count = count;
        buffFx.emission.SetBurst(0, burst);
    }

    void SetFxShape(Vector2Int size)
    {
        var shape = buffFx.shape;
        var scale = shape.scale;
        scale.x = 0.675F * size.x;
        shape.scale = scale;

        var position = shape.position;
        position.y = -(0.675F * size.y * 0.5F);
        shape.position = position;
    }

    void SetFxVelocity(Vector2Int size)
    {
        var velocityOverLifetime = buffFx.velocityOverLifetime;
        velocityOverLifetime.speedModifierMultiplier = size.y;
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
        // onBuffPeriodDamage = null;
        // onBuffStackChanged = null;
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