using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// 遗物操作状态管理器 —— 单例,管理全局的"遗物操作状态"。
    ///
    /// 什么是"遗物操作状态":
    ///   玩家在 RelicInventoryItem 上按下左键后进入,
    ///   icon 跟随鼠标移动,所有遗物 item 的 highlight 显示,
    ///   鼠标悬停的 item 额外显示 highlightHovered,
    ///   sellZone 显示,其他 UI 按钮屏蔽指针事件。
    ///
    /// 退出条件(任一):
    ///   • 右键点击 → icon 归位,退出操作状态
    ///   • 左键点击在任意 RelicInventoryItem 上 → 执行操作后 icon 归位,退出
    ///   • 左键点击在 SellZone 上 → 出售操作后 icon 归位,退出
    ///   • 左键点击在空白区域 → icon 归位,退出
    ///
    /// 状态是独占的:进入时记录 source,退出前不能再次进入。
    /// 与 BallOperationStateManager 互斥:球进入操作状态后遗物不能再进入,反之亦然。
    /// </summary>
    public sealed class RelicOperationStateManager
    {
        public static RelicOperationStateManager Instance { get; } = new();

        public bool IsActive => _state != State.Idle;

        /// <summary>当前操作的源。</summary>
        public IRelicOperationTarget CurrentSource => _source;

        /// <summary>当前鼠标悬停的目标。</summary>
        public IItemOperationTarget CurrentHovered => _hovered;

        // 事件
        /// <summary>操作确认,回调传入悬停的目标(null=空白区域)。</summary>
        public event Action<IItemOperationTarget> OperationConfirmed;

        /// <summary>操作取消(右键或左键在空白)。</summary>
        public event Action OperationCancelled;

        /// <summary>sellZone 显隐变化。</summary>
        public event Action<bool> SellZoneVisibilityChanged;

        IRelicOperationTarget _source;
        IItemOperationTarget _hovered;
        bool _sourceConsumed;

        // ==================== Enter / Exit ====================

        public void TryEnter(IRelicOperationTarget source, RectTransform iconSource)
        {
            if (_state != State.Idle)
                return;

            // 与 Ball 操作状态互斥
            if (BallOperationStateManager.Instance.IsActive)
                return;

            _source = source;
            _sourceConsumed = false;
            _hovered = source;
            _state = State.Active;
            _stateActiveFrame = Time.frameCount;

            source.BeginFollowMouse(iconSource);
            SellZoneVisibilityChanged?.Invoke(true);
            BroadcastHighlightChanged(source, true);
            ActivateBlocker(true);
        }

        public void Update()
        {
            if (IsActive)
            {
                if (_stateActiveFrame == Time.frameCount)
                    return;

                // 左键:确认操作
                if (Input.GetMouseButtonDown(0))
                    TryHandleLeftClick();

                // 右键:取消操作
                if (Input.GetMouseButtonDown(1))
                    TryHandleRightClick();
            }

            // 每帧驱动 icon 跟随和悬停检测
            FrameUpdate();
        }

        /// <summary>每帧 Update 调。</summary>
        void FrameUpdate()
        {
            if (_state != State.Active)
                return;

            _source?.UpdateFollowMouse(Input.mousePosition);
            UpdateHovered();
        }

        /// <summary>处理左键点击(在操作状态中消费事件)。</summary>
        public bool TryHandleLeftClick()
        {
            if (_state != State.Active)
                return false;

            if (!_sourceConsumed)
            {
                _sourceConsumed = true;
                OperationConfirmed?.Invoke(_hovered);
            }

            Exit();
            return true;
        }

        /// <summary>处理右键点击(退出操作状态)。</summary>
        public bool TryHandleRightClick()
        {
            if (_state != State.Active)
                return false;

            OperationCancelled?.Invoke();
            Exit();
            return true;
        }

        public void ForceExit()
        {
            if (_state == State.Idle)
                return;

            OperationCancelled?.Invoke();
            Exit();
        }

        void Exit()
        {
            if (_state == State.Idle)
                return;

            BroadcastHighlightChanged(_source, false);

            _source?.EndFollowMouse();
            _source = null;
            _hovered = null;
            _sourceConsumed = false;
            _state = State.Idle;
            _stateActiveFrame = 0;

            SellZoneVisibilityChanged?.Invoke(false);
            ActivateBlocker(false);
        }

        // ==================== Hover detection ====================

        void UpdateHovered()
        {
            var prev = _hovered;
            var hoveredTarget = RaycastHovered();
            if (hoveredTarget != null)
            {
                _hovered = hoveredTarget;
            }

            if (hoveredTarget != null && !ReferenceEquals(prev, hoveredTarget))
            {
                prev?.SetHovered(false);
                _hovered?.SetHovered(true);
            }
            
            if (hoveredTarget == null)
            {
                _hovered?.SetHovered(false);
            }
            else
            {
                hoveredTarget.SetHovered(true);
            }
        }

        PointerEventData ptrData = new(UnityEngine.EventSystems.EventSystem.current);

        IItemOperationTarget RaycastHovered()
        {
            ptrData.position = Input.mousePosition;
            using var _ = new ListScope<RaycastResult>(out var results);
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(ptrData, results);
            for (int i = 0; i < results.Count; i++)
            {
                var go = results[i].gameObject;
                if (go == null)
                    continue;

                if (_blockerGO && go == _blockerGO)
                    continue;

                if (!go.CompareTag("OperationTarget")) 
                    continue;

                if (!go.TryGetComponent<ItemOperationTargetBridge>(out var bridge)) 
                    continue;

                if (bridge.Target != null) 
                    return bridge.Target;
                
                // if (!ReferenceEquals(bridge.Target, _source))
                // {
                //     return bridge.Target;
                // }
            }

            return null;
        }

        void BroadcastHighlightChanged(IRelicOperationTarget source, bool visible)
        {
            BroadcastHighlightEvent?.Invoke(source, visible);
        }

        internal static event Action<IRelicOperationTarget, bool> BroadcastHighlightEvent;

        // ==================== Blocker ====================

        GameObject _blockerGO;
        CanvasGroup _blockerCG;

        public void RegisterBlocker(GameObject go)
        {
            _blockerGO = go;
            _blockerCG = go?.GetComponent<CanvasGroup>();
        }

        void ActivateBlocker(bool active)
        {
            if (_blockerGO != null)
                _blockerGO.SetActive(active);
        }

        // ==================== State ====================

        enum State
        {
            Idle,
            Active
        }

        State _state = State.Idle;
        int _stateActiveFrame;
    }

    // ==================== IRelicOperationTarget ====================

    public interface IRelicOperationTarget : IItemOperationTarget
    {
        void ExecuteOperation(IItemOperationTarget hoveredTarget);
    }

    // ==================== Bridge ====================

    /// <summary>
    /// 挂到每个 RelicInventoryItem 根 GameObject 上的 MonoBehaviour。
    /// 职责:①持有 Target 引用(供 RaycastHovered 查找) ②响应 highlight 广播。
    /// </summary>
    public class RelicOperationTargetBridge : ItemOperationTargetBridge
    {
        void OnEnable()
        {
            RelicOperationStateManager.BroadcastHighlightEvent += OnHighlightChanged;
        }

        void OnDisable()
        {
            RelicOperationStateManager.BroadcastHighlightEvent -= OnHighlightChanged;
        }
    }

    // ==================== BlockerController ====================

    /// <summary>
    /// 挂到 Blocker GameObject 上,负责注册到 manager。
    /// Relic 与 Ball 共用同一个 sellZone 区域,但 blocker 是独立的(由各自的 manager 控制)。
    /// </summary>
    public class RelicBlockerController : MonoBehaviour
    {
        [SerializeField]
        bool blocksRaycasts = true;

        void Awake()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                RelicOperationStateManager.Instance.RegisterBlocker(gameObject);
            }
        }
    }

    // ==================== RelicOperationStateController ====================

    /// <summary>
    /// 遗物操作状态控制器 —— 集中处理 FrameUpdate、鼠标右键取消、左键点击确认。
    /// 挂到一个始终激活的 GameObject 上(推荐挂在 RelicOperationPanel 同级或 Canvas 根)。
    /// </summary>
    public class RelicOperationStateController : MonoBehaviour
    {
        void Update()
        {
            RelicOperationStateManager.Instance.Update();
        }
    }
}