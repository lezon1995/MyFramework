using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Ball : IEventRouter
        , IEvent<DoHitEffect>
        , IEvent<DoSkillEffect>
        , IEvent<DoAttackKillEffect>
    {
        public IEventRouter Event => this;

        protected void addListeners() => Event.addAllListeners(this);
        protected void removeListeners() => Event.removeAllListeners(this);

        public virtual bool onImpactHitEnter(Brick brick, Vector2 normal, out bool triggerRegularHit)
        {
            var dmg = getImpactDmg(brick, normal);
            brick.onHitEnter(this, normal);

            playHitBrickSfx(brick);
            playHitBrickVfx(brick, normal);
            triggerRegularHit = true;
            foreach (var p in powers)
                p.onImpactHitBrick(brick, normal);

            _player.onBallImpactHitBrick(this, brick, normal, ref triggerRegularHit);

            brick.Health.Damage(ref dmg, gameObject, _player, DamageOnTouch.InvincibilityDuration, DamageOnTouch.DamageDirection);
            DamageOnTouch.ApplyKnockback(brick.Health, brick.Controller2D, dmg);

            onCauseImpactDamage(brick, in dmg);

            return true;
        }

        public virtual bool onSkillHitEnter(Brick b, Vector3 knockbackDir = default, float knockbackForce = 0F)
        {
            var dmg = getSkillDmg(b);

            _player.onBallSkillHitBrick(this, b);

            b.Health.Damage(ref dmg, gameObject, _player);
            var force = getKnockbackForce(b.Health, dmg.IsLethal);
            Vector3 knockback;
            if (knockbackDir.isZero())
                knockback = force * knockbackForce;
            else
                knockback = force.magnitude * knockbackDir;

            if (!knockback.isZero())
            {
                b.Health.ApplyKnockback(knockback, dmg);
            }

            onCauseSkillDamage(b, in dmg);

            return true;
        }

        public virtual bool onImpactHitEnter(APlayer p, Vector2 normal)
        {
            if (p.isBallReflectable)
            {
                // var dmg = getImpactDmg(p, normal);
                // p.onHitEnter(this, normal);

                foreach (var power in powers)
                    power.onHitPlayer(p);

                playHitPlayerSfx(p);
                playHitPlayerVfx(p, normal);
                // triggerRegularHit = true;
                // foreach (var power in powers)
                // power.onImpactHitPlayer(p, normal);

                _player.onBallImpactHitPlayer(this, p, normal);

                // p.Health.Damage(ref dmg, gameObject, _player, DamageOnTouch.InvincibilityDuration, DamageOnTouch.DamageDirection);
                // DamageOnTouch.ApplyKnockback(p.Health, p.Controller2D, dmg);

                // onCauseImpactDamage(p, in dmg);
            }

            return true;
        }

        public virtual bool onCauseImpactDamage(Brick brick, in Dmg dmg)
        {
            _player.onBallCauseImpactDamage(this, brick, in dmg);
            return true;
        }

        public virtual bool onCauseSkillDamage(Brick brick, in Dmg dmg)
        {
            _player.onBallCauseSkillDamage(this, brick, in dmg);
            return true;
        }

        public virtual bool onImpactCrit(Brick brick, in Dmg dmg)
        {
            counters.critHit.count();
            _player.onBallImpactCrit(this, brick, in dmg);
            return true;
        }

        public virtual bool onSkillCrit(Brick brick)
        {
            counters.critSkill.count();
            _player.onBallSkillCrit(this, brick);
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

        protected virtual void onBounceFinished() { }

        public virtual void onRecollected() { }

        protected virtual void onLevelSetup(int lv) { }

        public virtual bool onPlayerDashHit(Vector3 position, Vector3 direction)
        {
            isDashHitDisable = true;
            dashHitTimer = 0.2F;
            _player.onDashHitBall(this, position, direction);
            return false;
        }
    }
}