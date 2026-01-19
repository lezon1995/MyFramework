namespace MarbleHero;

public class BallCounters : ClassObject
{
    public HitCounter hit = new();
    public ReflectCounter reflect = new();
    public CritHitCounter critHit = new();
    public CritSkillCounter critSkill = new();
    public HitBrickCounter hitBrick = new();
    public HitKillCounter hitKill = new();
    public SkillKillCounter skillKill = new();
    public PenetrateBrickCounter penetrateBrick = new();

    public override void resetProperty()
    {
        base.resetProperty();
        hit.reset();
        reflect.reset();
        critHit.reset();
        critSkill.reset();
        hitBrick.reset();
        hitKill.reset();
        skillKill.reset();
        penetrateBrick.reset();
    }
}



public abstract class BallCounter : Counter
{
}

public class HitCounter : BallCounter
{
    public static HitCounter global = new();
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
public class CritSkillCounter : BallCounter
{
    public static CritSkillCounter global = new();

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

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class HitKillCounter : BallCounter
{
    public static HitKillCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}
public class SkillKillCounter : BallCounter
{
    public static SkillKillCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}