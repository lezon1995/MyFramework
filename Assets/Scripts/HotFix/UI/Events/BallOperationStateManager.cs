using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// 球操作状态管理器 —— 单例,管理全局的"球操作状态"。
    ///
    /// 什么是"球操作状态":
    ///   玩家在 BallSlotItem / BallInventoryItem 上按下左键后进入,
    ///   icon 跟随鼠标移动,所有球 item 的 highlight 显示,
    ///   鼠标悬停的 item 额外显示 highlightHovered,
    ///   sellZone 显示,其他 UI 按钮屏蔽指针事件。
    ///
    /// 退出条件(任一):
    ///   • 右键点击 → icon 归位,退出操作状态
    ///   • 左键点击在任意 BallSlotItem/BallInventoryItem 上 → 执行操作后 icon 归位,退出
    ///   • 左键点击在 SellZone 上 → 出售操作后 icon 归位,退出
    ///   • 左键点击在空白区域 → icon 归位,退出
    ///
    /// 状态是独占的:进入时记录 source,退出前不能再次进入。
    /// </summary>
    public sealed class BallOperationStateManager
    {
        public static BallOperationStateManager Instance { get; } = new();

        public bool IsActive => _state != State.Idle;

        /// <summary>当前操作的源。</summary>
        public IBallOperationTarget CurrentSource => _source;

        /// <summary>当前鼠标悬停的目标。</summary>
        public IItemOperationTarget CurrentHovered => _hovered;

        // 事件
        /// <summary>操作确认,回调传入悬停的目标(null=空白区域)。</summary>
        public event Action<IItemOperationTarget> OperationConfirmed;

        /// <summary>操作取消(右键或左键在空白)。</summary>
        public event Action OperationCancelled;

        /// <summary>sellZone 显隐变化。</summary>
        public event Action<bool, int> SellZoneVisibilityChanged;

        IBallOperationTarget _source;
        IItemOperationTarget _hovered;
        bool _sourceConsumed;

        // ==================== Enter / Exit ====================

        public void TryEnter(IBallOperationTarget source, RectTransform iconSource)
        {
            if (_state != State.Idle)
                return;

            _source = source;
            _sourceConsumed = false;
            _hovered = source;
            _state = State.Active;
            _stateActiveFrame = Time.frameCount;

            source.BeginFollowMouse(iconSource);
            int sellPrice = source switch
            {
                BallInventoryItem item => item.Slot.Item.SellPrice,
                BallSlotItem slot => slot.Slot.Item.SellPrice,
                _ => 0
            };

            SellZoneVisibilityChanged?.Invoke(true, sellPrice);
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

            SellZoneVisibilityChanged?.Invoke(false, 0);
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

                if (_blockerGO != null && go == _blockerGO)
                    continue;

                if (!go.CompareTag("OperationTarget"))
                    continue;

                if (!go.TryGetComponent<ItemOperationTargetBridge>(out var bridge))
                    continue;

                if (bridge.Target != null /* && !ReferenceEquals(bridge.Target, _source)*/)
                {
                    return bridge.Target;
                }
            }

            return null;
        }

        void BroadcastHighlightChanged(IBallOperationTarget source, bool visible)
        {
            BroadcastHighlightEvent?.Invoke(source, visible);
        }

        internal static event Action<IBallOperationTarget, bool> BroadcastHighlightEvent;

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

    // ==================== IBallOperationTarget ====================

    public interface IItemOperationTarget
    {
        void BeginFollowMouse(RectTransform iconSource);
        void UpdateFollowMouse(Vector2 screenMousePos);
        void EndFollowMouse();
        void SetHovered(bool isHovered);
        void SetHighlightVisible(bool visible);
        void SetEventBlocking(bool blocking);
    }

    public interface IBallOperationTarget : IItemOperationTarget
    {
        void ExecuteOperation(IItemOperationTarget hoveredTarget);
    }

    // ==================== Bridge ====================

    /// <summary>
    /// 挂到每个 BallSlotItem / BallInventoryItem 根 GameObject 上的 MonoBehaviour。
    /// 职责:①持有 Target 引用(供 RaycastHovered 查找) ②响应 highlight 广播。
    /// 不负责 Update/输入,那些集中在 BallOperationStateController 中。
    /// </summary>
    public class BallOperationTargetBridge : ItemOperationTargetBridge
    {
        void OnEnable()
        {
            BallOperationStateManager.BroadcastHighlightEvent += OnHighlightChanged;
        }

        void OnDisable()
        {
            BallOperationStateManager.BroadcastHighlightEvent -= OnHighlightChanged;
        }
    }

    public class ItemOperationTargetBridge : MonoBehaviour
    {
        public IItemOperationTarget Target;

        protected void OnHighlightChanged(IItemOperationTarget source, bool visible)
        {
            Target?.SetHighlightVisible(visible);
            if (visible && source == Target)
            {
                Target?.SetHovered(true);
            }
        }
    }

    // ==================== BlockerController ====================

    /// <summary>
    /// 挂到 Blocker GameObject 上,负责注册到 manager。
    /// </summary>
    public class BlockerController : MonoBehaviour
    {
        [SerializeField]
        bool blocksRaycasts = true;

        void Awake()
        {
            var cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                BallOperationStateManager.Instance.RegisterBlocker(gameObject);
            }
        }
    }

    // ==================== BallOperationStateController ====================

    /// <summary>
    /// 球操作状态控制器 —— 集中处理 FrameUpdate、鼠标右键取消、左键点击确认。
    /// 挂到一个始终激活的 GameObject 上(推荐挂在 BallOperationPanel 同级或 Canvas 根)。
    /// </summary>
    public class BallOperationStateController : MonoBehaviour
    {
        void Update()
        {
        }
    }
}