using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// Requires a CharacterMovement ability. 
    /// Makes the character move randomly, until it finds an obstacle in its path, in which case it'll pick a new direction at random
    /// </summary>
    //[RequireComponent(typeof(CharacterMovement))]
    public class EnemyMoveRandomlyAction : EnemyAction
    {
        [Header("Duration")]
        /// the maximum time a character can spend going in a direction without changing
        [Tooltip("the maximum time a character can spend going in a direction without changing")]
        public float MaximumDurationInADirection = 2f;

        [Header("Obstacles")]
        /// the layers the character will try to avoid
        [Tooltip("the layers the character will try to avoid")]
        public LayerMask ObstacleLayerMask = LayerManager.Obstacles_Mask;

        /// the minimum distance from the target this Character can reach.
        [Tooltip("the minimum distance from the target this Character can reach.")]
        public float ObstaclesDetectionDistance = 1f;

        /// the frequency (in seconds) at which to check for obstacles
        [Tooltip("the frequency (in seconds) at which to check for obstacles")]
        public float ObstaclesCheckFrequency;

        /// the minimal random direction to randomize from
        [Tooltip("the minimal random direction to randomize from")]
        public Vector2 MinimumRandomDirection = new(-1f, -1f);

        /// the maximum random direction to randomize from
        [Tooltip("the maximum random direction to randomize from")]
        public Vector2 MaximumRandomDirection = new(1f, 1f);

        [Header("怪物AI（意图部分）")]
        [Tooltip("怪物聚集力（向玩家移动的强度，AI 意图）")]
        public float MonsterAttractionForce = 1f;
        
        [Tooltip("怪物随机扰动力（徘徊，AI 意图）")]
        public float MonsterWanderForce = 0.5f;
        
        protected Vector2 _direction;
        protected Collider2D _collider;
        protected float _lastObstacleDetectionTimestamp;
        protected float _lastDirectionChangeTimestamp;

        /// <summary>
        /// On start, we grab our character movement component and pick a random direction
        /// </summary>
        public override void Initialization()
        {
            if (!ShouldInitialize)
                return;

            base.Initialization();
            this.TryGetComponentInParent(out _collider);
            PickRandomDirection();
        }

        /// <summary>
        /// On PerformAction we move
        /// </summary>
        /// <param name="dt"></param>
        public override void PerformAction(float dt)
        {
            CheckForObstacles();
            CheckForDuration();
            Move(dt);
        }

        /// <summary>
        /// Moves the character
        /// </summary>
        /// <param name="dt"></param>
        protected virtual void Move(float dt)
        {
            _direction *= MonsterAttractionForce;
            
            // 2. 随机徘徊（让怪物移动看起来更自然）
            _direction += Random.insideUnitCircle * MonsterWanderForce;

            // 将意图转换为期望速度
            Vector2 desiredVelocity = _direction.normalized * _movement._movementSpeed;
            
            // 平滑过渡到期望速度（保留原有速度，让 VolumeManager 处理碰撞反应）
            var velocity = Vector2.Lerp(_controller.IntentVelocity, desiredVelocity, dt * 5f);
            _controller.IntentVelocity = velocity;
            _movement.SetMovement(_direction);
        }

        /// <summary>
        /// Checks for obstacles by casting a ray
        /// </summary>
        protected virtual void CheckForObstacles()
        {
            if (Time.time - _lastObstacleDetectionTimestamp < ObstaclesCheckFrequency)
                return;

            RaycastHit2D hit = Physics2D.BoxCast(_collider.bounds.center, _collider.bounds.size, 0f, _direction.normalized, _direction.magnitude, ObstacleLayerMask);
            if (hit)
            {
                PickRandomDirection();
            }

            _lastObstacleDetectionTimestamp = Time.time;
        }

        /// <summary>
        /// Checks whether we should pick a new direction at random
        /// </summary>
        protected virtual void CheckForDuration()
        {
            if (Time.time - _lastDirectionChangeTimestamp > MaximumDurationInADirection)
            {
                PickRandomDirection();
            }
        }

        /// <summary>
        /// Picks a random direction
        /// </summary>
        protected virtual void PickRandomDirection()
        {
            _direction.x = Random.Range(MinimumRandomDirection.x, MaximumRandomDirection.x);
            _direction.y = Random.Range(MinimumRandomDirection.y, MaximumRandomDirection.y);
            _lastDirectionChangeTimestamp = Time.time;
        }

        /// <summary>
        /// On exit state we stop our movement
        /// </summary>
        public override void OnExitState()
        {
            base.OnExitState();

            if (_movement)
            {
                _movement.SetMovement(Vector2.zero);
            }
        }
    }
}