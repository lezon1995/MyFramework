using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public class PoisonBurningEffect : ALogicEffect
    , IArgs<APlayer, Brick>
{
    public static Dictionary<Brick, PoisonBurningEffect> affectedBricks = new();

    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_PoisonBurning.prefab";
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_PoisonBurning.prefab";

    protected APlayer _player;
    protected Brick brick;
    protected Buff buffPrototype;
    protected ParticleSystem buffFx;
    protected GameEffect buffEffect;
    Action onBuffRemoved;
    Action onBuffPeriodDamage;
    Action<int, int> onBuffStackChanged;

    public PoisonBurningEffect()
    {
        onBuffRemoved = OnBuffRemoved;
        onBuffPeriodDamage = OnBuffPeriodDamage;
        onBuffStackChanged = OnBuffStackChanged;
    }

    public void onCreate(APlayer b1, Brick b2)
    {
        duration = float.MaxValue;
        _player = b1;
        brick = b2;

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
            brick.Buffable.ApplyBuff(buffPrototype, _player.Buffable);
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
        scale.x = BrickUnitSize * size.x;
        shape.scale = scale;

        var position = shape.position;
        position.y = -(BrickUnitSize * size.y * 0.5F);
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
        affectedBricks.Remove(brick);
    }

    public bool tryApply()
    {
        brick.Buffable.ApplyBuff(buffPrototype, _player.Buffable);
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
        _player = null;
        brick = null;
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

    public static void Apply(APlayer p, Brick b)
    {
        if (!affectedBricks.TryGetValue(b, out var effect))
        {
            effect = effectManager.addLogic<PoisonBurningEffect>();
            effect.with(p, b);
            affectedBricks[b] = effect;
        }
        else
        {
            effect.tryApply();
        }
    }
}