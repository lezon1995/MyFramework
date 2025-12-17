namespace MarbleHero;

public interface IDoAttackEffect
{
    void onTrigger(Player player, Ball ball, Brick brick);
}

public class Buff : ClassObject
{
    BrickManager brickManager;

    public void setBrickManager(BrickManager manager)
    {
        brickManager = manager;
    }
}