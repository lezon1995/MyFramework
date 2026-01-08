using Drawing;
using UnityEngine;

namespace MarbleHero;

public abstract class AGameAction : ClassObject
{
    protected const float DEFAULT_DURATION = 0.5F;

    protected ACreature source, target;
    protected float duration, startDuration;
    public bool isDone;

    public override void resetProperty()
    {
        base.resetProperty();
        startDuration = duration = DEFAULT_DURATION;
        source = null;
        target = null;
        isDone = false;
    }

    public abstract void update(float dt);

    public virtual void fixedUpdate(float dt)
    {
    }

    protected void tickDuration(float dt)
    {
        Draw.ingame.xy.Label2D(new Vector2(0F, Screen.height / 4F), $"({duration:F2}) {GetType().Name}", 20, LabelAlignment.Center, Color.green);
        duration = clampMin(duration - dt);
        if (duration <= 0.0F)
            isDone = true;
    }

    protected bool shouldCancelAction()
    {
        return target == null || source is { isDying: true } || target.isDeadOrEscaped();
    }
}