using System.Collections.Generic;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class ElectricityStrikeEffect : ALogicEffect, IArgs<Ball, Brick, int>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_ElectricityStrike.prefab";
    protected const float GAP = 0.15F;
    protected const float AFTER_DURATION = 1F;

    protected List<LightningBolt2D> electricityList = new();
    protected List<Brick> history = new();
    protected Ball ball;
    protected Brick brick;

    protected Countdown count;
    protected bool lastOne;

    public void onCreate(Ball b1, Brick b2, int c)
    {
        duration = GAP;
        ball = b1;
        brick = b2;
        count = c;
        lastOne = false;
        history.add(b2);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        ball = null;
        brick = null;
        lastOne = false;
        count = 0;
        history.Clear();
        for (var i = electricityList.Count - 1; i >= 0; i--)
        {
            var bolt2D = electricityList[i];
            bolt2D.startPointTransform = null;
            bolt2D.endPointTransform = null;
            prefabPool.destroyObject(bolt2D.gameObject, false);
            electricityList.RemoveAt(i);
        }
    }

    public override bool fixedUpdate(float dt)
    {
        if (ball == null)
            return true;

        if (duration.unstarted && !lastOne)
        {
            var excludePos = history[count.elapsed].getWorldPosition();
            if (brickManager.getRandomActiveBrick(out var b, history, excludePos))
            {
                if (brick)
                {
                    var dmg = ball.getSkillDmg(brick);
                    brick.Health.Damage(ref dmg, ball.gameObject, ball.Player);
                    brick = null;
                }

                history.add(b);
                var o = prefabPool.createObject(path);
                if (o.TryGetComponent<LightningBolt2D>(out var bolt))
                {
                    electricityList.add(bolt);
                    bolt.startPointTransform = history[count.elapsed].getTransform();
                    bolt.endPointTransform = history[count.elapsed + 1].getTransform();
                    //Stop object from generating new lightnings
                    bolt.isPlaying = false;
                    //Generate lightnings once, based on your configuration
                    bolt.FireOnce();
                    var dmg = ball.getSkillDmg(b);
                    b.Health.Damage(ref dmg, ball.gameObject, ball.Player);

                    sound.play(SoundDefine.ELECTRICITY_STRIKE);
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