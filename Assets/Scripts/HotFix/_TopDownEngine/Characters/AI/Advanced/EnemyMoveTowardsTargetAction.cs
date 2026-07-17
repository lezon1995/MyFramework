using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Requires a CharacterMovement ability. Makes the character move up to the specified MinimumDistance in the direction of the target. 
    /// </summary>
    public class EnemyMoveTowardsTargetAction : EnemyAction
    {
        [Tooltip("if this is true, movement will be constrained to not overstep a certain distance to the target on the x axis")]
        public bool UseMinimumXDistance = true;

        [Tooltip("the minimum distance from the target this Character can reach on the x axis.")]
        public float MinimumXDistance = 1f;
        
        [Header("怪物AI（意图部分）")]
        [Tooltip("怪物聚集力（向玩家移动的强度，AI 意图）")]
        public float MonsterAttractionForce = 1f;
        
        [Tooltip("怪物随机扰动力（徘徊，AI 意图）")]
        public float MonsterWanderForce = 0.5f;

        protected Vector2 _direction;

        public override void Initialization()
        {
            if (!ShouldInitialize)
                return;

            base.Initialization();
        }

        public override void PerformAction()
        {
            Move();
        }

        /// <summary>
        /// Moves the character towards the target if needed
        /// </summary>
        protected virtual void Move()
        {
            if (_brain.Target == null)
                return;

            var targetPos = _brain.Target.position;
            var selfPos = transform.position;
            if (UseMinimumXDistance)
            {
                var h = selfPos.x < targetPos.x ? 1f : -1f;
                var v = selfPos.y < targetPos.y ? 1f : -1f;

                if (Mathf.Abs(selfPos.x - targetPos.x) < MinimumXDistance)
                    h = 0F;

                if (Mathf.Abs(selfPos.y - targetPos.y) < MinimumXDistance)
                    v = 0F;
                _direction = new Vector2(h, v).normalized;
            }
            else
            {
                _direction = (targetPos - selfPos).normalized;
            }

            _direction *= MonsterAttractionForce;
            
            // 2. 随机徘徊（让怪物移动看起来更自然）
            _direction += Random.insideUnitCircle * MonsterWanderForce;

            // 将意图转换为期望速度
            Vector2 desiredVelocity = _direction.normalized * _movement._movementSpeed;
            
            // 平滑过渡到期望速度（保留原有速度，让 VolumeManager 处理碰撞反应）
            var velocity = Vector2.Lerp(_controller.Velocity, desiredVelocity, Time.deltaTime * 5f);
            _controller.Velocity = velocity;
            _movement.SetMovement(_direction);
        }

        public override void OnExitState()
        {
            base.OnExitState();
            _movement.SetMovement(Vector2.zero);
        }
    }
}