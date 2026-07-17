using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 体积感系统演示组件
    ///
    /// 重要设计原则：
    /// - 本 Demo 只负责"意图"：玩家移动、怪物 AI（聚集/徘徊）、击退触发
    /// - "碰撞响应"（分离、挤压、软排斥）全部由 VolumeManager 负责
    /// - 不在 Demo 里写实体间的排斥逻辑，避免与 VolumeManager 重复计算
    /// </summary>
    public class VolumeDemo : MonoBehaviour
    {
        [Header("体积系统设置")]
        [Tooltip("体积管理器引用（为空则自动创建）")]
        public VolumeManager VolumeManager;

        [Header("玩家设置")]
        [Tooltip("玩家半径")]
        public float PlayerRadius = 0.5f;

        [Tooltip("玩家质量")]
        public float PlayerMass = 2f;

        [Tooltip("玩家最大重叠比率")]
        [Range(0f, 1f)]
        public float PlayerMaxOverlapRatio = 0.2f;

        [Tooltip("玩家移动速度")]
        public float PlayerSpeed = 8f;

        [Header("怪物设置")]
        [Tooltip("怪物数量")]
        public int MonsterCount = 20;

        [Tooltip("怪物半径范围")]
        public Vector2 MonsterRadiusRange = new(0.3f, 0.6f);

        [Tooltip("怪物质量范围")]
        public Vector2 MonsterMassRange = new(0.5f, 2f);

        [Tooltip("怪物移动速度")]
        public float MonsterSpeed = 2f;

        [Header("怪物AI（意图部分）")]
        [Tooltip("怪物聚集力（向玩家移动的强度，AI 意图）")]
        public float MonsterAttractionForce = 1f;

        [Tooltip("怪物随机扰动力（徘徊，AI 意图）")]
        public float MonsterWanderForce = 0.5f;

        [Header("击退设置")]
        [Tooltip("启用链式击退")]
        public bool EnableChainKnockback = true;

        [Tooltip("击退衰减率")]
        [Range(0f, 1f)]
        public float KnockbackDecayRatio = 0.6f;

        [Tooltip("链式击退检测范围乘数")]
        public float ChainKnockbackRangeMultiplier = 1.5f;

        [Header("调试显示")]
        [Tooltip("显示玩家半径")]
        public bool ShowPlayerRadius = true;

        [Tooltip("显示怪物半径")]
        public bool ShowMonsterRadius = true;

        [Tooltip("显示连线")]
        public bool ShowLines;

        // 运行时
        private TopDownController2D _player;
        private readonly List<TopDownController2D> _monsters = new();
        private Vector2 _playerMoveInput;

        // 预制体颜色
        private static readonly Color PlayerColor = new(0.2f, 0.5f, 1f, 1f);
        private static readonly Color[] MonsterColors = {
            new(1f, 0.3f, 0.3f, 1f),
            new(1f, 0.6f, 0.2f, 1f),
            new(0.9f, 0.4f, 0.4f, 1f),
            new(0.8f, 0.5f, 0.3f, 1f),
            new(1f, 0.4f, 0.4f, 1f),
        };

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void Initialize()
        {
            // 确保体积管理器存在
            if (VolumeManager == null)
            {
                var existing = FindAnyObjectByType<VolumeManager>();
                if (existing != null)
                {
                    VolumeManager = existing;
                }
                else
                {
                    var go = new GameObject("VolumeManager");
                    VolumeManager = go.AddComponent<VolumeManager>();
                }
            }

            // 配置体积管理器
            VolumeManager.EnableChainKnockback = EnableChainKnockback;
            VolumeManager.ChainDecayRatio = KnockbackDecayRatio;
            VolumeManager.ChainKnockbackRadiusMultiplier = ChainKnockbackRangeMultiplier;
            VolumeManager.ShowAllGizmos = ShowMonsterRadius;

            // 创建玩家和怪物
            CreatePlayer();
            CreateMonsters();
        }

        protected virtual void CreatePlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "VolumeDemo_Player";
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one * PlayerRadius * 2;

            var renderer = go.GetComponent<Renderer>();
            renderer.material.color = PlayerColor;

            _player = go.AddComponent<TopDownController2D>();
            _player.Radius = PlayerRadius;
            _player.Mass = PlayerMass;
            _player.MaxOverlapRatio = PlayerMaxOverlapRatio;
            _player.GizmosColor = PlayerColor;

            VolumeManager.Register(_player);
        }

        protected virtual void CreateMonsters()
        {
            for (int i = 0; i < MonsterCount; i++)
            {
                CreateMonster(i);
            }
        }

        protected virtual TopDownController2D CreateMonster(int index)
        {
            Vector2 spawnPos = Random.insideUnitCircle * 10f;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"VolumeDemo_Monster_{index}";
            go.transform.position = spawnPos;

            float radius = Random.Range(MonsterRadiusRange.x, MonsterRadiusRange.y);
            go.transform.localScale = Vector3.one * (radius * 2);

            Color color = MonsterColors[index % MonsterColors.Length];
            go.GetComponent<Renderer>().material.color = color;

            var body = go.AddComponent<TopDownController2D>();
            body.Radius = radius;
            body.Mass = Random.Range(MonsterMassRange.x, MonsterMassRange.y);
            body.MaxOverlapRatio = Random.Range(0.1f, 0.3f);
            body.GizmosColor = color;

            VolumeManager.Register(body);
            _monsters.Add(body);

            return body;
        }

        protected virtual void Update()
        {
            HandleInput();
            ApplyMonsterIntent();
        }

        protected virtual void HandleInput()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _playerMoveInput = new Vector2(h, v).normalized;
        }

        /// <summary>
        /// 怪物的"意图"控制：聚集到玩家 + 随机徘徊
        /// 注意：这里**不写**实体间排斥，排斥由 VolumeManager 统一处理
        /// </summary>
        protected virtual void ApplyMonsterIntent()
        {
            if (_player == null) return;

            foreach (var monster in _monsters)
            {
                if (monster == null) continue;

                // 意图向量：聚集玩家 + 随机扰动
                Vector2 intent = Vector2.zero;

                // 1. 聚集玩家
                Vector2 toPlayer = _player.Position - monster.Position;
                float distToPlayer = toPlayer.magnitude;
                if (distToPlayer > 0.1f)
                {
                    intent += toPlayer.normalized * MonsterAttractionForce;
                }

                // 2. 随机徘徊（让怪物移动看起来更自然）
                intent += Random.insideUnitCircle * MonsterWanderForce;

                // 将意图转换为期望速度
                Vector2 desiredVelocity = intent.normalized * MonsterSpeed;

                // 平滑过渡到期望速度（保留原有速度，让 VolumeManager 处理碰撞反应）
                monster.Velocity = Vector2.Lerp(monster.Velocity, desiredVelocity, Time.deltaTime * 5f);
            }
        }

        protected virtual void FixedUpdate()
        {
            // 玩家移动
            if (_player != null)
            {
                Vector2 targetVel = _playerMoveInput * PlayerSpeed;
                _player.Velocity = Vector2.Lerp(_player.Velocity, targetVel, Time.fixedDeltaTime * 10f);
                _player.Position += (Vector2)_player.Velocity * Time.fixedDeltaTime;
                _player.transform.position = _player.Position;
            }

            // 怪物移动（应用速度到位置）
            foreach (var monster in _monsters)
            {
                if (monster == null) continue;
                monster.Position += (Vector2)monster.Velocity * Time.fixedDeltaTime;
                monster.transform.position = monster.Position;
            }
        }

        protected virtual void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));

            GUILayout.Label("=== 体积感系统演示 ===", GUI.skin.box);
            GUILayout.Space(5);

            GUILayout.Label($"玩家位置: {_player?.Position ?? Vector2.zero:F2}");
            GUILayout.Label($"怪物数量: {_monsters.Count}");
            GUILayout.Label($"FPS: {1f / Time.deltaTime:F0}");

            GUILayout.Space(10);
            GUILayout.Label("操作说明:", GUI.skin.box);
            GUILayout.Label("WASD/方向键 - 移动玩家");
            GUILayout.Label("点击鼠标左键 - 击退周围怪物");
            GUILayout.Label("点击鼠标右键 - 强力击退（触发链式）");
            GUILayout.Label("空格 - 生成新怪物");
            GUILayout.Label("R - 重置位置");

            GUILayout.Space(10);
            GUILayout.Label("碰撞响应（在 VolumeManager 中）:", GUI.skin.box);
            GUILayout.Label("- EnableSoftRepulsion: 软排斥");
            GUILayout.Label("- SoftRepulsionStrength: 排斥强度");
            GUILayout.Label("- SoftRepulsionDistanceRatio: 排斥范围");
            GUILayout.Label("- BaseSeparationForce: 硬分离力");

            GUILayout.EndArea();

            // 处理输入
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateMonster(_monsters.Count);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPositions();
            }

            // 鼠标点击击退
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                HandleMouseKnockback(Input.GetMouseButtonDown(1));
            }
        }

        protected virtual void HandleMouseKnockback(bool isStrong)
        {
            if (_player == null || VolumeManager == null) return;

            Vector3 mouseWorldPos = Camera.main?.ScreenToWorldPoint(Input.mousePosition) ?? Input.mousePosition;
            Vector2 knockbackCenter = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

            float knockbackRadius = isStrong ? 5f : 2f;
            float knockbackForce = isStrong ? 20f : 10f;

            foreach (var monster in _monsters)
            {
                if (monster == null) continue;

                Vector2 toMonster = monster.Position - knockbackCenter;
                float dist = toMonster.magnitude;

                if (dist < knockbackRadius)
                {
                    float forceMultiplier = 1f - (dist / knockbackRadius);
                    Vector2 knockbackDir = toMonster.magnitude > 0.01f ? toMonster.normalized : Random.insideUnitCircle;
                    VolumeManager.ApplyKnockback(monster, knockbackDir, knockbackForce * forceMultiplier);
                }
            }
        }

        protected virtual void ResetPositions()
        {
            if (_player != null)
            {
                _player.Position = Vector2.zero;
                _player.Velocity = Vector2.zero;
                _player.transform.position = Vector3.zero;
            }

            foreach (var monster in _monsters)
            {
                if (monster == null) continue;
                monster.Position = Random.insideUnitCircle * 10f;
                monster.Velocity = Vector2.zero;
                monster.transform.position = monster.Position;
            }
        }

        protected virtual void OnDestroy()
        {
            if (VolumeManager != null)
            {
                if (_player != null) VolumeManager.Unregister(_player);
                foreach (var monster in _monsters)
                {
                    if (monster != null) VolumeManager.Unregister(monster);
                }
            }

            if (_player != null) Destroy(_player.gameObject);
            foreach (var monster in _monsters)
            {
                if (monster != null) Destroy(monster.gameObject);
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (!ShowLines) return;
            if (_player == null) return;

            Gizmos.color = Color.yellow;
            foreach (var monster in _monsters)
            {
                if (monster == null) continue;
                Gizmos.DrawLine(_player.transform.position, monster.transform.position);
            }
        }
    }
}