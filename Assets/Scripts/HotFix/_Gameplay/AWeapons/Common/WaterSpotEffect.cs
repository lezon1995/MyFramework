using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public class WaterSpotEffect : ALogicEffect, IArgs<Ball, Brick>
{
    protected const string spot_path = $"{GAMEPLAY_PATH}/Effects/Fx_WaterSpot.prefab";
    protected const string slowdown_path = $"{GAMEPLAY_PATH}/Effects/Fx_Slowdown.prefab";
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_WaterSpot.prefab";

    protected Ball ball;
    Vector2 spotPos;
    GameEffect buffEffect;
    static Dictionary<Brick, Buff> globalBrickBuffs = new();
    SafeDictionary<Brick, Buff> brickBuffs = new();
    HashSet<Brick> bricks = new();

    Action<Buffable> onBuffRemoved;

    public WaterSpotEffect()
    {
        onBuffRemoved = OnBuffRemoved;
    }

    public void onCreate(Ball b1, Brick b2)
    {
        duration = 3.2F;
        ball = b1;
        buffEffect = mEffectManager.createEffect(spot_path, duration);
        spotPos = b1.getWorldPosition();
        buffEffect.setWorldPosition(spotPos);
    }

    public override bool fixedUpdate(float dt)
    {
        bricks.Clear();
        using var _ = new ListScope<Collider2D>(out var colliders);
        var filter = new ContactFilter2D();
        filter.useTriggers = true;
        filter.SetLayerMask(BRICK_LAYER_MASK);
        var count = Physics2D.OverlapBox(spotPos, new Vector2(0.675F, 0.675F) * 3F, 0F, filter, colliders);
        for (var i = 0; i < count; i++)
        {
            var collider = colliders[i];
            if (collider.TryGetComponent(out Brick b))
            {
                if (b.IsDead())
                    continue;

                if (!globalBrickBuffs.TryGetValue(b, out var buffPrototype))
                {
                    var o = prefabPool.createObject(buff_path, moveToHide: true);
                    if (o.TryGetComponent(out buffPrototype))
                    {
                        buffPrototype.OnRemovedTarget = onBuffRemoved;
                        b.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
                    }

                    globalBrickBuffs[b] = buffPrototype;
                    brickBuffs[b] = buffPrototype;
                }

                bricks.add(b);
            }
        }

        using var __ = new SafeDictionaryReader<Brick, Buff>(brickBuffs, out var reader);
        foreach (var (b, buff) in reader)
        {
            if (!bricks.contains(b))
            {
                if (buff.HasReset)
                {
                    brickBuffs.remove(b);
                    continue;
                }

                b.Buffable.RemoveBuff(buff, Buff.Removal.Manul, false);
                brickBuffs.remove(b);
            }
        }

        return base.fixedUpdate(dt);
    }

    void OnBuffRemoved(Buffable target)
    {
        if (target.Character is Brick b)
        {
            if (globalBrickBuffs.TryGetValue(b, out var buff))
            {
                prefabPool.destroyObject(buff);
                globalBrickBuffs.Remove(b);
            }
        }
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        spotPos = default;
        // onBuffRemoved = null;

        foreach (var (b, buff) in brickBuffs)
        {
            b.Buffable.RemoveBuff(buff, Buff.Removal.Manul, false);
        }

        if (buffEffect)
        {
            mEffectManager.destroyEffect(ref buffEffect);
        }

        brickBuffs.clear();
        bricks.Clear();
    }
}