using MoreMountains.Tools;
using UnityEngine;

namespace MarbleHero;

public class BrickCollider : GameComponent
{
    Transform transform;

    BoxCollider2D collider;
    Vector2 offset, size;

    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Brick brick)
        {
            var obj = brick.transform;
            if (obj.find(out collider))
            {
                offset = collider.offset;
                size = collider.size;
            }

            transform = obj;
        }
    }

    public override void destroy()
    {
        base.destroy();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        transform = null;
        collider = null;
        offset = default;
        size = default;
    }

    public void setColliderEnabled(bool enabled)
    {
        collider.enabled = enabled;
        new OnBrickColliderChanged().trigger();
    }

    public void setSize(float width, float height)
    {
        return;
        /*var points = polygon.points;
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
        polygon.points = points;*/
    }

    public Rect getRect()
    {
        Vector2 pos = transform.position;
        return new(pos + offset - size * 0.5F, size);
    }
}