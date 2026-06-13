using UnityEngine;

namespace MarbleHero;

public class Border : MovableObject, IHittable
{
    protected SpriteRenderer renderer;

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

        obj.find(out renderer, "Renderer");
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
        int instanceID = c.gameObject.GetInstanceID();
        if (ballManager.getActiveBall(instanceID, out var ball))
            onBallEnter(ball);
    }

    protected void onTriggerExit(Collider c)
    {
        int instanceID = c.gameObject.GetInstanceID();
        if (ballManager.getActiveBall(instanceID, out var ball))
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
}

public class HBorder : Border
{
    public void setWidth(float width)
    {
        var size = renderer.size;
        size.x = width;
        renderer.size = size;
    }
}

public class VBorder : Border
{
    public void setHeight(float height)
    {
        var size = renderer.size;
        size.y = height;
        renderer.size = size;
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