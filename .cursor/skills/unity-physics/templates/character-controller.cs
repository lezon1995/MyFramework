using UnityEngine;

namespace MyGame.PhysicsModule
{
    /// <summary>
    /// CharacterController 角色控制器：含重力、贴地检测、斜面与台阶处理。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterController : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintMultiplier = 1.6f;
        [SerializeField] private float rotationSpeed = 12f;

        [Header("重力")]
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundedStickForce = -2f;

        [Header("地面检测")]
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private float groundCheckDistance = 0.15f;

        private CharacterController _controller;
        private float _verticalVelocity;
        private Vector3 _currentMove;
        private bool _isGrounded;

        public bool IsGrounded => _isGrounded;
        public Vector3 CurrentMove => _currentMove;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector3 input = new Vector3(horizontal, 0f, vertical);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            bool sprint = Input.GetKey(KeyCode.LeftShift);
            float speed = moveSpeed * (sprint ? sprintMultiplier : 1f);

            // 目标速度（世界空间，基于相机朝向由外部注入或使用 transform.forward）
            Vector3 desired = (transform.right * input.x + transform.forward * input.z) * speed;

            // 地面检测
            _isGrounded = _controller.isGrounded;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                    _controller.height * 0.5f + groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                _isGrounded = true;
                if (hit.normal.y > 0.55f)
                {
                    _isGrounded = true;
                }
            }

            if (_isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = groundedStickForce;
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            Vector3 move = desired * Time.deltaTime;
            move.y = _verticalVelocity * Time.deltaTime;

            // Move 返回实际位移，撞墙时会小于输入值
            Vector3 actual = _controller.Move(move);
            _currentMove = actual / Time.deltaTime;

            // 朝向
            if (desired.sqrMagnitude > 0.01f)
            {
                Quaternion target = Quaternion.LookRotation(desired.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 是否处于斜坡上（用于禁止跳跃等）。
        /// </summary>
        public bool IsOnSteepSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit,
                    _controller.height * 0.5f + groundCheckDistance, groundLayers, QueryTriggerInteraction.Ignore))
            {
                return hit.normal.y < 0.55f;
            }

            return false;
        }
    }
}
