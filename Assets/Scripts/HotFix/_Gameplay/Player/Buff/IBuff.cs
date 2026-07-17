namespace MoreMountains;

public interface IDoAttackEffect
{
    void onDoAttack(APlayer player, Ball ball, Brick brick);
}
public interface IDoAbilityEffect
{
    void onDoAbility(APlayer player, Ball ball, Brick brick);
}
public interface IDoAttackKillEffect
{
    void onDoAttackKill(APlayer player, Ball ball, Brick brick);
}
public interface IDoKillEffect
{
    void onDoKill(APlayer player, Ball ball, Brick brick);
}

public interface IHitEnterBrick
{
    void onDoAttack(APlayer player, Ball ball, Brick brick);
}

public class BuffObject : ClassObject
{
    protected int level = 1;

    public override void resetProperty()
    {
        base.resetProperty();
        level = 1;
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