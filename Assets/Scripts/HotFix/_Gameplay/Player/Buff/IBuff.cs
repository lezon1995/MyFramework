namespace MarbleHero;

public interface IDoAttackEffect
{
    void onDoAttack(Player player, Ball ball, Brick brick);
}
public interface IDoAttackKillEffect
{
    void onDoAttackKill(Player player, Ball ball, Brick brick);
}
public interface IDoKillEffect
{
    void onDoKill(Player player, Ball ball, Brick brick);
}

public interface IHitEnterBrick
{
    void onDoAttack(Player player, Ball ball, Brick brick);
}

public class Buff : ClassObject
{
    protected BrickManager brickManager;
    protected int level = 1;

    public override void resetProperty()
    {
        base.resetProperty();

        brickManager = null;
        level = 1;
    }

    public void setBrickManager(BrickManager manager)
    {
        brickManager = manager;
    }

    public virtual void setLevel(int v)
    {
        level = v;
    }

    public int getLevel()
    {
        return level;
    }

    public virtual int getMaxLevel()
    {
        return 5;
    }
}