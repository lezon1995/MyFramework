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

public class HitCounter : Counter
{
    public static HitCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class CritHitCounter : Counter
{
    public static CritHitCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class ReflectCounter : Counter
{
    public static ReflectCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class PenetrateBrickCounter : Counter
{
    public static PenetrateBrickCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class HitBrickCounter : Counter
{
    public static HitBrickCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}

public class KillBrickCounter : Counter
{
    public static KillBrickCounter global = new();

    public override bool count(int delta = 1)
    {
        global.internalCount(delta);
        return base.count(delta);
    }
}