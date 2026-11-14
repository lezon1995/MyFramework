using UnityEngine;

namespace MyGame.PhysicsModule
{
    /// <summary>
    /// 标准刚体组件：统一管理碰撞检测模式、插值、层与地面检测。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DisallowMultipleComponent]
    public class RigidbodySetup : MonoBehaviour
    {
        [Header("运动")]
        [SerializeField] private float mass = 1f;
        [SerializeField] private bool useGravity = true;
        [SerializeField] private RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;

        [Header("碰撞检测")]
        [Tooltip("高速移动的物体使用 Continuous Dynamic")]
        [SerializeField] private CollisionDetectionMode collisionDetection = CollisionDetectionMode.Discrete;

        [Header("层")]
        [SerializeField] private LayerMask groundLayers;

        private Rigidbody _rb;

        public Rigidbody Rigidbody => _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            _rb.mass = mass;
            _rb.useGravity = useGravity;
            _rb.interpolation = interpolation;
            _rb.collisionDetectionMode = collisionDetection;
        }

        /// <summary>
        /// 是否站在地面上。
        /// </summary>
        public bool IsGrounded()
        {
            return Physics.Raycast(
                transform.position,
                Vector3.down,
                out _,
                _rb.radius + 0.05f,
                groundLayers,
                QueryTriggerInteraction.Ignore);
        }

        /// <summary>
        /// 施加冲量。
        /// </summary>
        public void ApplyImpulse(Vector3 impulse, bool useMass = true)
        {
            _rb.AddForce(impulse, useMass ? ForceMode.Impulse : ForceMode.VelocityChange);
        }

        /// <summary>
        /// 朝某方向持续施力。
        /// </summary>
        public void ApplyForce(Vector3 force, bool useMass = true)
        {
            _rb.AddForce(force, useMass ? ForceMode.Force : ForceMode.Acceleration);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, GetComponent<Collider>() is SphereCollider sc ? sc.radius : 0.5f);
        }
    }
}
