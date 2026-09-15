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

        public virtual bool onHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            playHitBrickSfx(brick);
            playHitBrickVfx(brick, normal);
            triggerRegularHit = true;
            foreach (var p in powers)
                p.onHitBrick(brick, normal);

            _player.onBallHitBrick(this, brick, normal, ref triggerRegularHit);
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
            _player.onBallKillBrick(this, brick);
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
                    effect.onDoAttack(_player, e.ball, e.brick);
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
                    effect.onDoAbility(_player, e.ball, e.brick);
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
                    effect.onDoAttackKill(_player, e.ball, e.brick);
                }
            }
        }

        protected virtual void onBounceFinished()
        {
        }

        public virtual void onRecollected()
        {
        }

        protected virtual void onLevelSetup(int lv)
        {
        }

        public virtual bool onPlayerDashHit(Vector3 position, Vector3 direction)
        {
            isDashHitDisable = true;
            dashHitTimer = 0.2F;
            return false;
        }
    }
}