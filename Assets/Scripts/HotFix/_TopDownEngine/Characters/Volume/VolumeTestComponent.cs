using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 体积系统测试组件
    /// 用于演示和测试体积感系统
    /// </summary>
    public class VolumeTestComponent : MonoBehaviour
    {
        [Header("测试设置")]
        [Tooltip("是否启用测试")]
        public bool Enabled = true;

        [Tooltip("测试用的实体预设")]
        public GameObject TestEntityPrefab;

        [Tooltip("初始测试实体数量")]
        public int InitialEntityCount = 10;

        [Tooltip("测试实体生成范围")]
        public float SpawnRadius = 5f;

        [Header("玩家控制")]
        [Tooltip("WASD移动")]
        public bool UseWASD = true;

        [Tooltip("移动速度")]
        public float MoveSpeed = 5f;

        [Header("调试")]
        [Tooltip("显示所有实体半径")]
        public bool ShowAllRadius = true;

        [Tooltip("显示玩家半径")]
        public bool ShowPlayerRadius = true;

        [Tooltip("玩家半径")]
        public float PlayerRadius = 0.5f;

        [Tooltip("玩家质量")]
        public float PlayerMass = 2f;

        // 运行时数据
        private TopDownController2D _playerBody;
        private List<TopDownController2D> _testEntities = new();
        private Vector2 _moveInput;

        protected virtual void Start()
        {
            if (!Enabled) return;

            InitializeVolumeManager();
            CreatePlayer();
            CreateTestEntities();
        }

        protected virtual void Update()
        {
            if (!Enabled) return;

            HandleInput();
        }

        protected virtual void FixedUpdate()
        {
            if (!Enabled) return;

            ApplyPlayerMovement();
        }

        #region Initialization

        protected virtual void InitializeVolumeManager()
        {
            if (VolumeManager.Instance == null)
            {
                var go = new GameObject("VolumeManager");
                go.AddComponent<VolumeManager>();
            }
            VolumeManager.Instance.ShowAllGizmos = ShowAllRadius;
        }

        protected virtual void CreatePlayer()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = "Player_Test";
            go.transform.position = Vector3.zero;
            go.transform.localScale = Vector3.one * PlayerRadius * 2;
            go.GetComponent<Renderer>().material.color = Color.blue;

            _playerBody = go.AddComponent<TopDownController2D>();
            _playerBody.Radius = PlayerRadius;
            _playerBody.Mass = PlayerMass;
            _playerBody.MaxOverlapRatio = 0.2f;
            _playerBody.SpeedMultiplier = 1f;
            _playerBody.GizmosColor = Color.blue;

            VolumeManager.Instance.Register(_playerBody);
        }

        protected virtual void CreateTestEntities()
        {
            for (int i = 0; i < InitialEntityCount; i++)
            {
                CreateTestEntity();
            }
        }

        protected virtual TopDownController2D CreateTestEntity()
        {
            Vector2 spawnPos = Random.insideUnitCircle * SpawnRadius;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = $"TestEntity_{_testEntities.Count}";
            go.transform.position = spawnPos;

            float radius = Random.Range(0.3f, 0.8f);
            go.transform.localScale = Vector3.one * (radius * 2);

            // 设置随机颜色
            Color randomColor = Random.ColorHSV(0f, 1f, 0.5f, 1f, 0.5f, 1f);
            go.GetComponent<Renderer>().material.color = randomColor;

            var body = go.AddComponent<TopDownController2D>();
            body.Radius = radius;
            body.Mass = Random.Range(0.5f, 3f);
            body.MaxOverlapRatio = Random.Range(0.1f, 0.4f);
            body.SpeedMultiplier = Random.Range(0.5f, 1.5f);
            body.GizmosColor = randomColor;

            VolumeManager.Instance.Register(body);
            _testEntities.Add(body);

            return body;
        }

        #endregion

        #region Input

        protected virtual void HandleInput()
        {
            if (!UseWASD)
            {
                _moveInput = Vector2.zero;
                return;
            }

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            _moveInput = new Vector2(h, v).normalized;
        }

        protected virtual void ApplyPlayerMovement()
        {
            if (_playerBody == null) return;

            Vector2 move = _moveInput * (MoveSpeed * _playerBody.SpeedMultiplier);
            _playerBody.Position += move * Time.fixedDeltaTime;
            _playerBody.Velocity = move;
            _playerBody.transform.position = _playerBody.Position;
        }

        #endregion

        #region Debug Commands

        protected virtual void OnGUI()
        {
            if (!Enabled) 
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== 体积感系统测试 ===");
            GUILayout.Label($"实体数量: {_testEntities.Count + 1}");
            var str = VolumeManager.Instance ? "N/A" : "0";
            GUILayout.Label($"碰撞检测次数: {str}");
            GUILayout.Space(10);
            GUILayout.Label("操作:");
            GUILayout.Label("- WASD: 移动玩家(蓝)");
            GUILayout.Label("- 空格: 生成新实体");
            GUILayout.Label("- R: 重置位置");
            GUILayout.Label("- K: 测试击退");
            GUILayout.Space(10);
            GUILayout.Label($"玩家位置: {_playerBody?.Position ?? Vector2.zero}");
            GUILayout.Label($"玩家速度: {_playerBody?.Velocity ?? Vector2.zero}");
            GUILayout.EndArea();

            // 处理输入
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CreateTestEntity();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetPositions();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                TestKnockback();
            }
        }

        protected virtual void ResetPositions()
        {
            // 重置所有实体位置
            for (int i = 0; i < _testEntities.Count; i++)
            {
                Vector2 pos = Random.insideUnitCircle * SpawnRadius;
                _testEntities[i].Position = pos;
                _testEntities[i].Velocity = Vector2.zero;
                _testEntities[i].transform.position = pos;
            }
        }

        protected virtual void TestKnockback()
        {
            if (_playerBody == null || VolumeManager.Instance == null) 
                return;

            // 对所有测试实体施打击退力
            foreach (var entity in _testEntities)
            {
                Vector2 dir = (entity.Position - _playerBody.Position).normalized;
                VolumeManager.Instance.ApplyKnockback(entity, dir, 10f);
            }
        }

        #endregion

        protected virtual void OnDestroy()
        {
            // 清理
            foreach (var entity in _testEntities)
            {
                if (entity && VolumeManager.Instance)
                {
                    VolumeManager.Instance.Unregister(entity);
                }
            }
            _testEntities.Clear();

            if (_playerBody && VolumeManager.Instance)
            {
                VolumeManager.Instance.Unregister(_playerBody);
            }
        }
    }
}
