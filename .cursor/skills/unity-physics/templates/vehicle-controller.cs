using UnityEngine;

namespace MyGame.PhysicsModule
{
    /// <summary>
    /// WheelCollider 车辆控制器。
    /// 需要：Rigidbody（mass 建议 1500）+ 4 个 WheelCollider + 对应视觉 Mesh。
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleController : MonoBehaviour
    {
        [Header("轮子")]
        [SerializeField] private WheelCollider frontLeft;
        [SerializeField] private WheelCollider frontRight;
        [SerializeField] private WheelCollider rearLeft;
        [SerializeField] private WheelCollider rearRight;

        [Header("视觉轮子（可选，会自动同步）")]
        [SerializeField] private Transform visualFrontLeft;
        [SerializeField] private Transform visualFrontRight;
        [SerializeField] private Transform visualRearLeft;
        [SerializeField] private Transform visualRearRight;

        [Header("动力参数")]
        [SerializeField] private float maxMotorTorque = 2000f;
        [SerializeField] private float maxBrakeTorque = 4000f;
        [SerializeField] private float maxSteerAngle = 32f;
        [SerializeField] private float steerSpeed = 8f;

        [Header("空气动力学")]
        [SerializeField] private float downforceCoefficient = 2f;
        [SerializeField] private float highSpeedSteerReduction = 0.35f;
        [SerializeField] private float highSpeedThreshold = 40f;

        private Rigidbody _rb;
        private float _currentSteerAngle;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.centerOfMass = new Vector3(0f, -0.35f, 0f);   // 降低重心防翻车
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Update()
        {
            float steerInput = Input.GetAxisRaw("Horizontal");
            float throttleInput = Input.GetAxisRaw("Vertical");
            bool braking = Input.GetKey(KeyCode.Space);

            float speed = _rb.velocity.magnitude;

            // 高速时降低转向灵敏度
            float steerLimit = Mathf.Lerp(maxSteerAngle, maxSteerAngle * highSpeedSteerReduction,
                Mathf.Clamp01(speed / highSpeedThreshold));

            float targetSteer = steerInput * steerLimit;
            _currentSteerAngle = Mathf.MoveTowards(_currentSteerAngle, targetSteer, steerSpeed * steerLimit * Time.deltaTime);

            frontLeft.steerAngle = _currentSteerAngle;
            frontRight.steerAngle = _currentSteerAngle;

            // 后轮驱动
            rearLeft.motorTorque = throttleInput * maxMotorTorque;
            rearRight.motorTorque = throttleInput * maxMotorTorque;

            float brake = braking ? maxBrakeTorque : 0f;
            frontLeft.brakeTorque = brake;
            frontRight.brakeTorque = brake;
            rearLeft.brakeTorque = brake * 0.5f;
            rearRight.brakeTorque = brake * 0.5f;

            // 下压力，抑制高速翻车
            _rb.AddForce(-transform.up * downforceCoefficient * speed * speed * Time.deltaTime, ForceMode.Force);
        }

        private void FixedUpdate()
        {
            UpdateWheelVisual(frontLeft, visualFrontLeft);
            UpdateWheelVisual(frontRight, visualFrontRight);
            UpdateWheelVisual(rearLeft, visualRearLeft);
            UpdateWheelVisual(rearRight, visualRearRight);
        }

        private static void UpdateWheelVisual(WheelCollider wheel, Transform visual)
        {
            if (wheel == null || visual == null)
            {
                return;
            }

            wheel.GetWorldPose(out var position, out var rotation);
            visual.SetPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// 设置油门与转向（供 AI 或网络输入使用）。
        /// </summary>
        public void SetInput(float throttle, float steer)
        {
            float steerLimit = maxSteerAngle;
            frontLeft.steerAngle = steer * steerLimit;
            frontRight.steerAngle = steer * steerLimit;

            rearLeft.motorTorque = throttle * maxMotorTorque;
            rearRight.motorTorque = throttle * maxMotorTorque;
        }
    }
}
