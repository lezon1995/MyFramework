using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public partial class Ball : BouncyProjectile
        , IStatsGetter<Ball.Stat>
    {
        public enum Stat
        {
            HitDamage,
            EffectDamage,
            HitDamageRate,
            EffectDamageRate,
            AS,
            BallisticSpeed,
            CritChance,
            CritDamage,
            DmgRate,
            Duration,
            HealthMax,
            HitEffectChance,
            Knockback,
            LifeSteal,
            Range,
        }

        public APlayer Player { get; set; }
        public BallInventorySlot Slot { get; set; }

        public CircleCollider2D circleCollider
        {
            get
            {
                if (_circleCollider == null)
                    TryGetComponent(out _circleCollider);
                return _circleCollider;
            }
        }

        public CircleCollider2D _circleCollider;
        float _radius;

        public float Radius
        {
            get
            {
                if (_radius.isZero())
                    _radius = circleCollider.radius;

                return _radius;
            }
            set
            {
                _radius = value;
                circleCollider.radius = _radius;
            }
        }

        public bool IsRecollecting { get; set; }
        public new BallDamageOnTouch DamageOnTouch => (BallDamageOnTouch)_damageOnTouch;
        public new BallStats Stats => _stats as BallStats;

        protected override void OnAwake()
        {
            base.OnAwake();
            _circleCollider = _collider2D as CircleCollider2D;
            TryGetComponent(out ballRenderer);
            instanceID = GetInstanceID();
            curPos = getWorldPosition();

            _smallestBoundsWidth = Radius * 2F;
            _squaredBoundsWidth = _smallestBoundsWidth * _smallestBoundsWidth;

            _stats.LevelGetter = () => level;

            addListeners();
        }

        protected override void OnDestroy()
        {
            removeListeners();
            base.OnDestroy();
        }

        public override void reset()
        {
            base.reset();
            collidingBrick = null;
            overlappingBrick = null;
            hasCorrectPosThisFixedUpdate = false;
            movementDelta = 0;
            lastRadius = 0;
            enabled = false;
            hasBeenCollided = false;
            delayCounter = 0;
            lastHittable = null;
            isOverlappingBrick = false;
            IsRecollecting = false;
            ResetIgnoredToHitBricks();
        }

        public override void SetOwner(Character newOwner)
        {
            base.SetOwner(newOwner);
            if (newOwner.TryGetComponent(out APlayer p))
            {
                Player = p;
            }
        }

        public void SetColliderEnabled(bool v)
        {
            _collider2D.enabled = v;
        }

        public void SetPlayer(APlayer c)
        {
            Player = c;
        }

        public void SetBallSlot(BallInventorySlot s)
        {
            Slot = s;
        }

        protected override void Initialization()
        {
            base.Initialization();
            _amountOfBounces = int.MaxValue;
            _bouncesLeft = _amountOfBounces;
        }

        protected override void OnStatsSet()
        {
            SpeedModifier = (ref float raw) =>
            {
                float speed = raw;
                var ballMS = GetStat(Stat.BallisticSpeed);
                if (ballMS)
                    speed = ballMS.Value;

                var globalMS = Player.GetStat(Character.Stat.BallisticSpeed);
                if (globalMS)
                    speed *= (1 + globalMS.Value);

                raw = speed;
            };
        }

        public UniStats.Stat GetStat(Stat key)
        {
            return _hasStats ? Stats.GetStat(key.Key()) : null;
        }

        public bool GetStat(Stat key, out UniStats.Stat stat)
        {
            if (!_hasStats)
            {
                stat = null;
                return false;
            }

            return Stats.GetStat(key.Key(), out stat);
        }

        public override void Movement(float dt)
        {
            hasCorrectPosThisFixedUpdate = false;
            prePos = curPos;
            _movement = Direction * (moveSpeed * dt);

            if (_hasRigidBody2D)
            {
                var nextPos = curPos + _movement;
                curPos = nextPos;
                _rigidBody2D.MovePosition(nextPos);
            }

            Speed += Acceleration * dt;
        }

        public override void MovementTo(Vector3 pos)
        {
            hasCorrectPosThisFixedUpdate = false;
            prePos = curPos;
            _movement = pos - curPos;

            if (_hasRigidBody2D)
            {
                curPos = pos;
                _rigidBody2D.MovePosition(pos);
            }
        }

        public bool RepositionRigidbodyIfHitTrigger = true;
        public bool RepositionRigidbodyIfHitNonTrigger = true;

        protected float _smallestBoundsWidth;
        protected float _squaredBoundsWidth;

        protected override bool CheckWillPassingThrough(float dt, LayerMask targetLayer, out Vector3 hitPos, out RaycastHit2D hitInfo)
        {
            RaycastHit2D hit = default;
            hitPos = Vector3.zero;
            hitInfo = default;
            var range = moveSpeed * dt;
            var movement = Direction * range;
            var nextPos = curPos + movement;

            bool willPassingThrough = false;
            var lastMovement = nextPos - curPos;
            float distance = lastMovement.magnitude;

            // if we've moved further than our bounds, we may have missed something
            var potentialCollider = Physics2D.OverlapCircle(curPos, range + Radius, targetLayer);
            if (potentialCollider)
            {
                if (!IsPenetrable)
                {
                    hit = Physics2D.CircleCast(curPos, Radius, Direction, distance, targetLayer);
                    if (hit)
                    {
                        if (Vector2.Dot(hit.normal, Direction) < 0)
                        {
                            willPassingThrough = true;
                            hitPos = hit.point + hit.normal * Radius;
                            hitInfo = hit;
                        }
                    }
                }
                else
                {
                    var filter = new ContactFilter2D();
                    filter.useTriggers = true;
                    filter.SetLayerMask(BRICK_LAYER_MASK);

                    using var a = new ListScope<Collider2D>(out var overlapBricks);
                    var overlapCount = Physics2D.OverlapCircle(curPos, Radius, filter, overlapBricks);
                    if (overlapCount > 0)
                    {
                        filter.SetLayerMask(targetLayer);
                        using var _ = new ListScope<RaycastHit2D>(out var hits);
                        var count = Physics2D.CircleCast(curPos, Radius, Direction, filter, hits, distance);
                        if (count > 0)
                        {
                            hits.Sort(comparison);
                            for (var i = 0; i < count; i++)
                            {
                                hit = hits[i];
                                if (overlapBricks.Contains(hit.collider))
                                    continue;

                                var hitDir = hit.point - (Vector2)curPos;
                                if (Vector2.Dot(Direction, hitDir) < 0)
                                    continue;

                                if (hit.collider.TryGetComponent(out Brick brick))
                                {
                                    if (IsTheBrickBeingIgnoredToHit(brick))
                                        continue;
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        hit = Physics2D.CircleCast(curPos, Radius, Direction, distance, targetLayer);
                    }

                    if (hit)
                    {
                        if (Vector2.Dot(hit.normal, Direction) < 0)
                        {
                            willPassingThrough = true;
                            hitPos = hit.point + hit.normal * Radius;
                            hitInfo = hit;
                        }
                    }
                }
            }

            return willPassingThrough;
        }

        protected override void Colliding(Collider2D c)
        {
            if (!BounceLayers.MMContains(c.gameObject.layer))
                return;

            var raycastDir = (curPos - prePos).normalized;

            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BounceLayers);
            using var _ = new ListScope<RaycastHit2D>(out var hits);
            var count = Physics2D.CircleCast(prePos, Radius, raycastDir, filter, hits, raycastDir.magnitude);
            RaycastHit2D hit = default;
            if (count > 0)
            {
                for (var i = 0; i < hits.Count; i++)
                {
                    var hit2D = hits[i];
                    if (hit2D.collider == c)
                        hit = hit2D;
                }
            }

            if (hit)
            {
                EvaluateHit2D(hit.collider.gameObject, hit.normal, hit.point);
            }
        }

        SafeDictionary<Brick, MTimer> brickHitTimers = new();

        public bool IsTheBrickBeingIgnoredToHit(Brick brick)
        {
            return brickHitTimers.containsKey(brick);
        }

        public void ResetIgnoredToHitBricks()
        {
            foreach (var (brick, timer) in brickHitTimers)
                timer.release();

            brickHitTimers.clear();
        }

        protected override void CollidingManually(GameObject hitObject, Vector2 hitNormal, Vector2 hitPoint)
        {
            var ball = this;
            bool needReflect = true;
            var layer = hitObject.layer;
            if (IsPenetrable && PenetrableLayers.MMContains(layer))
                needReflect = false;

            lastHitNormal = hitNormal;
            var normal = hitNormal;
            switch (layer)
            {
                case LayerManager.Brick:
                    if (hitObject.TryGetComponent(out Brick brick))
                    {
                        if (IsTheBrickBeingIgnoredToHit(brick))
                        {
                            return;
                        }

                        lastHittable = brick;
                        var hitDmg = getHitDmg(brick, normal);
                        brick.onHitEnter(ball, normal);
                        ball.onHitEnter(brick, normal, out var triggerRegularHit);
                        collidingBrick = brick;

                        if (triggerRegularHit)
                        {
                            counters.hit.count();
                            counters.hitBrick.count();
                        }

                        DamageOnTouch.Colliding(brick, hitDmg);

                        ResetIgnoredToHitBricks();
                        brickHitTimers.add(brick, 0.2F);
                    }

                    break;
                case LayerManager.Obstacles:
                    if (hitObject.TryGetComponent(out Obstacle obstacle))
                    {
                        lastHittable = obstacle;
                        foreach (var p in powers)
                            p.onHitObstacle(obstacle);

                        counters.hit.count();
                        hasBeenCollided = true;

                        _player.onBallHitObstacle(ball, obstacle, ref normal);
                        playHitObstacleSfx();

                        var hitDmg = getHitDmg(obstacle, normal);
                        DamageOnTouch.Colliding(obstacle, hitDmg);
                    }

                    break;
            }

            if (needReflect)
            {
                EvaluateHit2D(hitObject, hitNormal, hitPoint);
            }
        }

        /// <summary>
        /// Decides whether we should bounce
        /// </summary>
        protected override void EvaluateHit2D(GameObject hitObject, Vector2 hitNormal, Vector2 hitPoint)
        {
            if (hitObject == null)
                return;

            var pos = hitPoint + hitNormal * Radius;
            hasCorrectPosThisFixedUpdate = true;
            curPos = pos;
            _rigidBody2D.position = pos;
            transform.position = pos;

            if (_bouncesLeft > 0)
            {
                Bounce2D(hitObject, hitNormal);
            }
            else
            {
                _health.Kill();
                _damageOnTouch.HitNonDamageableFeedback.Play();
            }
        }

        public override void PreventedCollision2D(RaycastHit2D hit)
        {
            if (_health.CurrentHealth <= 0)
                return;

            var raycastDir = (curPos - prePos).normalized;

            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BounceLayers);
            using var _ = new ListScope<RaycastHit2D>(out var hits);
            var count = Physics2D.CircleCast(prePos, Radius, raycastDir, filter, hits, raycastDir.magnitude);
            if (count > 0)
            {
                for (var i = 0; i < hits.Count; i++)
                {
                    var hit2D = hits[i];
                    if (hit2D.collider == hit.collider)
                        hit = hit2D;
                }
            }

            if (hit)
            {
                EvaluateHit2D(hit.collider.gameObject, hit.normal, hit.point);
            }
        }

        protected override void Bounce2D(GameObject hitObject, Vector2 hitNormal)
        {
            BounceFeedback.Play();
            var reflectDir = Vector2.Reflect(Direction, hitNormal).normalized;
            Debug.DrawLine(curPos, curPos + (Vector3)reflectDir, Color.red, 1F);
            bool fromBrick = hitObject.layer == LayerManager.Brick;
            _player.onBallReflect(this, hitNormal, fromBrick, ref reflectDir);
            float angle = Vector2.Angle(Direction, reflectDir);
            SetDirection(reflectDir, Quaternion.identity);
            _bouncesLeft--;
        }
    }
}