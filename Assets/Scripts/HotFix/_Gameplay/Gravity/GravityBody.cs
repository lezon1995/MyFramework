using UnityEngine;

namespace MoreMountains.Gravity
{
    /// <summary>
    /// 物体 A：进入引力范围后根据入射角映射坠落时间，沿螺旋轨迹坠向行星中心。
    /// 不使用物理引擎，完全由 FixedUpdate 驱动。
    ///
    /// 轨迹公式（t ∈ [0,1]，angle ∈ [0,90]）：
    ///   offset = sin(angle) * R_e * 4t(1-t)
    ///   pos(t) = Lerp(entryPos, planetPos, t) + offset * tangent
    ///   - angle=0：直线坠落
    ///   - angle=90：圆弧坠落
    /// </summary>
    public class GravityBody : MonoBehaviour
    {
        public enum State
        {
            Flying,
            Falling,
            Crashed,
        }

        [Header("初始速度（仅 Flying 阶段生效）")] 
        public Vector3 initialVelocity = new(0f, 0f, 0f);

        [Header("配置")] 
        [Tooltip("坠毁判定半径（设成行星视觉半径）")]
        public float crashRadius = 1f;

        [Tooltip("是否在坠毁后销毁 GameObject")] 
        public bool destroyOnCrash;

        [Header("运行时（只读）")] [SerializeField] State _state;
        [SerializeField] float _fallProgress; // 0→1

        public State CurrentState => _state;
        public Vector3 Velocity => _velocity;
        public float FallProgress => _fallProgress;

        Vector3 _velocity;

        // 坠落参数（Capture 时固定）
        Vector3 _entryPos;
        Vector3 _planetPos;
        Vector3 _tangent; // 垂直于 entry 半径方向的切向单位向量
        float _entryRadius; // 进入时的初始距离
        float _sinAngle; // 入射角的 sin 值
        float _fallDuration; // 实际坠落时间（秒）
        float _fallElapsed;
        GravitySource _source;

        void Awake()
        {
            _velocity = initialVelocity;
            _state = State.Flying;
        }

        void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            switch (_state)
            {
                case State.Flying:
                    HandleFlying(dt);
                    break;
                case State.Falling:
                    HandleFalling(dt);
                    break;
            }
        }

        /// <summary>自由飞行：直线匀速，直到进入某引力源的引力范围。</summary>
        void HandleFlying(float dt)
        {
            transform.position += _velocity * dt;

            var sources = FindObjectsOfType<GravitySource>();
            foreach (var s in sources)
            {
                if (s.IsWithinRange(transform.position))
                {
                    Capture(s);
                    return;
                }
            }
        }

        /// <summary>
        /// 进入引力范围的瞬间：计算入射角 → 查表得到坠落时间 → 锁定所有轨迹参数。
        /// </summary>
        void Capture(GravitySource source)
        {
            _source = source;
            _planetPos = source.Position;

            // 记录进入点
            _entryPos = transform.position;
            _entryRadius = (_entryPos - _planetPos).magnitude;

            // entry 点法线（从行星中心指向进入点，即引力方向的反向）
            Vector3 normal = (_entryPos - _planetPos).normalized;

            // 入射角：velocity 反方向与法线的夹角
            // velocity 方向指向飞行方向；-velocity 指向"来向"
            // 用 atan2 得到有符号角，再取绝对值得到 [0, 180]，映射到 [0, 90]
            float rawAngleDeg = Vector3.Angle(-_velocity, normal);
            float angleDeg = Mathf.Clamp(rawAngleDeg, 0f, 90f);

            // sin(angle) 用于轨迹公式
            _sinAngle = Mathf.Sin(angleDeg * Mathf.Deg2Rad);

            // 坠落时间：angle=0 → minDuration，angle=90 → maxDuration
            float t = angleDeg / 90f;
            _fallDuration = Mathf.Lerp(source.minDuration, source.maxDuration, t);

            // 切向单位向量：在 XY 平面内与 entry 半径方向垂直
            // -normal 始终指向 entry 点，即物体进入的那一侧（而不是行星中心）
            // velocity 减去它在 -normal 上的投影，剩余部分就是指向行星内侧的切向分量
            Vector3 inwardNormal = -normal;
            Vector3 tangent2D = _velocity - inwardNormal * Vector3.Dot(_velocity, inwardNormal);
            _tangent = tangent2D.normalized;

            _fallElapsed = 0f;
            _fallProgress = 0f;
            _state = State.Falling;

            Debug.Log($"[GravityBody] 捕获！入射角={angleDeg:F1}°，" + $"sin={_sinAngle:F3}，坠落时间={_fallDuration:F2}s，" + $"entry半径={_entryRadius:F2}，tangent={_tangent}");
        }

        /// <summary>
        /// 坠落阶段：用解析螺旋路径插值，每帧推进 _fallElapsed。
        /// </summary>
        void HandleFalling(float dt)
        {
            if (_source == null)
            {
                _state = State.Flying;
                return;
            }

            _fallElapsed += dt;
            float t = Mathf.Clamp01(_fallElapsed / _fallDuration);
            _fallProgress = t;

            // 螺旋轨迹：
            //   offset = sin(angle) × R_entry × 4t(1-t)
            //   pos = Lerp(entryPos, planetPos, t) + offset × tangent
            float offset = _sinAngle * _entryRadius * 4f * t * (1f - t);
            Vector3 radialLerp = Vector3.Lerp(_entryPos, _planetPos, t);
            transform.position = radialLerp + _tangent * offset;

            // 更新速度（用于可视化）
            // 下一帧位置差分
            float nextT = Mathf.Clamp01((_fallElapsed + dt) / _fallDuration);
            float nextOffset = _sinAngle * _entryRadius * 4f * nextT * (1f - nextT);
            Vector3 nextRadialLerp = Vector3.Lerp(_entryPos, _planetPos, nextT);
            Vector3 nextPos = nextRadialLerp + _tangent * nextOffset;
            _velocity = (nextPos - transform.position) / dt;

            // 坠毁检测：抵达行星中心附近
            if (t >= 1f || (_planetPos - transform.position).magnitude <= crashRadius)
            {
                transform.position = _planetPos;
                _state = State.Crashed;
                OnCrash(_source);
            }
        }

        /// <summary>坠毁回调，可被子类重写。</summary>
        protected virtual void OnCrash(GravitySource source)
        {
            Debug.Log($"[{name}] 坠入行星 {source.name}！");

            if (destroyOnCrash)
                Destroy(gameObject);
        }

        /// <summary>手动重置为自由飞行状态。</summary>
        public void ResetState(Vector3 newPosition, Vector3 newVelocity)
        {
            _velocity = newVelocity;
            transform.position = newPosition;
            _state = State.Flying;
            _fallElapsed = 0f;
            _fallProgress = 0f;
            _source = null;
        }
    }
}