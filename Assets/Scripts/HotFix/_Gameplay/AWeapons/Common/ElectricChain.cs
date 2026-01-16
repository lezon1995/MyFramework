using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class ElectricChain : ALogicEffect, IArgs<Ball, Brick, int>
{
    const string path = $"{GAMEPLAY_PATH}/Prefabs/FxParticle/FxElectricChain.prefab";
    const float GAP = 0.15F;
    const float AFTER_DURATION = 1F;
    Brick brick;
    Ball ball;
    List<Brick> brickQueue = new();
    TimerInt count;
    bool lastOne;
    List<GameObject> list = new();

    public void onCreate(Ball b1, Brick b2, int c)
    {
        duration = GAP;
        ball = b1;
        brick = b2;
        count = c;
        lastOne = false;
        brickQueue.add(b2);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        brick = null;
        lastOne = false;
        count = 0;
        brickQueue.Clear();
        for (var i = list.Count - 1; i >= 0; i--)
        {
            mPrefabPoolManager.destroyObject(list[i], false);
            list.RemoveAt(i);
        }
    }

    public override bool update(float dt)
    {
        if (ball == null)
            return true;
        
        if (duration.unstarted && !lastOne)
        {
            var excludePos = brickQueue[count.elapsed].getWorldPosition();
            if (brickManager.getRandomActiveBrick(out var b, brickQueue, excludePos, 1.5F))
            {
                brickQueue.add(b);
                var o = mPrefabPoolManager.createObject(path, 0, false, true, null);
                list.add(o);
                if (o.TryGetComponent<LightningBolt2D.LightningBolt2D>(out var bolt))
                {
                    bolt.startPoint = brickQueue[count.elapsed].getWorldPosition();
                    bolt.endPoint = brickQueue[count.elapsed + 1].getWorldPosition();
                    //Stop object from generating new lightnings
                    bolt.isPlaying = false;
                    //Generate lightnings once, based on your configuration
                    bolt.FireOnce();
                    var dmg = ball.getSkillDmg(b);
                    gameplayManager.handleSkillDamage(ball, b, ref dmg, out _);
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

        base.update(dt);
        if (isDone && !lastOne)
        {
            duration.reset();
            isDone = false;
        }

        return isDone;
    }
}