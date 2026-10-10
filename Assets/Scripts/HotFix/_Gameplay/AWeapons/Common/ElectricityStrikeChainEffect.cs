using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

/// <summary>
/// 连锁电流
/// 多个目标顺序受到伤害
/// 伤害递增
/// </summary>
public class ElectricityStrikeChainEffect : ALogicEffect, IArgs<APlayer, Brick, int, int>
{
    protected const string path = $"{GAMEPLAY_PATH}/Effects/Fx_ElectricityStrike_Blue.prefab";
    protected const float GAP = 0.15F;
    protected const float AFTER_DURATION = 1F;

    protected List<LightningBolt2D> electricityList = new();
    protected List<Brick> history = new();
    protected APlayer _player;
    protected Brick brick;

    protected int maxCount;
    protected int counter;
    protected int damage;
    protected bool lastOne;

    public void onCreate(APlayer p, Brick b2, int c, int d)
    {
        duration = GAP;
        _player = p;
        brick = b2;
        maxCount = c;
        counter = 0;
        damage = d;
        lastOne = false;
        history.add(b2);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        brick = null;
        _player = null;
        lastOne = false;
        maxCount = 0;
        counter = 0;
        damage = 0;
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
        if (duration.unstarted && !lastOne)
        {
            var excludePos = history[counter].getWorldPosition();
            if (brickManager.getNearestActiveBrick(excludePos, out var b, history))
            {
                if (brick)
                {
                    brick = null;
                }

                history.add(b);
                var o = prefabPool.createObject(path);
                if (o.TryGetComponent<LightningBolt2D>(out var bolt))
                {
                    electricityList.add(bolt);
                    bolt.startPointTransform = history[counter].getTransform();
                    bolt.endPointTransform = history[counter + 1].getTransform();
                    //Stop object from generating new lightnings
                    bolt.isPlaying = false;
                    //Generate lightnings once, based on your configuration
                    bolt.FireOnce();

                    var dmg = Dmg.AP(damage);
                    b.Health.Damage(ref dmg, _player.gameObject, _player, direction: Vector3.up);
                    if (dmg.IsLethal)
                    {
                        maxCount++;
                    }

                    sound.play(SoundDefine.ELECTRICITY_STRIKE);
                }

                counter++;
                if (counter == maxCount)
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