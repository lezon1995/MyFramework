using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Ball : IEventRouter
        , IEvent<OnBrickColliderChanged>
        , IEvent<DoHitEffect>
        , IEvent<DoSkillEffect>
        , IEvent<DoAttackKillEffect>
    {
        public IEventRouter Event => this;

        protected void addListeners() => Event.addAllListeners(this);
        protected void removeListeners() => Event.removeAllListeners(this);

        protected virtual bool onHitEnter(Collider2D c, Vector2 normal)
        {
            if (c == null)
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
                OBSTACLE_LAYER => onHitEnterObstacle(c, normal),
                _ => false
            };
        }

        protected virtual bool onHitEnterBrick(Collider2D c, Vector2 normal)
        {
            hasBeenCollided = true;

            if (c.TryGetComponent(out Brick brick))
            {
                lastHittable = brick;
                var ball = this;
                var dmg = ball.getHitDmg(brick, normal);
                brick.onHitEnter(ball, normal);
                ball.onHitEnter(brick, normal, out var triggerRegularHit);
                collidingBrick = brick;

                if (triggerRegularHit)
                {
                    counters.hit.count();
                    counters.hitBrick.count();
                    if (IsPenetrable)
                        setDirection(getDirection());
                    else
                        reflectBounce(normal, true);
                }

                ball.DamageOnTouch.Colliding(brick, dmg);
            }

            return true;
        }

        protected virtual bool onHitEnterObstacle(Collider2D c, Vector2 normal)
        {
            var ball = this;

            if (c.TryGetComponent(out Obstacle obstacle))
            {
                lastHittable = obstacle;
                foreach (var p in powers)
                    p.onHitObstacle(obstacle);

                counters.hit.count();
                hasBeenCollided = true;

                player.onBallHitObstacle(ball, obstacle, ref normal);
                reflectBounce(normal);
                playHitObstacleSfx();

                var dmg = ball.getHitDmg(obstacle, normal);
                ball.DamageOnTouch.Colliding(obstacle, dmg);
                return true;
            }

            return false;
        }


        protected virtual bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            playHitBrickSfx();
            playHitBrickVfx();
            triggerRegularHit = true;
            foreach (var p in powers)
                p.onHitBrick(brick, normal);

            player.onBallHitBrick(this, brick, normal, ref triggerRegularHit);
            return true;
        }

        protected virtual bool onHitEnter(BorderTop border, Vector2 normal)
        {
            var ball = this;

            lastHittable = border;
            foreach (var p in powers)
                p.onHitBorder(border);

            player.onBallHitBorderTop(ball, border, ref normal);
            counters.hit.count();
            hasBeenCollided = true;
            reflectBounce(normal);
            playHitBorderSfx();

            var dmg = ball.getHitDmg(border, normal);
            ball.DamageOnTouch.Colliding(border, dmg);
            return true;
        }

        protected virtual bool onHitEnter(BorderBot border, Vector2 normal)
        {
            var ball = this;

            lastHittable = border;
            bool forceReturn = true;
            player.onBallHitBorderBot(ball, border, normal, ref forceReturn);
            if (forceReturn)
            {
                if (ball.isTemp)
                {
                    ball.Health.Kill();
                    return true;
                }

                if (hasBeenCollided)
                {
                    player.setBallReturn(ball);
                }
            }
            else
            {
                foreach (var p in powers)
                    p.onHitBorder(border);

                counters.hit.count();
                reflectBounce(normal);
                playHitBorderSfx();
            }

            var dmg = ball.getHitDmg(border, normal);
            ball.DamageOnTouch.Colliding(border, dmg);
            return true;
        }

        protected virtual bool onHitEnter(BorderLeft border, Vector2 normal)
        {
            var ball = this;

            lastHittable = border;
            foreach (var p in powers)
                p.onHitBorder(border);

            counters.hit.count();
            hasBeenCollided = true;
            if (horizontalBorderTeleportable)
            {
                var dist = (curPos.x - border.getWorldPosition().x).abs();
                var teleportedX = levelManager.borderRight.getWorldPosition().x + dist;
                setTeleportPosition(new(teleportedX, curPos.y), BORDER_RIGHT_LAYER_MASK);
            }
            else
            {
                player.onBallHitBorderLeft(ball, border, ref normal);
                reflectBounce(normal);
                playHitBorderSfx();
            }

            var dmg = ball.getHitDmg(border, normal);
            ball.DamageOnTouch.Colliding(border, dmg);
            return true;
        }

        protected virtual bool onHitEnter(BorderRight border, Vector2 normal)
        {
            var ball = this;

            lastHittable = border;
            foreach (var p in powers)
                p.onHitBorder(border);

            counters.hit.count();
            hasBeenCollided = true;
            if (horizontalBorderTeleportable)
            {
                var dist = (curPos.x - border.getWorldPosition().x).abs();
                var teleportedX = levelManager.borderLeft.getWorldPosition().x - dist;
                setTeleportPosition(new(teleportedX, curPos.y), BORDER_LEFT_LAYER_MASK);
            }
            else
            {
                player.onBallHitBorderRight(ball, border, ref normal);
                reflectBounce(normal);
                playHitBorderSfx();
            }

            var dmg = ball.getHitDmg(border, normal);
            ball.DamageOnTouch.Colliding(border, dmg);
            return true;
        }

        public virtual bool onCritHit(Brick brick)
        {
            counters.critHit.count();
            return true;
        }

        public virtual bool onCritSkill(Brick brick)
        {
            counters.critSkill.count();
            return true;
        }

        protected virtual bool onKill(Brick brick)
        {
            player.onBallKillBrick(this, brick);
            return true;
        }

        public virtual bool onHitKill(Brick brick)
        {
            onKill(brick);
            counters.hitKill.count();
            return true;
        }

        public virtual bool onSkillKill(Brick brick)
        {
            onKill(brick);
            counters.skillKill.count();
            return true;
        }

        public void onEvent(OnBrickColliderChanged e)
        {
            refreshHitInfo();
        }

        public virtual void onEvent(DoHitEffect e)
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


        public virtual void onEvent(DoSkillEffect e)
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
}