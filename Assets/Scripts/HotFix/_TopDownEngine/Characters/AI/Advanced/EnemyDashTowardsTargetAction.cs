using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// An action that makes the character dash towards the target. Useful for charge attacks or quick approach.
    /// </summary>
    public class EnemyDashTowardsTargetAction : EnemyAction
    {
        /// the possible spaces the dash should happen in, either in world coordinates or local ones
        public enum DashSpaces
        {
            World,
            Local
        }

        [Header("Dash")]
        public DashSpaces DashSpace = DashSpaces.World;

        public LayerMask ObstaclesLayerMask = LayerManager.Obstacles_Mask;

        [Tooltip("the distance the dash should last for")]
        public float DashDistance = 6f;

        [Tooltip("the duration of the dash, in seconds")]
        public int DashDurationCounter = 10;

        [Tooltip("the delay of the dash in seconds")]
        public float DashDelay = 1f;

        [Tooltip("the cooldown between dashes in seconds")]
        public float DashCooldown = 2f;

        [Tooltip("the minimum distance to trigger a dash")]
        public float MinDistanceToDash = 5f;

        [Tooltip("the maximum distance to dash")]
        public float MaxDashDistance = 10f;

        [Tooltip("the movement constraint type (free, 4-dir, 8-dir, etc.)")]
        public Movements MovementConstraint = Movements.Free;

        public MMFeedbacks DashFeedback;

        [Tooltip("the animation curve to apply to the dash acceleration")]
        public AnimationCurve DashCurve = new(new(0f, 0f), new(1f, 1f));

        protected Vector3 _dashOrigin;
        protected Vector3 _dashDestination;
        protected Vector2 _dashDirection;
        protected Vector3 _newPosition;
        protected Vector3 _oldPosition;
        protected SpriteAfterImageEmitter _spriteEmitter;

        protected Timer _delayTimer;
        protected Countdown _dashTimer;
        protected Timer _cooldownTimer;
        protected bool _isDelaying;
        protected bool _isDashing;
        protected bool _isCooldown;

        public override void Initialization()
        {
            base.Initialization();
            _delayTimer = 0f;
            _dashTimer = 0;
            _cooldownTimer = 0f;
            _isDelaying = false;
            _isDashing = false;
            _isCooldown = false;

            this.TryGetComponentInChildren(out _spriteEmitter);
        }

        public override void PerformAction(float dt)
        {
            if (_brick.IsDead())
            {
                _movement.SetMovement(Vector2.zero);
                return;
            }

            if (_isDashing)
            {
                PerformDash();
            }
            else
            {
                CheckForDash();
            }

            UpdateTimers(dt);
        }

        protected virtual void CheckForDash()
        {
            if (_isDelaying || _isCooldown || _brain.NoTarget)
                return;

            float distance = Vector3.Distance(transform.position, _brain.Target.position);
            if (distance >= MinDistanceToDash && distance <= MaxDashDistance)
            {
                StartDashDelay();
            }
        }

        GameEffect indicatorEffect;
        LineRegion lineRegion;

        protected virtual void StartDashDelay()
        {
            if (_brain.NoTarget)
                return;

            _isDelaying = true;
            _delayTimer = DashDelay;
            _dashOrigin = transform.position;
            _dashDirection = (_brain.Target.position - _dashOrigin).normalized;
            _dashDirection = ConstrainDirection(_dashDirection);
            HandleDashMode();

            var path = $"{GAMEPLAY_PATH}/Indicators/FxIndicator_Line.prefab";
            indicatorEffect = mEffectManager.createEffect(path, _dashOrigin);
            if (indicatorEffect.tryGetUnityComponent(out lineRegion))
            {
                lineRegion.Angle = -_dashDirection.getAngle(ANGLE.DEGREE);
                lineRegion.Length = (_dashDestination - _dashOrigin).magnitude;
                lineRegion.Width = _brick.getMaxWidthOrHeight();
                lineRegion.FillProgress = 0F;
            }

            _brain.IsDecisionLocked = true;
        }

        protected virtual void DashStart()
        {
            if (_brain.NoTarget)
                return;

            _isDashing = true;
            _dashTimer = DashDurationCounter;
            _brick.motionState.ChangeState(Character.Motions.Dashing);
            _dashOrigin = transform.position;
            DashFeedback.Play(transform.position);

            HandleDashMode();
            _brick.Event.trigger(new DoDash());

            if (_spriteEmitter)
                _spriteEmitter.Begin();
        }

        protected virtual void HandleDashMode()
        {
            var dashDistance = DashDistance;
            var selfPos = transform.position;
            var dashDestination = selfPos + (Vector3)_dashDirection.normalized * dashDistance;

            var dir = dashDestination - selfPos;
            var radius = _brick.Controller2D.Volume.BoundingRadius;
            var hit = Physics2D.CircleCast(selfPos, radius, dir, dashDistance, ObstaclesLayerMask);
            if (hit)
            {
                dashDestination = hit.point + hit.normal * radius;
            }

            _dashDestination = dashDestination;
        }

        protected virtual void PerformDash()
        {
            if (!_isDashing)
                return;

            // _movement.SetMovement(_dashDirection, DashSpeedMultiplier);

            if (_dashTimer)
            {
                var f = DashCurve.Evaluate(_dashTimer.pct);
                _dashTimer.update();
                switch (DashSpace)
                {
                    case DashSpaces.World:
                        _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, f);
                        _controller2D.MovePositionTo(_newPosition);
                        break;
                    case DashSpaces.Local:
                        _oldPosition = _dashTimer.elapsed == 0 ? _dashOrigin : _newPosition;
                        _newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, f);
                        _controller2D.MovePositionTo(transform.position + _newPosition - _oldPosition);
                        break;
                }

                var emitter = _spriteEmitter;
                if (emitter)
                {
                    emitter.FixedUpdateCounter++;
                    if (emitter.FixedUpdateCounter >= emitter.spawnFixedUpdateInterval)
                    {
                        emitter.FixedUpdateCounter = 0;
                        emitter.Spawn(_newPosition);
                    }
                }
            }
            else
            {
                _isDashing = false;
                _isCooldown = true;
                _cooldownTimer = DashCooldown;
                DashStop();
            }
        }

        public virtual void DashStop()
        {
            DashFeedback.Stop(transform.position);

            _brick.motionState.ChangeState(Character.Motions.Idle);
            _isDashing = false;

            if (_spriteEmitter)
                _spriteEmitter.End();
            
            _brain.IsDecisionLocked = false;
        }

        protected virtual void UpdateTimers(float dt)
        {
            if (_isDelaying)
            {
                lineRegion.transform.position = _brick.getCenterPosition();
                lineRegion.FillProgress = _delayTimer.pct;
                if (_delayTimer.update(dt))
                {
                    lineRegion.FillProgress = 1F;
                    _isDelaying = false;
                    DashStart();

                    mEffectManager.destroyEffect(ref indicatorEffect);
                    lineRegion = null;
                }
            }
            else if (_isCooldown)
            {
                if (_cooldownTimer.update(dt))
                {
                    _isCooldown = false;
                }
            }
        }

        /// <summary>
        /// 根据 Movements 枚举限制方向向量
        /// </summary>
        protected virtual Vector2 ConstrainDirection(Vector2 dir)
        {
            if (dir == Vector2.zero)
                return dir;

            switch (MovementConstraint)
            {
                case Movements.Free:
                    return dir;

                case Movements.Strict2DirectionsHorizontal:
                    return new(Mathf.Sign(dir.x), 0f);

                case Movements.Strict2DirectionsVertical:
                    return new(0f, Mathf.Sign(dir.y));

                case Movements.Strict4Directions:
                {
                    // 取 x/y 中绝对值更大的分量作为主方向
                    if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
                        return new(Mathf.Sign(dir.x), 0f);
                    return new(0f, Mathf.Sign(dir.y));
                }

                case Movements.Strict8Directions:
                {
                    // 将方向量化为 8 个方向之一
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    // 归一化到 [-180, 180]
                    angle = Mathf.Repeat(angle + 180f, 360f) - 180f;
                    // 每 45° 一个区间，量化到最近的 8 方向
                    int sector = Mathf.RoundToInt(angle / 45f);
                    float quantizedAngle = sector * 45f * Mathf.Deg2Rad;
                    return new(Mathf.Cos(quantizedAngle), Mathf.Sin(quantizedAngle));
                }

                default:
                    return dir;
            }
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _isDashing = false;
            _isCooldown = false;
            _movement.SetMovement(Vector2.zero);
        }
    }
}