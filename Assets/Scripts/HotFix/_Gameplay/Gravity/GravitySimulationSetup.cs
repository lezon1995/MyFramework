using UnityEngine;

namespace MoreMountains.Gravity
{
    /// <summary>
    /// 演示场景：自动生成行星和物体 A，Gizmos 可视化引力范围和轨迹。
    /// 在场景中新建空对象，加上此组件即可运行。
    /// </summary>
    [RequireComponent(typeof(GravitySource))]
    public class GravitySimulationSetup : MonoBehaviour
    {
        [Header("行星")]
        [Range(0.1f, 5f)]
        public float planetVisualRadius = 1f;

        [Header("引力范围")]
        [Range(1f, 50f)]
        public float gravityRange = 15f;

        [Header("坠落时间区间（秒）")]
        [Range(0.1f, 30f)]
        public float minDuration = 1f;

        [Range(0.1f, 30f)]
        public float maxDuration = 5f;

        [Header("物体 A（受引力体）")]
        public GameObject objectAPrefab;

        [Range(0.05f, 2f)]
        public float objectAVisualRadius = 0.3f;

        [Range(0f, 50f)]
        public float initialSpeed = 8f;

        [Range(-180f, 180f)]
        public float launchAngle = 0f;

        [Header("轨迹可视化")]
        public bool drawTrajectory = true;
        public int trajectorySteps = 200;
        public float trajectoryDt = 0.05f;

        [Header("Debug 可视化")]
        public bool showGizmos = true;
        public bool showGravityRange = true;

        GravitySource _planetSource;
        GravityBody _objectA;
        GameObject _objectAGo;

        void Awake()
        {
            _planetSource = GetComponent<GravitySource>();
            _planetSource.gravityRange = gravityRange;
            _planetSource.minDuration = minDuration;
            _planetSource.maxDuration = maxDuration;

            SpawnObjectA();
        }

        void SpawnObjectA()
        {
            float rad = launchAngle * Mathf.Deg2Rad;

            // 物体 A 初始位置在引力范围边界上（引力范围外）
            Vector3 spawnPos = transform.position
                + new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * gravityRange;

            if (objectAPrefab != null)
            {
                _objectAGo = Instantiate(objectAPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                _objectAGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _objectAGo.transform.position = spawnPos;
                _objectAGo.transform.localScale = Vector3.one * objectAVisualRadius * 2f;
            }

            _objectA = _objectAGo.GetComponent<GravityBody>();
            if (_objectA == null)
                _objectA = _objectAGo.AddComponent<GravityBody>();

            // 速度方向：从引力范围边界指向行星（会自然进入引力范围）
            _objectA.initialVelocity = new Vector3(-Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * initialSpeed;
            _objectA.crashRadius = planetVisualRadius;
        }

        void OnDestroy()
        {
            if (_objectAGo != null)
                Destroy(_objectAGo);
        }

        void OnDrawGizmos()
        {
            if (!showGizmos) return;

            float range = (Application.isPlaying && _planetSource != null)
                ? _planetSource.gravityRange : gravityRange;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, planetVisualRadius);

            if (showGravityRange)
            {
                Gizmos.color = new Color(0.3f, 0.7f, 1f, 0.25f);
                Gizmos.DrawWireSphere(transform.position, range);
            }

            if (_objectAGo != null && _objectA != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_objectAGo.transform.position, objectAVisualRadius);

                if (Application.isPlaying)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(_objectAGo.transform.position, _objectA.Velocity * 0.3f);
                }
            }

            if (drawTrajectory && _objectA != null && Application.isPlaying)
                DrawPredictedTrajectory();
        }

        void DrawPredictedTrajectory()
        {
            if (_objectA.CurrentState == GravityBody.State.Crashed) return;

            Vector3 planetPos = transform.position;
            Vector3 pos = _objectAGo.transform.position;
            Vector3 vel = _objectA.Velocity;
            float rangeSqr = _planetSource.gravityRange * _planetSource.gravityRange;

            Gizmos.color = new Color(1f, 1f, 0f, 0.6f);

            // 阶段一：匀速直线飞行直到进入引力范围
            for (int i = 0; i < trajectorySteps; i++)
            {
                Vector3 toward = planetPos - pos;
                if (toward.magnitude < planetVisualRadius) return;

                if (toward.sqrMagnitude <= rangeSqr)
                {
                    // 进入引力范围后改为螺旋轨迹
                    DrawSpiralTrajectory(pos, vel, planetPos);
                    return;
                }

                pos += vel * trajectoryDt;
                if (i % 3 == 0)
                    Gizmos.DrawSphere(pos, 0.04f);
            }
        }

        void DrawSpiralTrajectory(Vector3 entryPos, Vector3 vel, Vector3 planetPos)
        {
            float rEntry = (entryPos - planetPos).magnitude;
            Vector3 normal = (entryPos - planetPos).normalized;
            float angleDeg = Mathf.Clamp(Vector3.Angle(-vel, normal), 0f, 90f);
            float sinA = Mathf.Sin(angleDeg * Mathf.Deg2Rad);
            float duration = Mathf.Lerp(_planetSource.minDuration, _planetSource.maxDuration, angleDeg / 90f);

            // -normal 始终指向 entry 点（物体进入侧），velocity 减去它在 -normal 上的投影
            // = velocity 沿切向的分量，即指向行星内侧的切向
            Vector3 inwardNormal = -normal;
            Vector3 tangent2D = vel - inwardNormal * Vector3.Dot(vel, inwardNormal);
            Vector3 tangent = tangent2D.normalized;

            int steps = Mathf.CeilToInt(duration / trajectoryDt);
            Vector3 prevPos = entryPos;

            for (int i = 1; i <= steps; i++)
            {
                float t = Mathf.Clamp01((float)i * trajectoryDt / duration);
                float offset = sinA * rEntry * 4f * t * (1f - t);
                Vector3 spiral = Vector3.Lerp(entryPos, planetPos, t) + tangent * offset;
                if (i % 3 == 0)
                    Gizmos.DrawLine(prevPos, spiral);
                prevPos = spiral;
                if ((spiral - planetPos).magnitude < planetVisualRadius) break;
            }
        }

        [ContextMenu("Re-launch Object A")]
        public void RelaunchObjectA()
        {
            if (_objectAGo != null)
            {
                Destroy(_objectAGo);
                _objectA = null;
                _objectAGo = null;
            }
            SpawnObjectA();
        }
    }
}
