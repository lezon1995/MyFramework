using UnityEngine;

namespace MarbleHero;

public class BrickCollider : GameComponent
{
    const float offset = 0.06F;
    GameObject gameObject;

    PolygonCollider2D polygon;

    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Brick brick)
        {
            var obj = brick.gameObject;
            gameObject = obj;
            findComponent(obj, out polygon);
        }
    }

    public override void destroy()
    {
        base.destroy();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        gameObject = null;
        polygon = null;
    }

    public void setColliderEnabled(bool enabled)
    {
        polygon.enabled = enabled;
        new OnBrickColliderChanged().trigger();
    }

    public void setSize(float width, float height)
    {
        var points = polygon.points;
        var x = width / 2F;
        var y = height / 2F;
        points[0] = new(x - offset, y);
        points[1] = new(-(x - offset), y);
        points[2] = new(-x, y - offset);
        points[3] = new(-x, -(y - offset));
        points[4] = new(-(x - offset), -y);
        points[5] = new(x - offset, -y);
        points[6] = new(x, -(y - offset));
        points[7] = new(x, y - offset);
        polygon.points = points;
    }
}