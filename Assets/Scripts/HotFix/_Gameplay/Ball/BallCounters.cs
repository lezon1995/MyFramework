using System.Collections.Generic;

namespace MarbleHero;

public class BallCounters : ClassObject
{
    public HitCounter hit = new();
    public ReflectCounter reflect = new();
    public CritHitCounter critHit = new();
    public HitBrickCounter hitBrick = new();
    public KillBrickCounter killBrick = new();
    public PenetrateBrickCounter penetrateBrick = new();

    public override void resetProperty()
    {
        base.resetProperty();
        hit.reset();
        reflect.reset();
        critHit.reset();
        hitBrick.reset();
        killBrick.reset();
        penetrateBrick.reset();
    }
}



public abstract class BallCounter : Counter
{
    public List<CounterTrigger> triggers = new();

    public void addTrigger(CounterTrigger trigger)
    {
        triggers.Add(trigger);
    }
}

public class HitCounter : BallCounter
{
    public static HitCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        base.count(delta);
        foreach (var trigger in triggers)
        {
            trigger.count(delta);
        }
        return true;
    }
}

public class CritHitCounter : BallCounter
{
    public static CritHitCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class ReflectCounter : BallCounter
{
    public static ReflectCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class PenetrateBrickCounter : BallCounter
{
    public static PenetrateBrickCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class HitBrickCounter : BallCounter
{
    public static HitBrickCounter global = new();

    public void count(Brick b)
    {
        foreach (var t in triggers)
        {
            if (t.triggerAction is ITriggerAction<Brick> action)
            {
                action.trigger(b);
            }
        }
    }

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class KillBrickCounter : BallCounter
{
    public static KillBrickCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}