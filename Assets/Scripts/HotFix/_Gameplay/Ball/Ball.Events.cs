using UnityEngine;

namespace MarbleHero;

public partial class Ball : IEventRouter
    , IEvent<OnBrickColliderChanged>
    , IEvent<DoAttackEffect>
    , IEvent<DoAbilityEffect>
    , IEvent<DoAttackKillEffect>
{
    public IEventRouter eventRouter => this;
    
    protected void addListeners() => eventRouter.addAllListener(this);
    protected void removeListeners() => eventRouter.removeAllListener(this);

    protected virtual bool onHitEnter(Collider2D c, Vector2 normal)
    {
        if (hitCollider == null)
        {
            Debug.LogError("onHitEnter collider == null");
            return false;
        }

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
        hasBeenCollided = true;
        if (brickManager.getActiveBrick(c.gameObject.GetInstanceID(), out var brick))
        {
            var ball = this;
            gameplayManager.handleAttackDamage(ball, brick);
            brick.onHitEnter(ball, normal);
            ball.onHitEnter(brick, normal);
        }

        if (isPenetrable)
        {
            setDirection(getDirection());
        }
        else
        {
            reflectBounce(normal);
        }

        return true;
    }

    protected virtual bool onHitEnter(Brick brick, Vector2 normal)
    {
        return true;
    }

    protected virtual bool onHitEnter(BorderTop border, Vector2 normal)
    {
        hasBeenCollided = true;
        reflectBounce(normal);
        return true;
    }

    protected virtual bool onHitEnter(BorderBot border, Vector2 normal)
    {
        if (hasBeenCollided)
        {
            player.setBallReturn(this);
        }

        return true;
    }

    protected virtual bool onHitEnter(BorderLeft border, Vector2 normal)
    {
        hasBeenCollided = true;
        if (horizontalBorderTeleportable)
        {
            var dist = abs(curPos.x - border.getWorldPosition().x);
            var teleportedX = levelManager.borderRight.getWorldPosition().x + dist;
            setTeleportPosition(new(teleportedX, curPos.y), BORDER_RIGHT_LAYER_MASK);
        }
        else
        {
            reflectBounce(normal);
        }

        return true;
    }

    protected virtual bool onHitEnter(BorderRight border, Vector2 normal)
    {
        hasBeenCollided = true;
        if (horizontalBorderTeleportable)
        {
            var dist = abs(curPos.x - border.getWorldPosition().x);
            var teleportedX = levelManager.borderLeft.getWorldPosition().x - dist;
            setTeleportPosition(new(teleportedX, curPos.y), BORDER_LEFT_LAYER_MASK);
        }
        else
        {
            reflectBounce(normal);
        }

        return true;
    }

    public void onEvent(OnBrickColliderChanged e)
    {
        refreshHitInfo(true);
    }

    public void onEvent(DoAttackEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAttackEffect effect)
            {
                effect.onDoAttack(player, e.ball, e.brick);
            }
        }
    }
    
    
    public void onEvent(DoAbilityEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAbilityEffect effect)
            {
                effect.onDoAbility(player, e.ball, e.brick);
            }
        }
    }

    public void onEvent(DoAttackKillEffect e)
    {
        for (var i = 0; i < buffs.Count; i++)
        {
            var b = buffs[i];
            if (b is IDoAttackKillEffect effect)
            {
                effect.onDoAttackKill(player, e.ball, e.brick);
            }
        }
    }
}