namespace MarbleHero;

public class LightingStrike : Buff, IDoAttackEffect
{
    public void onTrigger(Player player, Ball ball, Brick brick)
    {
        if (randomHit(0.5F))
        {
            Brick randomBrick = brickManager.getRandomBrick(brick);
            if (randomBrick)
            {
                gameplayManager.handleHitDamage(ball, randomBrick);
            }
        }
    }
}