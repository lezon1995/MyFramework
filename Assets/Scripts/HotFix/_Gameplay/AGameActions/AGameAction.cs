namespace MarbleHero;

public abstract class AGameAction : ClassObject
{
    protected const float DEFAULT_DURATION = 0.5F;

    public bool isDone;

    protected ACreature target;
    protected ACreature source;
    protected float duration;
    protected float startDuration;

    public override void resetProperty()
    {
        base.resetProperty();
        startDuration = duration = DEFAULT_DURATION;
        source = null;
        target = null;
        isDone = false;
    }

    protected static void addToBot(AGameAction action) => actionManager.addToBot(action);
    protected static void addToTop(AGameAction action) => actionManager.addToTop(action);

    public abstract void update(float dt);

    public virtual void fixedUpdate(float dt)
    {
    }

    protected void tickDuration(float dt)
    {
        duration = clampMin(duration - dt);
        if (duration <= 0.0F)
            isDone = true;
    }

    protected bool shouldCancelAction()
    {
        return target == null || source is { isDying: true } || target.isDeadOrEscaped();
    }
}