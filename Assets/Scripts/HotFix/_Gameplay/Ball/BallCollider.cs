namespace MarbleHero;

public class BallCollider : GameComponent
{
    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Ball ball)
        {
            var obj = ball.gameObject;
        }
    }
    
    public override void resetProperty()
    {
        base.resetProperty();
    }
}