using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 测试用：在Update中模拟金币掉落，并可视化椭圆掉落范围
    /// </summary>
    public class CoinSystemTester : MonoBehaviour
    {
        [Header("依赖")]
        public CoinManager coinManager;
        public Transform DropPoint;
        public CoinDropConfig coinDropConfig;

        [Header("按键")]
        public KeyCode dropKey = KeyCode.Space;
        public KeyCode clearKey = KeyCode.C;
        public KeyCode drop360Key = KeyCode.F;        // 360度均匀生成金币用于验证椭圆

        [Header("测试参数")]
        public float dropInterval = 0.3f;
        public int coinCountForFullCircle = 32;       // 360度生成的金币数（用于验证椭圆范围）

        [Header("椭圆可视化")]
        public bool visualizeEllipse = true;          // 是否显示椭圆范围
        public bool visualizeEllipseInGame = true;    // 是否在Game视图也显示
        public Color ellipseColor = new Color(1f, 0.8f, 0f, 1f);  // 椭圆线条颜色（金色）
        public Color groundCenterColor = new Color(0f, 1f, 0f, 1f); // 起点（椭圆中心）颜色
        public Color groundColor = new Color(1f, 0f, 0f, 1f);      // 实际落点颜色
        public float lineWidth = 0.05f;               // 线条宽度
        public int ellipseSampleCount = 64;           // 椭圆周长采样数
        public int bounceEllipseSampleCount = 48;     // 反弹椭圆采样数

        float _timer;
        LineRenderer _ellipseLine;       // 第一段（初始段）椭圆
        LineRenderer _bounceLine;        // 反弹段椭圆（衰减后）
        LineRenderer _centerMarkerLine;  // 起点十字标记
        LineRenderer _axisLine;          // 长半轴方向箭头
        LineRenderer _pickupRangeLine;   // 拾取范围圆

        void Awake()
        {
            TryFindLineRenderers();
        }

        void TryFindLineRenderers()
        {
            transform.Find("CoinEllipseVis_FirstSegment").TryGetComponent(out _ellipseLine);
            transform.Find("CoinEllipseVis_BounceSegment").TryGetComponent(out _bounceLine);
            transform.Find("CoinEllipseVis_Center").TryGetComponent(out _centerMarkerLine);
            transform.Find("CoinEllipseVis_Axis").TryGetComponent(out _axisLine);
            transform.Find("CoinEllipseVis_PickupRange").TryGetComponent(out _pickupRangeLine);
        }

        void Update()
        {
            if (coinManager == null)
                return;

            // 单次生成金币（朝鼠标方向）
            if (Input.GetKeyDown(dropKey))
            {
                Vector3 center = DropPoint != null ? DropPoint.position : transform.position;
                Vector2 randomDir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - center;
                coinManager.DropCoin(center, randomDir, 1, coinDropConfig);
            }

            // 清理金币
            if (Input.GetKeyDown(clearKey))
            {
                coinManager.ClearAllCoins();
            }

            // 360度均匀生成金币用于验证椭圆范围
            if (Input.GetKeyDown(drop360Key))
            {
                DropCoins360();
            }

            // 持续生成金币用于测试
            _timer += Time.deltaTime;
            if (_timer >= dropInterval)
            {
                _timer = 0f;
            }
        }

        /// <summary>
        /// 在 DropPoint 周围 360 度均匀生成金币
        /// 落点位置 = 各方向射线与椭圆边界的交点 → 落点集合形成椭圆
        /// </summary>
        void DropCoins360()
        {
            if (coinManager == null)
                return;

            Vector3 center = DropPoint != null ? DropPoint.position : transform.position;
            var cfg = coinDropConfig;

            for (int i = 0; i < coinCountForFullCircle; i++)
            {
                // 方向：均匀分布在 360 度
                float alpha = (float)i / coinCountForFullCircle * Mathf.PI * 2f;
                Vector2 baseDir = new Vector2(Mathf.Cos(alpha), Mathf.Sin(alpha));

                // DropCoin 内部会用椭圆射线交点计算落点
                coinManager.DropCoin(center, baseDir, 1, cfg);
            }
        }

        #region 椭圆可视化

        void OnEnable()
        {
            EnsureVisualizers();
        }

        void OnValidate()
        {
            UpdateEllipseVisualization();
        }

        void LateUpdate()
        {
            if (visualizeEllipse)
                UpdateEllipseVisualization();
        }

        /// <summary>
        /// 创建所有 LineRenderer（懒加载）
        /// </summary>
        void EnsureVisualizers()
        {
            TryFindLineRenderers();
            
            /*if (_ellipseLine == null)
                _ellipseLine = CreateLineRenderer("CoinEllipseVis_FirstSegment", ellipseColor);
            if (_bounceLine == null)
                _bounceLine = CreateLineRenderer("CoinEllipseVis_BounceSegment", new Color(1f, 0.5f, 0f, 0.8f));
            if (_centerMarkerLine == null)
                _centerMarkerLine = CreateLineRenderer("CoinEllipseVis_Center", groundCenterColor);
            if (_axisLine == null)
                _axisLine = CreateLineRenderer("CoinEllipseVis_Axis", new Color(0f, 0.5f, 1f, 1f));
            if (_pickupRangeLine == null)
                _pickupRangeLine = CreateLineRenderer("CoinEllipseVis_PickupRange", new Color(0f, 1f, 1f, 0.5f));*/

            // 拾取范围圆默认隐藏，由 UpdateEllipseVisualization 控制
            if (_pickupRangeLine != null)
                _pickupRangeLine.gameObject.SetActive(visualizeEllipseInGame);
        }

        /// <summary>
        /// 创建一个 LineRenderer 子对象
        /// </summary>
        LineRenderer CreateLineRenderer(string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var lr = go.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.numCapVertices = 2;
            return lr;
        }

        /// <summary>
        /// 更新椭圆范围可视化
        ///
        /// 显示三层：
        /// 1. 第一段椭圆（最大椭圆）：HorizontalSpread（=水平方向半径） + DropHeight（=垂直方向半径）
        /// 2. 反弹段椭圆（按 BounceDecayRatio 衰减）：两个半径都按 ratio 缩小
        /// 3. 主方向箭头 + 起点十字（主方向 = Vector2.right，约定俗成的 HorizontalSpread 方向）
        ///
        /// 椭圆方向约定：
        /// - 主方向 baseDirection = Vector2.right（X 轴正方向，= "HorizontalSpread 的方向"）
        /// - 沿 baseDirection 的最大半径 = HorizontalSpread（=水平的横向长度）
        /// - 垂直 baseDirection 的最大半径 = DropHeight（=上下的纵向高度）
        ///
        /// 即：HorizontalSpread 改大 → 椭圆在 X 方向变得更长；DropHeight 改大 → 椭圆在 Y 方向变得更高
        /// </summary>
        void UpdateEllipseVisualization()
        {
            if (coinManager == null || DropPoint == null)
            {
                SetLineActive(_ellipseLine, false);
                SetLineActive(_bounceLine, false);
                SetLineActive(_centerMarkerLine, false);
                SetLineActive(_axisLine, false);
                return;
            }

            EnsureVisualizers();

            // 控制 Game 视图是否显示
            SetLineActive(_ellipseLine, visualizeEllipseInGame);
            SetLineActive(_bounceLine, visualizeEllipseInGame);
            SetLineActive(_centerMarkerLine, visualizeEllipseInGame);
            SetLineActive(_axisLine, visualizeEllipseInGame);

            Vector2 center = DropPoint.position;
            var cfg = coinDropConfig;

            // === 第一段椭圆（最大范围）===
            // 沿 baseDirection 的水平半径 = HorizontalSpread
            // 垂直 baseDirection 的垂直半径 = DropHeight
            float horizontalRadius = Mathf.Max(0.01f, cfg?.HorizontalSpread ?? 1.5f);
            float verticalRadius = Mathf.Max(0.01f, cfg?.DropHeight ?? 1.2f);

            // 主方向：X 轴正方向（= "HorizontalSpread 方向"）
            // 这样 HorizontalSpread 改大 → 椭圆在水平方向变长（符合命名直觉）
            //    DropHeight 改大 → 椭圆在垂直方向变高
            Vector2 baseDir = Vector2.right;

            // 画第一段椭圆
            DrawEllipseSegment(_ellipseLine, center, horizontalRadius, verticalRadius, ellipseSampleCount);

            // === 反弹段椭圆（按衰减比例）===
            float decay = Mathf.Clamp01(cfg?.BounceDecayRatio ?? 0.6f);
            float bounceHRadius = horizontalRadius * decay;
            float bounceVRadius = verticalRadius * decay;
            DrawEllipseSegment(_bounceLine, center, bounceHRadius, bounceVRadius, bounceEllipseSampleCount);

            // === 起点十字标记 ===
            DrawCenterMarker(_centerMarkerLine, center, 0.3f);

            // === 主方向箭头（指向 HorizontalSpread 方向 = X 正方向）===
            DrawAxisArrow(_axisLine, center, baseDir, horizontalRadius);

            // 应用线条颜色/宽度
            ApplyLineStyle(_ellipseLine, ellipseColor);
            ApplyLineStyle(_bounceLine, new Color(1f, 0.5f, 0f, 0.8f));
            ApplyLineStyle(_centerMarkerLine, groundCenterColor);
            ApplyLineStyle(_axisLine, new Color(0f, 0.5f, 1f, 1f));
            ApplyLineStyle(_pickupRangeLine, new Color(0f, 1f, 1f, 0.5f));
        }

        /// <summary>
        /// 画一段椭圆
        /// </summary>
        void DrawEllipseSegment(LineRenderer lr, Vector2 center, float horizontalRadius, float verticalRadius, int samples)
        {
            if (lr == null)
                return;

            // 调用 SampleEllipsePerimeter 取得椭圆周长上的点序列
            var points = CoinEllipseScatter.SampleEllipsePerimeter(center, horizontalRadius, verticalRadius, samples);
            lr.positionCount = points.Length;
            for (int i = 0; i < points.Length; i++)
            {
                lr.SetPosition(i, new Vector3(points[i].x, points[i].y, DropPoint.position.z - 0.1f));
            }
        }

        /// <summary>
        /// 画起点十字
        /// </summary>
        void DrawCenterMarker(LineRenderer lr, Vector2 center, float size)
        {
            if (lr == null)
                return;

            lr.positionCount = 4;
            lr.SetPosition(0, new Vector3(center.x - size, center.y, DropPoint.position.z - 0.1f));
            lr.SetPosition(1, new Vector3(center.x + size, center.y, DropPoint.position.z - 0.1f));
            lr.SetPosition(2, new Vector3(center.x, center.y - size, DropPoint.position.z - 0.1f));
            lr.SetPosition(3, new Vector3(center.x, center.y + size, DropPoint.position.z - 0.1f));
        }

        /// <summary>
        /// 画长半轴方向箭头（从中心沿 baseDirection 长度 a）
        /// </summary>
        void DrawAxisArrow(LineRenderer lr, Vector2 center, Vector2 baseDirection, float length)
        {
            if (lr == null)
                return;

            Vector2 end = center + baseDirection.normalized * length;
            // 箭头线段：主轴 + 两个箭头分支
            Vector2 perpendicular = new Vector2(-baseDirection.y, baseDirection.x);
            float arrowSize = 0.15f;
            Vector2 arrowBase = end - baseDirection.normalized * arrowSize;

            Vector2 a1 = arrowBase + perpendicular * (arrowSize * 0.5f);
            Vector2 a2 = arrowBase - perpendicular * (arrowSize * 0.5f);

            lr.positionCount = 4;
            lr.SetPosition(0, new Vector3(center.x, center.y, DropPoint.position.z - 0.1f));
            lr.SetPosition(1, new Vector3(end.x, end.y, DropPoint.position.z - 0.1f));
            lr.SetPosition(2, new Vector3(end.x, end.y, DropPoint.position.z - 0.1f));
            lr.SetPosition(3, new Vector3(a1.x, a1.y, DropPoint.position.z - 0.1f));
        }

        void ApplyLineStyle(LineRenderer lr, Color color)
        {
            if (lr == null)
                return;
            lr.startColor = color;
            lr.endColor = color;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
        }

        void SetLineActive(LineRenderer lr, bool active)
        {
            if (lr != null)
                lr.gameObject.SetActive(active);
        }

        #endregion

        void OnGUI()
        {
            if (coinManager == null)
                return;

            GUILayout.BeginArea(new Rect(Screen.width - 280, 10, 270, 250));
            GUILayout.BeginVertical("box");
            GUILayout.Label($"=== Coin System Debug ===");
            GUILayout.Label($"Active Coins: {coinManager.GetActiveCoinCount()}");
            GUILayout.Label($"Pickup Range: {coinManager.PickupRange:F1}");
            GUILayout.Label($"Drop Duration: {coinDropConfig?.DropDuration:F2}s");
            GUILayout.Label($"Bounce Count: {coinDropConfig?.BounceCount}");
            GUILayout.Label($"Horizontal (X) Radius: {coinDropConfig?.HorizontalSpread:F2}");
            GUILayout.Label($"Vertical (Y) Radius: {coinDropConfig?.DropHeight:F2}");
            GUILayout.Label($"Decay: {coinDropConfig?.BounceDecayRatio:F2}");
            GUILayout.Label($"Spread: {coinDropConfig?.DirectionSpreadAngle:F0}° (use if BounceSpreadAngle<0)");
            GUILayout.Label($"Bounce Spread: {coinDropConfig?.BounceSpreadAngle:F0}°");
            GUILayout.Space(5);
            GUILayout.Label($"[{dropKey}] Drop coin at mouse");
            GUILayout.Label($"[{drop360Key}] Drop coins in 360° ({coinCountForFullCircle})");
            GUILayout.Label($"[{clearKey}] Clear all coins");
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
