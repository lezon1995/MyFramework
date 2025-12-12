using UnityEngine;

namespace MarbleHero;

public partial class Ball : IEventRouter, IEvent<OnBrickColliderChanged>
{
    public IEventRouter eventRouter => this;

    protected virtual bool onHitEnter(Collider2D c, Vector2 normal)
    {
        var layer = c.gameObject.layer;
        return layer switch
        {
            BORDER_LAYER => onHitEnterBorder(c, normal),
            BRICK_LAYER => onHitEnterBrick(c, normal),
            _ => false
        };
    }

    protected virtual bool onHitEnterBorder(Collider2D c, Vector2 normal)
    {
        if (c.CompareTag(BORDER_TOP_TAG))
            return onHitEnter(levelManager.borderTop, normal);

        if (c.CompareTag(BORDER_BOT_TAG))
            return onHitEnter(levelManager.borderBot, normal);

        if (c.CompareTag(BORDER_LEFT_TAG))
            return onHitEnter(levelManager.borderLeft, normal);

        if (c.CompareTag(BORDER_RIGHT_TAG))
            return onHitEnter(levelManager.borderRight, normal);

        return false;
    }

    protected virtual bool onHitEnterBrick(Collider2D c, Vector2 normal)
    {
        if (brickManager.getBrick(c.gameObject.GetInstanceID(), out var brick))
        {
            var ball = this;
            gameplayManager.handleHitDamage(ball, brick);
            brick.onHitEnter(ball, normal);
            ball.onHitEnter(brick, normal);
        }

        reflectBounce(normal);
        return true;
    }

    protected virtual bool onHitEnter(Brick brick, Vector2 normal)
    {
        return true;
    }

    protected virtual bool onHitEnter(BorderTop border, Vector2 normal)
    {
        reflectBounce(normal);
        return true;
    }

    protected virtual bool onHitEnter(BorderBot border, Vector2 normal)
    {
        reflectBounce(normal);
        return true;
    }

    protected virtual bool onHitEnter(BorderLeft border, Vector2 normal)
    {
        reflectBounce(normal);
        return true;
    }

    protected virtual bool onHitEnter(BorderRight border, Vector2 normal)
    {
        reflectBounce(normal);
        return true;
    }

    public void onEvent(OnBrickColliderChanged e)
    {
        refreshHitInfo();
    }
}