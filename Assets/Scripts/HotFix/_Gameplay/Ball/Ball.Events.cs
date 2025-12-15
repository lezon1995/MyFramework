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
            BORDER_LEFT_LAYER => onHitEnter(levelManager.borderLeft, normal),
            BORDER_RIGHT_LAYER => onHitEnter(levelManager.borderRight, normal),
            BORDER_TOP_LAYER => onHitEnter(levelManager.borderTop, normal),
            BORDER_BOT_LAYER => onHitEnter(levelManager.borderBot, normal),
            BRICK_LAYER => onHitEnterBrick(c, normal),
            _ => false
        };
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