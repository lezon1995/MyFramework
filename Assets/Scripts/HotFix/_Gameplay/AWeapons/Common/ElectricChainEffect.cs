using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class ElectricChainEffect : ALogicEffect, IArgs<Ball, Brick, int>
{
    protected const string path = $"{GAMEPLAY_PATH}/Prefabs/FxParticle/FxElectricChain.prefab";
    protected const float GAP = 0.15F;
    protected const float AFTER_DURATION = 1F;

    protected List<GameObject> list = new();
    protected List<Brick> history = new();
    protected Ball ball;

    protected Countdown count;
    protected bool lastOne;

    public void onCreate(Ball b1, Brick b2, int c)
    {
        duration = GAP;
        ball = b1;
        count = c;
        lastOne = false;
        history.add(b2);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        lastOne = false;
        count = 0;
        history.Clear();
        for (var i = list.Count - 1; i >= 0; i--)
        {
            mPrefabPoolManager.destroyObject(list[i], false);
            list.RemoveAt(i);
        }
    }

    public override bool fixedUpdate(float dt)
    {
        if (ball == null)
            return true;
        
        if (duration.unstarted && !lastOne)
        {
            var excludePos = history[count.elapsed].getWorldPosition();
            if (brickManager.getRandomActiveBrick(out var b, history, excludePos, 1.5F))
            {
                history.add(b);
                var o = mPrefabPoolManager.createObject(path, 0, false, true, null);
                list.add(o);
                if (o.TryGetComponent<LightningBolt2D.LightningBolt2D>(out var bolt))
                {
                    bolt.startPoint = history[count.elapsed].getWorldPosition();
                    bolt.endPoint = history[count.elapsed + 1].getWorldPosition();
                    //Stop object from generating new lightnings
                    bolt.isPlaying = false;
                    //Generate lightnings once, based on your configuration
                    bolt.FireOnce();
                    var dmg = ball.getSkillDmg(b);
                    gameplayManager.handleSkillDamage(ball, b, ref dmg);
                }

                if (count.update())
                {
                    lastOne = true;
                    isDone = false;
                    duration = AFTER_DURATION;
                }
            }
            else
            {
                lastOne = true;
                isDone = false;
                duration = AFTER_DURATION;
            }
        }

        base.fixedUpdate(dt);
        if (isDone && !lastOne)
        {
            duration.reset();
            isDone = false;
        }

        return isDone;
    }
}

public class MovingElectricChainEffect : ElectricChainEffect
{
    
} 