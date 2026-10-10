using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 点到点的 2D 激光特效控制器。
    ///
    /// 工作模式:
    ///   - Quad 独立于 controller.transform 存在,放在世界空间
    ///   - 因此 controller.transform 移动不影响 Quad 的世界位置
    ///   - 修改 startPoint / endPoint 会立即把 Quad 摆到新位置
    ///
    /// 使用方式:
    ///   1) 准备 Material, Shader = "Game/LaserBeam2D"
    ///   2) 挂本组件到一个空对象上(任意位置)
    ///   3) 调用 <see cref="Show(Vector2, Vector2)"/> 或者 setter 设置两端
    ///   4) <see cref="Hide"/> 隐藏
    ///
    /// 注意:
    ///   本特效使用 additive 混合,起点 = UV.x = 0,终点 = UV.x = 1。
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteAlways]
    public class LaserBeamController : MonoBehaviour
    {
        // ===== Shader uniforms =====
        static readonly int StartPosId    = Shader.PropertyToID("_StartPos");
        static readonly int EndPosId      = Shader.PropertyToID("_EndPos");
        static readonly int StartRadiusId = Shader.PropertyToID("_StartRadius");
        static readonly int EndRadiusId   = Shader.PropertyToID("_EndRadius");
        static readonly int StartFracId   = Shader.PropertyToID("_StartFrac");
        static readonly int EndFracId     = Shader.PropertyToID("_EndFrac");

        [Header("Material")]
        [Tooltip("Shader = Game/LaserBeam2D 的材质")]
        [SerializeField] Material laserMaterial;

        [Header("Endpoints")]
        [Tooltip("激光起点(本地坐标,见 followTransform)\n" +
                 "followTransform=true: 相对控制器自身,控制器移动时自动跟随\n" +
                 "followTransform=false: 视为世界坐标")]
        [SerializeField] Vector2 startPoint = new Vector2(-2f, 0f);

        [Tooltip("激光终点(同上)")]
        [SerializeField] Vector2 endPoint   = new Vector2( 2f, 0f);

        [Tooltip("Quad 的 Z 深度(正方向远离相机)。负数会被推到场景更深处")]
        [SerializeField] float quadZ = 0f;

        [Tooltip("起点端的半径 (光斑/漩涡大小, 世界单位)")]
        [SerializeField, Min(0.05f)] float startRadius = 0.6f;

        [Tooltip("终点端的半径 (光斑/冲击大小, 世界单位)")]
        [SerializeField, Min(0.05f)] float endRadius   = 0.6f;

        [Header("Follow")]
        [Tooltip("是否每帧把 controller.transform.position 加到 startPoint/endPoint 上\n" +
                 "勾选后,startPoint/endPoint 是相对控制器自身的本地坐标\n" +
                 "控制器移动时,激光两端会自动跟随")]
        [SerializeField] bool followTransform = true;

        [Header("Render")]
        [Tooltip("激光粗细 (世界单位,中间光束的宽度)")]
        [SerializeField, Min(0.01f)] float thickness = 0.4f;

        [Tooltip("起始时是否立即显示")]
        [SerializeField] bool playOnStart = true;

        // 运行时:创建的 Quad (独立存在,不在 controller 的 transform 下)
        GameObject _quad;
        Renderer   _rdr;
        Material   _inst;
        bool       _ownsMaterial;

        // 上一次 ApplyTransform 算出来的 quadLength
        // 用于把 (startPoint, endPoint) 映射到 UV.x 范围,告诉 shader 在哪个 UV 区间画 beam body
        float _lastQuadLength = 1f;

        // ===== Endpoints 属性: setter 会自动推送 uniforms =====
        /// <summary>起点 (世界坐标)。修改后自动同步到 Shader。</summary>
        public Vector2 StartPoint
        {
            get => startPoint;
            set
            {
                if (startPoint == value) return;
                startPoint = value;
                MarkDirty();
            }
        }

        /// <summary>终点 (世界坐标)。修改后自动同步到 Shader。</summary>
        public Vector2 EndPoint
        {
            get => endPoint;
            set
            {
                if (endPoint == value) return;
                endPoint = value;
                MarkDirty();
            }
        }

        /// <summary>起点半径 (世界单位)</summary>
        public float StartRadius
        {
            get => startRadius;
            set { startRadius = Mathf.Max(0.05f, value); MarkDirty(); }
        }

        /// <summary>终点半径 (世界单位)</summary>
        public float EndRadius
        {
            get => endRadius;
            set { endRadius = Mathf.Max(0.05f, value); MarkDirty(); }
        }

        /// <summary>激光粗细 (世界单位)</summary>
        public float Thickness
        {
            get => thickness;
            set { thickness = Mathf.Max(0.01f, value); MarkDirty(); }
        }

        bool _dirty;

        void MarkDirty()
        {
            _dirty = true;
        }

        void Awake()
        {
            EnsureQuad();
            if (playOnStart) Show();
            else             Hide();
        }

        void OnEnable()
        {
            if (_quad != null && playOnStart)
            {
                ApplyTransform();
                ApplyEndpoints();
            }
        }

        void LateUpdate()
        {
            if (_quad == null) return;

            // 显示状态下:每帧都重算 Quad transform + 推送 uniform
            // (不需要缓存判断,代价可忽略,逻辑最不容易出错)
            if (_rdr != null && _rdr.enabled)
            {
                ApplyTransform();
                ApplyEndpoints();
                _dirty = false;
            }
        }

        void OnDestroy()
        {
            if (_inst)
            {
                if (Application.isPlaying) Destroy(_inst);
                else                       DestroyImmediate(_inst);
                _inst = null;
            }
            if (_ownsMaterial && laserMaterial)
            {
                if (Application.isPlaying) Destroy(laserMaterial);
                else                       DestroyImmediate(laserMaterial);
                laserMaterial = null;
                _ownsMaterial = false;
            }
            if (_quad)
            {
                if (Application.isPlaying) Destroy(_quad);
                else                       DestroyImmediate(_quad);
                _quad = null;
            }
        }

        /// <summary>创建 Quad / MeshRenderer / 材质实例(只创建一次)</summary>
        void EnsureQuad()
        {
            if (_quad != null) return;

            _quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _quad.name = "LaserBeam_Quad";
            _quad.hideFlags = HideFlags.DontSave;
            // 关键:Quad 不挂父子,直接放世界空间,避免与 controller.transform 叠加造成偏移
            _quad.transform.SetParent(null, worldPositionStays: false);

            // Quad 自带 Mesh Collider,激光特效不参与物理,删掉
            var col = _quad.GetComponent<Collider>();
            if (col) Destroy(col);

            _rdr = _quad.GetComponent<MeshRenderer>();
            if (_rdr == null)
            {
                _rdr = _quad.AddComponent<MeshRenderer>();
            }
            _rdr.shadowCastingMode    = UnityEngine.Rendering.ShadowCastingMode.Off;
            _rdr.receiveShadows       = false;
            _rdr.lightProbeUsage      = UnityEngine.Rendering.LightProbeUsage.Off;
            _rdr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            if (laserMaterial == null && !_ownsMaterial)
            {
                var fallback = Shader.Find("Game/LaserBeam2D");
                if (fallback != null)
                {
                    laserMaterial = new Material(fallback);
                    _ownsMaterial = true;
                }
            }

            if (laserMaterial != null)
            {
                _inst = new Material(laserMaterial);
                _rdr.sharedMaterial = _inst;
            }
            else
            {
                Debug.LogWarning("[LaserBeam] 未指定 laserMaterial,且找不到 Game/LaserBeam2D Shader。", this);
            }
        }

        /// <summary>把 Quad 摆到从 start 到 end 的位置 + 旋转 + 缩放,
        /// 尺寸要覆盖两端圆形光斑。
        ///
        /// 关键: Quad 的左右两个端点(world space) = 两端圆形的圆心 ± 沿激光方向的半径,
        ///       这样圆心正好落在 Quad 的两个端顶点上。</summary>
        void ApplyTransform()
        {
            if (_quad == null) return;

            Vector3 posStart = GetWorldStartPoint();
            Vector3 posEnd   = GetWorldEndPoint();

            Vector2 dir2   = (Vector2)(posEnd - posStart);
            float   length = dir2.magnitude;
            if (length < 1e-4f) length = 0.0001f;
            Vector2 dirUnit = dir2 / dir2.magnitude;

            // —— Quad 沿激光方向的左右两端 = 两圆各自最外缘 ——
            // 起点圆左缘 = startPoint - dirUnit * startRadius
            // 终点圆右缘 = endPoint   + dirUnit * endRadius
            Vector2 leftEdge  = (Vector2)posStart - dirUnit * startRadius;
            Vector2 rightEdge = (Vector2)posEnd   + dirUnit * endRadius;

            Vector2 mid    = (leftEdge + rightEdge) * 0.5f;
            float   quadLength = Vector2.Distance(leftEdge, rightEdge);

            // —— 高度方向: 让两端圆形都不被 Quad 上下边裁掉 ——
            // Quad 半高 = max(thickness/2, max(startRadius, endRadius))
            float halfH = Mathf.Max(thickness * 0.5f, Mathf.Max(startRadius, endRadius));
            float quadHeight = halfH * 2.0f;

            float angle = Mathf.Atan2(dirUnit.y, dirUnit.x) * Mathf.Rad2Deg;

            _quad.transform.position = new Vector3(mid.x, mid.y, quadZ);
            _quad.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            _quad.transform.localScale = new Vector3(quadLength, quadHeight, 1f);

            // 缓存给 ApplyEndpoints 用
            _lastQuadLength = quadLength;
        }

        /// <summary>把端点位置 / 半径 / beam trim 推到 Shader(世界坐标)</summary>
        void ApplyEndpoints()
        {
            if (_inst == null) return;
            Vector3 ws = GetWorldStartPoint();
            Vector3 we = GetWorldEndPoint();
            _inst.SetVector(StartPosId,    new Vector4(ws.x, ws.y, 0, 0));
            _inst.SetVector(EndPosId,      new Vector4(we.x, we.y, 0, 0));
            _inst.SetFloat (StartRadiusId, startRadius);
            _inst.SetFloat (EndRadiusId,   endRadius);

            // —— Beam body 修剪范围 ——
            // Quad 范围: UV.x = 0  对应 world  = leftEdge
            //           UV.x = 1  对应 world  = rightEdge
            // 总长度 = quadLength = length + startRadius + endRadius
            // 起点圆心 = startPoint = leftEdge + startRadius * dirUnit
            //   → UV.x  = startRadius / quadLength = startFrac
            // 终点圆心 = endPoint   = rightEdge - endRadius * dirUnit
            //   → UV.x  = (quadLength - endRadius) / quadLength = 1 - endRadius/quadLength = endFrac
            float ql = Mathf.Max(_lastQuadLength, 1e-4f);
            float startFrac = startRadius / ql;
            float endFrac   = 1.0f - endRadius / ql;
            _inst.SetFloat(StartFracId, startFrac);
            _inst.SetFloat(EndFracId,   endFrac);
        }

        Vector3 GetWorldStartPoint()
        {
            return followTransform
                ? (Vector3)startPoint + transform.position
                : (Vector3)startPoint;
        }

        Vector3 GetWorldEndPoint()
        {
            return followTransform
                ? (Vector3)endPoint + transform.position
                : (Vector3)endPoint;
        }

        /// <summary>显示激光并更新端点</summary>
        public void Show(Vector2 start, Vector2 end)
        {
            startPoint = start;
            endPoint   = end;
            Show();
        }

        /// <summary>显示激光(使用已配置的 startPoint / endPoint)</summary>
        public void Show()
        {
            EnsureQuad();
            if (_rdr == null) return;
            _rdr.enabled = true;
            ApplyTransform();
            ApplyEndpoints();
            _dirty = false;
        }

        /// <summary>隐藏激光</summary>
        public void Hide()
        {
            if (_rdr != null) _rdr.enabled = false;
        }

        /// <summary>是否正在显示</summary>
        public bool IsShowing => _rdr != null && _rdr.enabled;

        // ===== Scene 可视化辅助: 端点位置 / 半径在 Scene 视图直接可视化 =====
        // (这些是世界坐标,Gizmos 自身就用世界坐标画,不需要额外处理)
#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Vector3 ws = GetWorldStartPoint();
            Vector3 we = GetWorldEndPoint();

            // 起点圆 (黄)
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.9f);
            DrawCircle(ws + Vector3.forward * -0.1f, startRadius, 32);

            // 终点圆 (橙红)
            Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.9f);
            DrawCircle(we + Vector3.forward * -0.1f, endRadius, 32);

            // 连接线
            Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.8f);
            Gizmos.DrawLine(ws, we);

            // 中心点
            Vector3 mid = (ws + we) * 0.5f;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(mid, 0.05f);
        }

        static void DrawCircle(Vector3 center, float radius, int segments)
        {
            if (radius <= 0f) return;
            float step = Mathf.PI * 2f / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float a = step * i;
                Vector3 cur = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                Gizmos.DrawLine(prev, cur);
                prev = cur;
            }
        }

        void OnValidate()
        {
            if (thickness    < 0.01f) thickness    = 0.01f;
            if (startRadius  < 0.05f) startRadius  = 0.05f;
            if (endRadius    < 0.05f) endRadius    = 0.05f;

            if (!Application.isPlaying)
            {
                EnsureQuad();
                if (_rdr != null && _rdr.enabled)
                {
                    ApplyTransform();
                    ApplyEndpoints();
                }
            }
        }
#endif
    }
}