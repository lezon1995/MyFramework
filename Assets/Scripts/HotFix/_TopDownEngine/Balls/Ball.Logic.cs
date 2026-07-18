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

        public int instanceID; //GameObject的instanceID，可以根据不同GameObject而变化
        protected Type type;
        public long guid; // Ball这个对象的guid，

        #region Stats

        public bool isTemp; //是否是临时生成出来的球
        public bool horizontalBorderTeleportable; //是否可在左右边界来回传送
        public bool usePhysics;

        public void setInitialHealth(int value)
        {
            _health.SetHealth(value, value, RefreshHealthBarType.Immediately);
        }

        public void setHealth(int value)
        {
            _health.SetHealth(value, RefreshHealthBarType.Immediately);
        }

        public void setPenetrable(bool value)
        {
            IsPenetrable = value;
        }

        public void setTemp(bool value)
        {
            isTemp = value;
        }

        public void setHorizontalBorderTeleportable(bool value)
        {
            horizontalBorderTeleportable = value;
        }

        #endregion

        protected List<Buff> buffs = new();
        public List<BallPower> powers = new();

        public BallRenderer ballRenderer;

        APlayer player;
        Brick collidingBrick;
        Brick overlappingBrick;

        BorderToBallDamageModifier borderToBallDamageModifier;

        public Vector3 targetPos;
        Vector2 lastDirection;
        Vector2 hitNormal;
        bool hasCorrectPosThisFixedUpdate;
        public BallCounters counters = new();

        float movementDelta;
        float lastRadius;
        bool enabled;
        bool hasBeenCollided;
        int delayCounter;

        public IHittable lastHittable;
        public bool isOverlappingBrick;

        public void setBorderToBallDamageModifier(BorderToBallDamageModifier m) => borderToBallDamageModifier = m;
        public void setBallType(Type t) => type = t;
        public void setID(long id) => guid = id;
        public Type getType() => type;
        public long getGUID() => guid;

        public Ball()
        {
            comparison = Comparison;
        }

        public override void onAcquire()
        {
            base.onAcquire();
            this.addListener<OnBrickColliderChanged>();
        }

        public override void onRelease()
        {
            lastHittable = null;
            prePos = curPos = targetPos = Vector2.zero;
            movementDelta = 0;
            Direction = Vector2.zero;
            hitNormal = Vector2.zero;
            lastRadius = 0;
            lastDirection = default;
            enabled = false;
            hasBeenCollided = false;
            removeAllPowers();

            horizontalBorderTeleportable = false;

            reset();
            this.removeListener<OnBrickColliderChanged>();
            base.onRelease();
        }

        public void setPlayer(APlayer p) => player = p;
        public APlayer getPlayer() => player;

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

                if (usePhysics)
                {
                    OnFixedUpdate(dt);
                }
                else
                {
                    fixedUpdate(dt);
                }
            }
        }

        public override void OnFixedUpdate(float dt)
        {
            using var _ = new SafeDictionaryReader<Brick, MTimer>(brickHitTimers, out var reader);
            foreach (var (brick, timer) in reader)
            {
                if (timer.update(dt))
                {
                    brickHitTimers.remove(brick);
                    timer.release();
                }
            }

            if (!_shouldMove)
                return;

            if (IsPenetrable)
            {
                if (ManuallyColliding)
                {
                    if (collidingBrick)
                    {
                        if (willPassingThroughThisFrame)
                        {
                            CollidingManually(willPassingThroughHit);
                            willPassingThroughThisFrame = false;
                            correctPos = Vector3.zero;
                            willPassingThroughHit = default;
                        }
                    }
                    else
                    {
                        if (willPassingThroughThisFrame && curPos == correctPos)
                        {
                            CollidingManually(willPassingThroughHit);
                            willPassingThroughThisFrame = false;
                            correctPos = Vector3.zero;
                            willPassingThroughHit = default;
                            return;
                        }
                    }
                }

                if (collidingBrick)
                {
                    willPassingThroughThisFrame = CheckWillPassingThrough(dt, BounceLayers, out correctPos, out willPassingThroughHit);
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

                if (collidingBrick)
                {
                    if (circleIntersectRectangle(getCircle(), collidingBrick.getRect()))
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
                                player.onBallEndOverlappingBrickOne(this, lastOverlappingBrick, true);
                            }

                            var noOverlappingBefore = overlappingBrick == null;
                            overlappingBrick = collidingBrick;
                            if (noOverlappingBefore)
                                player.onBallBeginOverlappingBrickAll(this, overlappingBrick);

                            player.onBallBeginOverlappingBrickOne(this, overlappingBrick);
                        }
                    }
                    else
                    {
                        if (isOverlappingBrick)
                        {
                            isOverlappingBrick = false;
                            player.onBallEndOverlappingBrickOne(this, overlappingBrick, false);
                            player.onBallEndOverlappingBrickAll(this, overlappingBrick, false);
                            overlappingBrick = null;
                        }

                        collidingBrick = null;
                    }
                }
            }
            else
            {
                if (ManuallyColliding)
                {
                    if (willPassingThroughThisFrame && curPos == correctPos)
                    {
                        willPassingThroughThisFrame = false;
                        correctPos = Vector3.zero;
                        CollidingManually(willPassingThroughHit);
                        willPassingThroughHit = default;
                        return;
                    }
                }

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

            if (FaceMovement)
                FaceMovementDirection(Direction);
        }

        void fixedUpdate(float dt)
        {
            if (isVectorEqual(hitNormal, Vector2.zero))
                return;

            checkRadius();

            prePos = curPos;
            movementDelta = moveSpeed * dt;
            curPos = Vector2.MoveTowards(curPos, targetPos, movementDelta);
            MovementTo(curPos);

            var mid = (prePos + curPos) / 2F;
            Debug.DrawLine(prePos, mid, Color.red, 0.02F);
            Debug.DrawLine(mid, curPos, Color.green, 0.02F);
            Debug.DrawLine(curPos, targetPos, Color.white, 0.02F);
            if (prePos == targetPos)
            {
                delayCounter--;
                if (delayCounter <= 0)
                {
                    var validHit = onHitEnter(hitCollider, hitNormal);
                    if (!validHit)
                    {
                        refreshHitInfo();
                    }
                }
            }
            else
            {
                if (collidingBrick)
                {
                    if (circleIntersectRectangle(getCircle(), collidingBrick.getRect()))
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
                                player.onBallEndOverlappingBrickOne(this, lastOverlappingBrick, true);
                            }

                            var noOverlappingBefore = overlappingBrick == null;
                            overlappingBrick = collidingBrick;
                            if (noOverlappingBefore)
                                player.onBallBeginOverlappingBrickAll(this, overlappingBrick);

                            player.onBallBeginOverlappingBrickOne(this, overlappingBrick);
                        }
                    }
                    else
                    {
                        if (isOverlappingBrick)
                        {
                            isOverlappingBrick = false;
                            player.onBallEndOverlappingBrickOne(this, overlappingBrick, false);
                            player.onBallEndOverlappingBrickAll(this, overlappingBrick, false);
                            overlappingBrick = null;
                        }

                        collidingBrick = null;
                    }
                }
            }
        }

        public void reflectBounce(Vector2 normal, bool fromBrick = false)
        {
            var reflectDir = Vector2.Reflect(Direction, normal);
            player.onBallReflect(this, normal, fromBrick, ref reflectDir);
            setDirection(reflectDir);
            counters.reflect.count();
        }

        public Vector2 getDirection()
        {
            return Direction;
        }

        public Vector2 getVelocity()
        {
            return Direction * moveSpeed;
        }

        public void setDirection(Vector2 dir, int exceptMask = 0)
        {
            lastDirection = Direction;
            Direction = dir.normalized;
            refreshHitInfo(exceptMask);
        }

        public void setShootDirection(Vector2 dir, int exceptMask = 0)
        {
            lastDirection = Direction;
            Direction = dir.normalized;
            refreshHitInfo(exceptMask);
        }

        public void setEnabled(bool b)
        {
            enabled = b;
        }

        protected void refreshHitInfo(int exceptMask = 0)
        {
            RaycastHit2D hit = default;

            var mask = ALL_BORDER_LAYER_MASK;
            mask |= BRICK_LAYER_MASK;
            mask |= OBSTACLE_LAYER_MASK;
            mask &= ~exceptMask;

            // Issue 1: 先检测球是否已嵌入碰撞体，若是则尝试向反方向推出
            /*var overlapFilter = new ContactFilter2D();
            overlapFilter.SetLayerMask(mask);
            using var overlapList = new ListScope<Collider2D>(out var overlapColliders);
            int overlapCount = Physics2D.OverlapCircle(curPos, radius, overlapFilter, overlapColliders);
            if (overlapCount > 0)
            {
                // 先尝试沿运动方向的反方向推出
                Vector2 antiPushDir = direction != Vector2.zero ? -direction : Vector2.left;
                float pushDist = radius * 2f;
                Vector2 pushedPos = curPos + antiPushDir * pushDist;
                overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
                if (overlapCount == 0)
                {
                    curPos = pushedPos;
                }
                else
                {
                    // 反方向不行，尝试正交方向
                    Vector2 orthoDir = new Vector2(-direction.y, direction.x);
                    pushedPos = curPos + orthoDir * pushDist;
                    overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
                    if (overlapCount == 0)
                    {
                        curPos = pushedPos;
                    }
                    else
                    {
                        pushedPos = curPos - orthoDir * pushDist;
                        overlapCount = Physics2D.OverlapCircle(pushedPos, radius, overlapFilter, overlapColliders);
                        if (overlapCount == 0)
                        {
                            curPos = pushedPos;
                        }
                        // 所有方向都无法推出，保持原位，交给ensureNotOverlapping处理
                    }
                }
            }*/

            if (IsPenetrable)
            {
                // Issue 4: 每次新建filter，避免状态残留
                var filter = new ContactFilter2D();
                filter.useTriggers = true;
                filter.SetLayerMask(BRICK_LAYER_MASK);

                using var a = new ListScope<Collider2D>(out var overlapColliders2);
                var overlapCount2 = Physics2D.OverlapCircle(curPos, Radius, filter, overlapColliders2);
                if (overlapCount2 > 0)
                {
                    filter.SetLayerMask(mask);
                    using var _ = new ListScope<RaycastHit2D>(out var hits);
                    var count = Physics2D.CircleCast(curPos, Radius, Direction, filter, hits, PHYSICS_CAST_DISTANCE);
                    if (count > 0)
                    {
                        hits.Sort(comparison);
                        for (var i = 0; i < count; i++)
                        {
                            hit = hits[i];
                            if (overlapColliders2.Contains(hit.collider))
                                continue;

                            var hitDir = hit.point - (Vector2)curPos;
                            if (Vector2.Dot(Direction, hitDir) < 0)
                                continue;

                            break;
                        }
                    }
                }
                else
                {
                    hit = Physics2D.CircleCast(curPos, Radius, Direction, PHYSICS_CAST_DISTANCE, mask);
                }
            }
            else
            {
                hit = Physics2D.CircleCast(curPos, Radius, Direction, PHYSICS_CAST_DISTANCE, mask);
            }

            if (hit)
            {
                targetPos = hit.point + hit.normal * Radius;
                hitNormal = hit.normal;
                hitCollider = hit.collider;
                delayCounter = 1;
            }
            else
            {
                // Issue 7: 无命中时设置远端目标，防止球停住
                hitCollider = null;
                hitNormal = Vector2.zero;
                targetPos = curPos + Direction * PHYSICS_CAST_DISTANCE;
                delayCounter = 1;
            }
        }

        int Comparison(RaycastHit2D h1, RaycastHit2D h2)
        {
            var d1 = Vector2.Distance(curPos, h1.point);
            var d2 = Vector2.Distance(curPos, h2.point);
            return d1.CompareTo(d2);
        }

        public void setTeleportPosition(Vector2 pos, int exceptMask = 0)
        {
            prePos = curPos = pos;
            _rigidBody2D.MovePosition(pos);
            setPosition(pos);
            setDirection(Direction, exceptMask);
            ballRenderer.clearTrail();
        }

        public void setRendererActive(bool active) => ballRenderer.setRendererActive(active);

        public void setRadius(float value)
        {
            lastRadius = Radius;
            Radius = value;
            var diameter = value * 2F;
            ballRenderer.setRadius(diameter);
        }

        public Circle2 getCircle()
        {
            return new(curPos, Radius);
        }

        void checkRadius()
        {
            if (isFloatEqual(lastRadius, Radius))
                return;
            setRadius(Radius);
        }

        public float getPhysicDamage()
        {
            float physicDamage = 0;
            if (GetStat(Stat.AD, out var ballAD))
            {
                physicDamage += ballAD.Value;
            }

            if (character.GetStat(Character.Stat.AD, out var characterAD))
            {
                physicDamage += characterAD.Value;
            }

            return physicDamage;
        }

        public float getMagicDamage()
        {
            GetStat(Stat.AP, out var stat);
            return stat.Value;
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

        public virtual Dmg getHitDmg(Brick brick, Vector2 normal)
        {
            var d = getPhysicDamage();
            var dmg = Dmg.AD(d);
            dmg.setAttackEffect();
            GetStat(Stat.DmgRate, out var dmgRate);
            dmg.SetDmgRate(dmgRate.Value);
            dmg.setHitNormal(normal);
            GetStat(Stat.CritChance, out var critChance);
            if (randomHit(critChance.Value))
                dmg.Crit();
            return dmg;
        }

        public virtual Dmg getHitDmg(Border border, Vector2 normal)
        {
            return Dmg.True(0);
        }

        public virtual Dmg getHitDmg(Obstacle obstacle, Vector2 normal)
        {
            return Dmg.True(0);
        }

        public virtual Dmg getSkillDmg(Brick brick)
        {
            var d = getMagicDamage();
            var dmg = Dmg.AP(d);
            dmg.setSkillEffect();

            GetStat(Stat.DmgRate, out var stat);
            dmg.SetDmgRate(stat.Value);
            return dmg;
        }

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