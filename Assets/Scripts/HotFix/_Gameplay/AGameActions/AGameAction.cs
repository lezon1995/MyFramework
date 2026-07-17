using Drawing;
using UnityEngine;

namespace MoreMountains;

public abstract class AGameAction : ClassObject
{
    protected const float DEFAULT_DURATION = 0.5F;

    protected ACreature source, target;
    protected Timer duration;
    public bool isDone;

    public override void resetProperty()
    {
        base.resetProperty();
        duration = DEFAULT_DURATION;
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
        Draw.ingame.xy.Label2D(new Vector2(Screen.width / 4F, Screen.height / 4F), $"({duration.remain:F2} / {duration.duration:F2}) {GetType().Name}", 20, LabelAlignment.Center, Color.green);
        if (duration.update(dt, true))
            isDone = true;
    }

    protected bool shouldCancelAction()
    {
        return target == null || source is { isDying: true } || target.isDeadOrEscaped();
    }
}