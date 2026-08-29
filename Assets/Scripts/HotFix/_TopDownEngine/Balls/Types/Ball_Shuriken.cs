using System;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains
{
    public class Ball_Shuriken : Ball
    {
        public override BallType BallType => BallType.Shuriken;

        Comparison<Collider2D> distanceSorter;
        protected Transform lastTarget;

        public Ball_Shuriken()
        {
            distanceSorter = DistanceSorter;
        }

        Countdown countdown;

        public override void onAcquire()
        {
            base.onAcquire();
            countdown = 3;
            lastTarget = null;
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            if (_damageOnTouch)
            {
                _damageOnTouch.TriggerFilter = MoreMountains.DamageOnTouch.TriggerMask.IgnoreAll;
            }
        }

        public override void refreshDuration()
        {
            // player.GetStat(Character.Stat.Duration, out var playerDuration);
            // GetStat(Stat.Duration, out var ballDuration);
            // var duration = ballDuration.Value * (1 + playerDuration.Value);
            lifeDuration = 30;
            isExpired = false;
        }

        public override void OnFixedUpdate(float dt)
        {
            if (_shouldMove)
            {
                Movement(dt);

                if (FaceMovement)
                {
                    FaceMovementDirection(Direction);
                }
            }
            
            CheckExpiration(dt);
        }

        protected override bool CheckWillPassingThrough(float dt, LayerMask targetLayer, out Vector3 hitPos, out RaycastHit2D hitInfo)
        {
            hitPos = default;
            hitInfo = default;
            return false;
        }

        public override void Movement(float dt)
        {
            if (_target == null)
                return;

            prePos = curPos;
            movementDelta = moveSpeed * dt;
            targetPos = _target.position;
            curPos = Vector3.MoveTowards(curPos, targetPos, movementDelta);

            var dir = targetPos - curPos;
            SetDirection(dir.normalized, Quaternion.identity);
            if (dir.sqrMagnitude == 0F)
            {
                var direction = curPos - prePos;
                lastTarget = _target;
                CollidingManually(_target.gameObject, direction, curPos);
            }
        }

        protected override void EvaluateHit2D(GameObject hitObject, Vector2 hitNormal, Vector2 hitPoint)
        {
            if (countdown.update())
            {
                SetTarget(null);
                _health.Kill();
            }
            else
            {
                using var _ = new ListScope<Collider2D>(out var colliders);
                var filter = new ContactFilter2D();
                filter.useTriggers = true;
                filter.SetLayerMask(BRICK_LAYER_MASK);
                int count = Physics2D.OverlapCircle(curPos, 3, filter, colliders);
                if (count > 0)
                {
                    colliders.Sort(distanceSorter);
                    bool findNewTarget = false;
                    foreach (var t in colliders)
                    {
                        if (lastTarget == t.transform)
                            continue;

                        SetTarget(t.transform);
                        findNewTarget = true;
                    }

                    if (!findNewTarget)
                    {
                        SetTarget(null);
                        _health.Kill();
                    }
                }
                else
                {
                    SetTarget(null);
                    _health.Kill();
                }
            }
        }

        protected override void Bounce2D(GameObject hitObject, Vector2 hitNormal)
        {
            BounceFeedback.Play();
            var reflectDir = Vector2.Reflect(Direction, hitNormal).normalized;
            bool fromBrick = hitObject.layer == LayerManager.Brick;
            player.onBallReflect(this, hitNormal, fromBrick, ref reflectDir);
            float angle = Vector2.Angle(Direction, reflectDir);
            SetDirection(reflectDir, Quaternion.identity);
            _bouncesLeft--;
        }

        int DistanceSorter(Collider2D c1, Collider2D c2)
        {
            var d1 = c1.transform.position - curPos;
            var d2 = c2.transform.position - curPos;
            return d1.sqrMagnitude.CompareTo(d2.sqrMagnitude);
        }
    }
}