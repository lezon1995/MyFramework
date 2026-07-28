using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip事件数据
    /// </summary>
    public class TooltipEventData : BaseEventData
    {
        public TooltipContent content;
        public TooltipTrigger trigger;
        public RectTransform targetRect;
        public Vector2 screenPosition;

        public TooltipEventData(UnityEngine.EventSystems.EventSystem eventSystem) : base(eventSystem)
        {
        }
    }

    /// <summary>
    /// Tooltip事件类型
    /// </summary>
    public enum TooltipEventType
    {
        Show,
        Hide,
        Update,
        Refresh
    }

    /// <summary>
    /// Tooltip事件接口
    /// </summary>
    public interface ITooltipEventHandler : IEventSystemHandler
    {
        void OnTooltipShow(TooltipEventData eventData);
        void OnTooltipHide(TooltipEventData eventData);
        void OnTooltipUpdate(TooltipEventData eventData);
    }

    /// <summary>
    /// Tooltip事件管理器
    /// 提供全局Tooltip事件的分发功能
    /// </summary>
    public class TooltipEventSystem
    {
        private static TooltipEventSystem _instance;
        public static TooltipEventSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TooltipEventSystem();
                }
                return _instance;
            }
        }

        public event Action<TooltipEventData> OnShow;
        public event Action<TooltipEventData> OnHide;
        public event Action<TooltipEventData> OnUpdate;

        private TooltipEventData _eventData;

        private TooltipEventSystem()
        {
        }

        /// <summary>
        /// 触发显示事件
        /// </summary>
        public void TriggerShow(TooltipContent content, TooltipTrigger trigger, RectTransform targetRect)
        {
            if (_eventData == null)
            {
                _eventData = new TooltipEventData(UnityEngine.EventSystems.EventSystem.current);
            }

            _eventData.content = content;
            _eventData.trigger = trigger;
            _eventData.targetRect = targetRect;
            _eventData.screenPosition = targetRect != null ? targetRect.position : Vector2.zero;

            OnShow?.Invoke(_eventData);
        }

        /// <summary>
        /// 触发隐藏事件
        /// </summary>
        public void TriggerHide(TooltipContent content, TooltipTrigger trigger, RectTransform targetRect)
        {
            if (_eventData == null)
            {
                _eventData = new TooltipEventData(UnityEngine.EventSystems.EventSystem.current);
            }

            _eventData.content = content;
            _eventData.trigger = trigger;
            _eventData.targetRect = targetRect;
            _eventData.screenPosition = targetRect != null ? targetRect.position : Vector2.zero;

            OnHide?.Invoke(_eventData);
        }

        /// <summary>
        /// 触发更新事件
        /// </summary>
        public void TriggerUpdate(TooltipContent content, TooltipTrigger trigger, RectTransform targetRect)
        {
            if (_eventData == null)
            {
                _eventData = new TooltipEventData(UnityEngine.EventSystems.EventSystem.current);
            }

            _eventData.content = content;
            _eventData.trigger = trigger;
            _eventData.targetRect = targetRect;
            _eventData.screenPosition = targetRect != null ? targetRect.position : Vector2.zero;

            OnUpdate?.Invoke(_eventData);
        }

        /// <summary>
        /// 清除所有事件订阅
        /// </summary>
        public void ClearAll()
        {
            OnShow = null;
            OnHide = null;
            OnUpdate = null;
        }
    }

    /// <summary>
    /// Tooltip事件监听器组件
    /// 用于监听全局Tooltip事件
    /// </summary>
    public class TooltipEventListener : MonoBehaviour, ITooltipEventHandler
    {
        [Header("Event Settings")]
        [SerializeField]
        private bool _listenShow = true;

        [SerializeField]
        private bool _listenHide = true;

        [SerializeField]
        private bool _listenUpdate;

        [Header("Event Targets")]
        [SerializeField]
        private UnityEngine.Events.UnityEvent<TooltipContent> onShowEvent;

        [SerializeField]
        private UnityEngine.Events.UnityEvent<TooltipContent> onHideEvent;

        [SerializeField]
        private UnityEngine.Events.UnityEvent<TooltipContent> onUpdateEvent;

        private void Start()
        {
            if (_listenShow)
            {
                TooltipEventSystem.Instance.OnShow += HandleShow;
            }

            if (_listenHide)
            {
                TooltipEventSystem.Instance.OnHide += HandleHide;
            }

            if (_listenUpdate)
            {
                TooltipEventSystem.Instance.OnUpdate += HandleUpdate;
            }
        }

        private void OnDestroy()
        {
            TooltipEventSystem.Instance.OnShow -= HandleShow;
            TooltipEventSystem.Instance.OnHide -= HandleHide;
            TooltipEventSystem.Instance.OnUpdate -= HandleUpdate;
        }

        private void HandleShow(TooltipEventData eventData)
        {
            onShowEvent?.Invoke(eventData.content);
        }

        private void HandleHide(TooltipEventData eventData)
        {
            onHideEvent?.Invoke(eventData.content);
        }

        private void HandleUpdate(TooltipEventData eventData)
        {
            onUpdateEvent?.Invoke(eventData.content);
        }

        public void OnTooltipShow(TooltipEventData eventData)
        {
            onShowEvent?.Invoke(eventData.content);
        }

        public void OnTooltipHide(TooltipEventData eventData)
        {
            onHideEvent?.Invoke(eventData.content);
        }

        public void OnTooltipUpdate(TooltipEventData eventData)
        {
            onUpdateEvent?.Invoke(eventData.content);
        }
    }
}
