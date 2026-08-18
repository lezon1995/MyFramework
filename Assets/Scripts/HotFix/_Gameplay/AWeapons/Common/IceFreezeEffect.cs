using System;

namespace MoreMountains;

/// <summary>
/// 冰冻效果
/// </summary>
public class IceFreezeEffect : ALogicEffect
    , IArgs<Ball, Brick>
{
    protected const string buff_path = $"{GAMEPLAY_PATH}/Buffs/Buff_IceFreeze.prefab";

    protected Ball ball;
    protected Brick brick;
    protected Buff buffPrototype;
    Action onBuffRemoved;

    public IceFreezeEffect()
    {
        onBuffRemoved = OnBuffRemoved;
    }

    public void onCreate(Ball b1, Brick b2)
    {
        duration = float.MaxValue;
        ball = b1;
        brick = b2;
        
        var o = prefabPool.createObject(buff_path, moveToHide: true);
        if (o.TryGetComponent(out buffPrototype))
        {
            buffPrototype.OnRemoved = onBuffRemoved;
            var success = brick.Buffable.ApplyBuff(buffPrototype, ball.Buffable);
            if (!success)
            {
                isDone = true;
                log("目标身上已经存在原型Buff，添加失败");
            }
        }
    }

    void OnBuffRemoved()
    {
        isDone = true;
    }

    public override bool update(float dt)
    {
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
        // onBuffRemoved = null;
        // onBuffPeriodDamage = null;
        if (buffPrototype)
        {
            prefabPool.destroyObject(buffPrototype);
        }

        buffPrototype = null;
    }
}