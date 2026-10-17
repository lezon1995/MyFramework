using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Ball
    {
        const float PHYSICS_CAST_DISTANCE = 100F;
        protected Comparison<RaycastHit2D> comparison;
        public virtual BallType BallType => BallType.Normal;

        public int instanceID; //GameObject的instanceID，可以根据不同GameObject而变化
        public long guid; // Ball这个对象的guid，
        public BallDef Def;

        #region Stats

        public bool isTemp { get; set; } //是否是临时生成出来的球

        public void refreshInitialHealth()
        {
            var health = int.MaxValue;
            _health.SetHealth(health, health);
        }

        public void setInitialHealth(int value)
        {
            _health.SetHealth(value, value);
        }

        public void setHealth(int value)
        {
            _health.SetHealth(value);
        }

        public void setPenetrable(bool value)
        {
            IsPenetrable = value;
        }

        public void setTemp(bool value)
        {
            isTemp = value;
        }

        public void setHorizontalBorderTeleportable(bool value) { }

        #endregion

        protected List<Buff> buffs = new();
        public List<BallPower> powers = new();

        public BallRenderer ballRenderer;

        protected APlayer _player;
        public Brick collidingBrick { get; set; }
        protected Brick overlappingBrick;

        public APlayer collidingPlayer { get; set; }
        protected APlayer overlappingPlayer;
        
        public Obstacle collidingObstacle { get; set; }
        protected Obstacle overlappingObstacle;

        BorderToBallDamageModifier borderToBallDamageModifier;

        public Vector3 targetPos;
        protected Vector2 lastDirection;
        protected Vector2 lastHitNormal;
        protected bool hasCorrectPosThisFixedUpdate;
        public BallCounters counters = new();

        protected float movementDelta;
        float lastRadius;
        bool enabled;
        public bool hasBeenCollided { get; set; }
        int delayCounter;

        protected Countdown safePassCounter;
        protected Timer lifeDuration;
        protected Timer dashHitTimer;
        public bool isDashHitDisable { get; set; }
        public bool insidePlayer { get; set; }

        public bool isExpired { get; set; }
        public bool Recollectable = true;

        public IHittable lastHittable;
        public bool isOverlappingBrick { get; set; }
        public bool isOverlappingPlayer { get; set; }
        public int level = 1;
        public int levelIndex => level - 1;

        public void setBorderToBallDamageModifier(BorderToBallDamageModifier m) => borderToBallDamageModifier = m;
        public void setID(long id) => guid = id;
        public void setDef(BallDef def) => Def = def;
        public long getGUID() => guid;

        public Ball()
        {
            comparison = Comparison;
        }

        public override void onRelease()
        {
            lastHittable = null;
            prePos = curPos = targetPos = Vector2.zero;
            movementDelta = 0;
            Direction = Vector2.zero;
            lastHitNormal = Vector2.zero;
            lastRadius = 0;
            lastDirection = default;
            enabled = false;
            hasBeenCollided = false;
            isTemp = false;
            removeAllPowers();

            if (Stats)
                Stats.ClearStats();

            base.onRelease();
        }

        public void setPlayer(APlayer p) => _player = p;
        public APlayer getPlayer() => _player;

        public override void OnUpdate(float dt)
        {
            base.OnUpdate(dt);

            if (!enabled)
                return;

            float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
            if (hasCorrectPosThisFixedUpdate)
            {
                t = 1F;
            }

            var p = Vector3.Lerp(prePos, curPos, t);
            setPosition(p);

            // Draw.ingame.xy.Circle(p, radius, Color.red);
        }

        protected override void FixedUpdate()
        {
            if (mNeedFixedUpdate)
            {
                var dt = Time.fixedDeltaTime;
                if (!enabled)
                    return;

                OnFixedUpdate(dt);
            }
        }

        public override void OnFixedUpdate(float dt)
        {
            FixedUpdatePowers(dt);
            CheckBrickHitTimerExpiration(dt);

            if (!_shouldMove)
                return;

            if (IsPenetrable)
            {
                TryMoveWithCollidingObject(dt, collidingBrick != null || collidingPlayer != null, isOverlappingBrick || isOverlappingPlayer);
                
                if (ManuallyColliding)
                {
                    if (!TryCollideWithCollidingObject(collidingBrick != null || collidingPlayer != null))
                        return;
                }
                
                CheckOverlappingBrick(dt);
            }
            else
            {
                TryMoveWithCollidingObject(dt, collidingPlayer != null, isOverlappingPlayer);
                
                if (ManuallyColliding)
                {
                    if (!TryCollideWithCollidingObject(collidingPlayer != null))
                        return;
                }
                
                CheckOverlappingPlayer(dt);
            }

            if (FaceMovement)
                FaceMovementDirection(Direction);

            CheckBallExpiration(dt);
            CheckDashHitExpiration(dt);
        }

        bool TryCollideWithCollidingObject(bool isColliding)
        {
            if (insidePlayer)
            {
                return true;
            }
            
            if (isColliding)
            {
                if (willPassingThroughThisFrame)
                {
                    CollidingManually(willPassingThroughHit.collider.gameObject, willPassingThroughHit.normal, willPassingThroughHit.point);
                    willPassingThroughThisFrame = false;
                    correctPos = Vector3.zero;
                    willPassingThroughHit = default;
                }
            }
            else
            {
                if (willPassingThroughThisFrame && curPos == correctPos)
                {
                    CollidingManually(willPassingThroughHit.collider.gameObject, willPassingThroughHit.normal, willPassingThroughHit.point);
                    willPassingThroughThisFrame = false;
                    correctPos = Vector3.zero;
                    willPassingThroughHit = default;
                    return true;
                }
            }

            return true;
        }

        void TryMoveWithCollidingObject(float dt, bool isColliding, bool isOverlapping)
        {
            if (isColliding)
            {
                if (isOverlapping)
                {
                    willPassingThroughThisFrame = false;
                }
                else
                {
                    willPassingThroughThisFrame = CheckWillPassingThrough(dt, BounceLayers, out correctPos, out willPassingThroughHit);
                }

                Movement(dt);
            }
            else
            {
                willPassingThroughThisFrame = CheckWillPassingThrough(dt, BounceLayers, out correctPos, out willPassingThroughHit);
                if (willPassingThroughThisFrame)
                {
                    MovementTo(correctPos);
                }
                else
                {
                    Movement(dt);
                }
            }
        }

        void CheckOverlappingBrick(float dt)
        {
            if (collidingBrick)
            {
                if (collidingBrick.IsAlive() && circleIntersectRectangle(getCircle(), collidingBrick.getRect()))
                {
                    if (overlappingBrick != collidingBrick)
                    {
                        if (!isOverlappingBrick)
                        {
                            isOverlappingBrick = true;
                        }
                        else
                        {
                            //如果上一次的Overlapping还未结束，则提前结束上一次的Overlapping
                            var lastOverlappingBrick = overlappingBrick;
                            _player.onBallEndOverlappingBrickOne(this, lastOverlappingBrick, true);
                        }

                        var noOverlappingBefore = overlappingBrick == null;
                        overlappingBrick = collidingBrick;
                        if (noOverlappingBefore)
                            _player.onBallBeginOverlappingBrickAll(this, overlappingBrick);

                        _player.onBallBeginOverlappingBrickOne(this, overlappingBrick);
                    }
                    else
                    {
                        OnFixedUpdateOverlappingBrick(overlappingBrick, dt);
                    }
                }
                else
                {
                    if (isOverlappingBrick)
                    {
                        isOverlappingBrick = false;
                        _player.onBallEndOverlappingBrickOne(this, overlappingBrick, false);
                        _player.onBallEndOverlappingBrickAll(this, overlappingBrick, false);
                        overlappingBrick = null;
                    }

                    collidingBrick = null;
                }
            }
        }

        void CheckOverlappingPlayer(float dt)
        {
            if (collidingPlayer)
            {
                if (collidingPlayer.IsAlive() && circleIntersectRectangle(getCircle(), collidingPlayer.getRect()))
                {
                    if (overlappingPlayer != collidingPlayer || collidingBrick != null || collidingObstacle != null)
                    {
                        if (!isOverlappingPlayer)
                        {
                            isOverlappingPlayer = true;
                        }
                        else
                        {
                            //如果上一次的Overlapping还未结束，则提前结束上一次的Overlapping
                            var lastOverlappingPlayer = overlappingPlayer;
                            _player.onBallEndOverlappingPlayerOne(this, lastOverlappingPlayer, true);
                        }

                        if (collidingBrick != null || collidingObstacle != null)
                        {
                            collidingPlayer = null;
                            overlappingPlayer = null;
                            return;
                        }
                        
                        var noOverlappingBefore = overlappingPlayer == null;
                        overlappingPlayer = collidingPlayer;
                        if (noOverlappingBefore)
                            _player.onBallBeginOverlappingPlayerAll(this, overlappingPlayer);

                        _player.onBallBeginOverlappingPlayerOne(this, overlappingPlayer);
                    }
                    else
                    {
                        OnFixedUpdateOverlappingPlayer(overlappingPlayer, dt);
                    }
                }
                else
                {
                    if (isOverlappingPlayer)
                    {
                        isOverlappingPlayer = false;
                        _player.onBallEndOverlappingPlayerOne(this, overlappingPlayer, false);
                        _player.onBallEndOverlappingPlayerAll(this, overlappingPlayer, false);
                        overlappingPlayer = null;
                    }

                    collidingPlayer = null;
                }
            }
        }

        protected void FixedUpdatePowers(float dt)
        {
            foreach (var p in powers)
                p.onFixedUpdate(dt);
        }

        protected virtual void OnFixedUpdateOverlappingBrick(Brick brick, float dt) { }
        protected virtual void OnFixedUpdateOverlappingPlayer(APlayer p, float dt) { }

        protected virtual void CheckBallExpiration(float dt)
        {
            var deltaTime = _player.ballDurationElapsedSpeed.value * dt;
            if (lifeDuration.update(deltaTime) || isExpired)
            {
                new OnBallExpired(this).trigger(this);
                _health.Kill();
                isExpired = true;
            }
        }

        protected void CheckDashHitExpiration(float dt)
        {
            if (dashHitTimer.update(dt))
            {
                dashHitTimer = 0;
                isDashHitDisable = false;
            }
        }

        protected void CheckBrickHitTimerExpiration(float dt)
        {
            if (brickHitTimers.count() > 0)
            {
                foreach (var (brick, timer) in brickHitTimers)
                {
                    if (timer.update(dt))
                    {
                        brickHitTimers.remove(brick);
                        timer.release();
                    }
                }
            }
        }

        public Vector2 getDirection()
        {
            return Direction;
        }

        public Vector2 getVelocity()
        {
            return Direction * moveSpeed;
        }

        public Vector3 getKnockbackForce(Health colliderHealth, bool isLethal)
        {
            Vector3 force;
            if (isLethal)
                force = _damageOnTouch.LethalDamageKnockbackForce;
            else
                force = _damageOnTouch.DamageKnockbackForce;

            float knockbackValue = 0F;
            var b1 = GetStat(Ball.Stat.Knockback, out var knockbackStat1);
            var b2 = getPlayer().GetStat(Character.Stat.Knockback, out var knockbackStat2);
            if (b1)
                knockbackValue += knockbackStat1.Value;
            if (b2)
                knockbackValue += knockbackStat2.Value;

            var knockbackForce = force * (colliderHealth.KnockbackForceMultiplier + knockbackValue);
            knockbackForce = getDirection() * knockbackForce.magnitude;
            return knockbackForce;
        }

        public void setDirection(Vector2 dir)
        {
            lastDirection = Direction;
            Direction = dir.normalized;
        }

        public void setShootDirection(Vector2 dir)
        {
            lastDirection = Direction;
            Direction = dir.normalized;
        }

        public void setEnabled(bool b)
        {
            enabled = b;
        }

        int Comparison(RaycastHit2D h1, RaycastHit2D h2)
        {
            var d1 = Vector2.Distance(curPos, h1.point);
            var d2 = Vector2.Distance(curPos, h2.point);
            return d1.CompareTo(d2);
        }

        public void setTeleportPosition(Vector2 pos)
        {
            prePos = curPos = pos;
            _rigidBody2D.position = pos;
            setPosition(pos);
            setDirection(Direction);
            ballRenderer.clearTrail();
        }

        public void setRendererActive(bool active) => ballRenderer.setRendererActive(active);

        public void setLevel(int lv)
        {
            level = lv;
            ballRenderer.setLevel(lv);

            foreach (var (key, stat) in _stats.AllStats)
                stat.InitialOverride?.SetDirty();

            onLevelSetup(lv);
        }

        public void setRadius(float value)
        {
            lastRadius = Radius;
            Radius = value;
        }

        public Circle2 getCircle()
        {
            return new(curPos, Radius);
        }

        void checkRadius()
        {
            if (lastRadius.isEqual(Radius))
                return;

            setRadius(Radius);
        }

        public virtual int getImpactDamage()
        {
            float hitDamage = 0;
            if (GetStat(Stat.AD, out var ballHitDamage))
            {
                hitDamage += ballHitDamage.Value;
            }

            if (Player.GetStat(Character.Stat.AD, out var characterAD))
            {
                GetStat(Stat.HitDamageRate, out var ballHitDamageRate);
                hitDamage += (characterAD.Value * ballHitDamageRate.Value);
            }

            return (int)hitDamage;
        }

        public int getSkillDamage()
        {
            float effectDamage = 0;
            if (GetStat(Stat.AP, out var ballEffectDamage))
            {
                effectDamage += ballEffectDamage.Value;
            }

            if (Player.GetStat(Character.Stat.AP, out var characterAP))
            {
                GetStat(Stat.EffectDamageRate, out var ballEffectDamageRate);
                effectDamage += (characterAP.Value * ballEffectDamageRate.Value);
            }

            return (int)effectDamage;
        }

        public virtual bool getSelfDamage(Brick brick, out int selfDamage)
        {
            selfDamage = 1;
            return true;
        }

        public virtual bool getSelfDamage(Border border, out int selfDamage)
        {
            selfDamage = 1;
            if (borderToBallDamageModifier == null)
                return true;

            return borderToBallDamageModifier(ref selfDamage);
        }

        public virtual bool getSelfDamage(Obstacle obstacle, out int selfDamage)
        {
            selfDamage = 1;
            if (borderToBallDamageModifier == null)
                return true;

            return borderToBallDamageModifier(ref selfDamage);
        }

        public virtual bool getSelfDamage(APlayer p, out int selfDamage)
        {
            selfDamage = 0;
            return false;
        }

        public virtual Dmg getImpactDmg(Brick brick, Vector2 normal)
        {
            var d = getImpactDamage();
            var dmg = Dmg.AD(d);
            dmg.setAttackEffect();
            _player.GetStat(Character.Stat.DmgRate, out var playerDmgRate);
            GetStat(Stat.DmgRate, out var ballDmgRate);
            var dmgRate = playerDmgRate.Value * ballDmgRate.Value;
            dmg.SetDmgRate(dmgRate);
            dmg.setHitNormal(normal);

            _player.GetStat(Character.Stat.CritChance, out var playerCritChance);
            GetStat(Stat.CritChance, out var ballCritChance);
            var critChange = playerCritChance.Value + ballCritChance.Value;
            if (randomHit(critChange))
                dmg.Crit();

            _player.GetStat(Character.Stat.CritDamage, out var playerCritDamage);
            GetStat(Stat.CritDamage, out var ballCritDamage);
            var critDamage = playerCritDamage.Value * ballCritDamage.Value;
            dmg.SetCritDamage(critDamage);

            _player.onBallGetImpactDmg(this, ref dmg);
            return dmg;
        }

        public virtual Dmg getImpactDmg(Border border, Vector2 normal) => Dmg.True(0);
        public virtual Dmg getImpactDmg(Obstacle obstacle, Vector2 normal) => Dmg.True(0);
        public virtual Dmg getImpactDmg(APlayer p, Vector2 normal) => Dmg.True(0);

        public virtual Dmg getSkillDmg(Brick brick = null)
        {
            var d = getSkillDamage();
            var dmg = Dmg.AP(d);
            dmg.setSkillEffect();

            GetStat(Stat.DmgRate, out var stat);
            dmg.SetDmgRate(stat.Value);

            _player.onBallGetSkillDmg(this, ref dmg);
            return dmg;
        }

        public virtual void refreshDuration()
        {
            GetStat(Stat.Duration, out var ballDuration);
            var duration = ballDuration.Value * _player.durationPct;
            lifeDuration = duration.clampMin(1F);
            isExpired = false;
        }

        public void setInfiniteDuration()
        {
            lifeDuration = int.MaxValue;
            isExpired = false;
        }

        public void setDuration(float duration)
        {
            var d = duration.clampMin(0.4F);
            lifeDuration = d;
            isExpired = d <= 0 || d.isZero();
        }

        public void extendDuration(float delta)
        {
            lifeDuration.extend(delta);
            var d = lifeDuration.duration;
            isExpired = d <= 0 || d.isZero();
        }

        public float getDurationRemain()
        {
            return lifeDuration.remain;
        }

        public virtual void onPreparedToShoot() { }

        /*public void returnBall(Vector3 nextPosition)
        {
            setEnabled(false);
            Tween
                .Position(getTransform(), endValue: nextPosition, duration: 0.25f, ease: Ease.OutCubic)
                .OnComplete(this, ball =>
                {
                    ballManager.releaseBall(ball);
                });
        }*/

        public void addBuff(Buff buff)
        {
            buffs.add(buff);
        }

        public T addPower<T>() where T : BallPower
        {
            var power = CLASS<BallPower>(typeof(T));
            power.with(this);
            powers.add(power);
            power.onGainPower(this);
            return power as T;
        }

        public void removeAllPowers()
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                var power = powers[i];
                power.onLosePower(this);
                powers.removeAt(i);
                UN_CLASS(power);
            }
        }

        public bool removePower<T>() where T : BallPower
        {
            for (var i = powers.Count - 1; i >= 0; i--)
            {
                if (powers[i] is T power)
                {
                    power.onLosePower(this);
                    powers.removeAt(i);
                    UN_CLASS(power);
                    return true;
                }
            }

            return false;
        }
    }
}