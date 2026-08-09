using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains;

public class Border : MovableObject
    , IHittable
    , IEventRouter
{
    public IEventRouter Event => this;

    public override void resetProperty()
    {
        base.resetProperty();
    }

    public Border()
    {
    }

    public override void init()
    {
        base.init();
    }

    public override void setObject(GameObject obj)
    {
        base.setObject(obj);
    }

    protected override void initComponents()
    {
        base.initComponents();

        var trigger = getOrAddUnityComponent<ColliderTrigger>();
        trigger.onTriggerEnter = onTriggerEnter;
        trigger.onTriggerExit = onTriggerExit;
    }

    protected void onTriggerEnter(Collider c)
    {
        if (c.TryGetComponent(out Ball ball))
            onBallEnter(ball);
    }

    protected void onTriggerExit(Collider c)
    {
        if (c.TryGetComponent(out Ball ball))
            onBallExit(ball);
    }

    protected virtual void onBallEnter(Ball ball)
    {
        // var curDir = ball.getDirection();
        // var b = Physics.Raycast(ball.getPosition(), curDir, out var hit, 10F, BORDER_LAYER_MASK, QueryTriggerInteraction.Collide);
        // if (b)
        // {
        //     var reflectDir = Vector3.Reflect(curDir, hit.normal);
        //     float angle = Vector3.Angle(curDir, reflectDir);
        //     var newDir = reflectDir.normalized;
        //     ball.setDirection(newDir);
        // }
    }

    protected virtual void onBallExit(Ball ball)
    {
    }

    public void takeDamage(ref Dmg dmg, GameObject instigator, Ball source, float invincibleTime = 0, Vector3 direction = default, IDmgCalculator calculator = null)
    {
    }
}

public class HBorder : Border
{
    public void setWidth(float width)
    {
    }
}

public class VBorder : Border
{
    public void setHeight(float height)
    {
    }
}

public class BorderTop : HBorder
{
}

public class BorderBot : HBorder
{
}

public class BorderLeft : VBorder
{
}

public class BorderRight : VBorder
{
}