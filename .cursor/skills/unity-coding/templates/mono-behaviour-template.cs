using UnityEngine;

namespace MyGame.ModuleName
{
    /// <summary>
    /// [一句话描述本组件职责]
    /// </summary>
    [DisallowMultipleComponent]
    public class XxxComponent : MonoBehaviour
    {
        #region Serialized Fields

        [Header("引用")]
        [SerializeField] private XxxController controller;
        [SerializeField] private Transform visualRoot;

        [Header("数值")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private int maxCount = 10;

        #endregion

        #region Cached References

        private Camera _mainCamera;
        private Collider _collider;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // 只做自身引用缓存与初始化
            _collider = GetComponent<Collider>();
        }

        private void OnEnable()
        {
            // 订阅事件（与 OnDisable 成对）
        }

        private void OnDisable()
        {
            // 退订事件
        }

        private void Start()
        {
            // 依赖外部状态的初始化
        }

        private void Update()
        {
            // 只做必要逻辑，避免 GetComponent / new / 字符串拼接
        }

        private void LateUpdate()
        {
        }

        private void OnDestroy()
        {
            // 释放非托管资源
        }

        #endregion

        #region Public Methods

        public void Setup(XxxController owner)
        {
            controller = owner;
        }

        #endregion

        #region Private Methods

        private void HandleXxx()
        {
        }

        #endregion
    }
}
