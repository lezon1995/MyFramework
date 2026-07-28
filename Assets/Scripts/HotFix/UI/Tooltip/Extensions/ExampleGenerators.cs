using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 自定义Tooltip内容生成器示例
    /// 展示如何动态生成Tooltip内容
    /// </summary>
    public class ExampleContentGenerator : TooltipContentGenerator
    {
        [Serializable]
        public class ItemData
        {
            public string itemName;
            public string itemDescription;
            public int itemLevel;
            public Sprite itemIcon;
            public string[] tags;
        }

        [SerializeField]
        private ItemData itemData;

        public TooltipContent GenerateContent(TooltipTrigger trigger)
        {
            if (itemData == null)
            {
                return new TooltipContent("未配置物品数据", "请联系开发者");
            }

            string title = itemData.itemName;
            string description = itemData.itemDescription;

            if (itemData.itemLevel > 0)
            {
                description = $"[等级 {itemData.itemLevel}]\n{description}";
            }

            if (itemData.tags != null && itemData.tags.Length > 0)
            {
                description += "\n\n";
                foreach (var tag in itemData.tags)
                {
                    description += $"• [{tag}]\n";
                }
            }

            return new TooltipContent(title, description, itemData.itemIcon);
        }
    }

    /// <summary>
    /// Buff专用的Tooltip内容生成器
    /// </summary>
    public class BuffTooltipContentGenerator : TooltipContentGenerator
    {
        [Serializable]
        public class BuffData
        {
            public string buffName;
            public string buffDescription;
            public float duration;
            public int damagePerTick;
            public float tickInterval;
            public Sprite icon;
        }

        [SerializeField]
        private BuffData buffData;

        public TooltipContent GenerateContent(TooltipTrigger trigger)
        {
            if (buffData == null)
            {
                return new TooltipContent("未知Buff", "Buff数据未配置");
            }

            string title = $"<color=yellow>{buffData.buffName}</color>";
            string description = buffData.buffDescription;

            if (buffData.duration > 0)
            {
                description += $"\n\n持续时间: {buffData.duration}秒";
            }

            if (buffData.damagePerTick > 0)
            {
                description += $"\n每跳伤害: {buffData.damagePerTick}";
                if (buffData.tickInterval > 0)
                {
                    description += $"\n跳伤间隔: {buffData.tickInterval}秒";
                }
            }

            return new TooltipContent(title, description, buffData.icon);
        }
    }

    /// <summary>
    /// 技能Tooltip内容生成器
    /// </summary>
    public class SkillTooltipContentGenerator : TooltipContentGenerator
    {
        [Serializable]
        public class SkillData
        {
            public string skillName;
            public string description;
            public int manaCost;
            public float cooldown;
            public int damage;
            public Sprite icon;
        }

        [SerializeField]
        private SkillData skillData;

        public TooltipContent GenerateContent(TooltipTrigger trigger)
        {
            if (skillData == null)
            {
                return new TooltipContent("未知技能", "技能数据未配置");
            }

            string title = skillData.skillName;

            if (skillData.manaCost > 0)
            {
                title += $" <color=blue>({skillData.manaCost} MP)</color>";
            }

            string description = skillData.description;

            if (skillData.damage > 0)
            {
                description += $"\n\n伤害值: {skillData.damage}";
            }

            if (skillData.cooldown > 0)
            {
                description += $"\n冷却时间: {skillData.cooldown}秒";
            }

            return new TooltipContent(title, description, skillData.icon);
        }
    }

    /// <summary>
    /// 动态内容Tooltip生成器
    /// 可以根据运行时数据动态生成Tooltip内容
    /// </summary>
    public class DynamicTooltipContentGenerator : TooltipContentGenerator
    {
        public enum ContentSource
        {
            Static,
            FromComponent,
            FromDataTable,
            FromNetwork
        }

        [Header("Content Source")]
        [SerializeField]
        private ContentSource source = ContentSource.Static;

        [Header("Static Content")]
        [SerializeField]
        private string staticTitle;

        [SerializeField]
        [TextArea(2, 5)]
        private string staticDescription;

        [Header("Component Reference")]
        [SerializeField]
        private Component dataSourceComponent;

        [SerializeField]
        private string dataMethodName = "GetTooltipContent";

        public TooltipContent GenerateContent(TooltipTrigger trigger)
        {
            switch (source)
            {
                case ContentSource.Static:
                    return GenerateStaticContent();

                case ContentSource.FromComponent:
                    return GenerateFromComponent();

                case ContentSource.FromDataTable:
                    return GenerateFromDataTable();

                case ContentSource.FromNetwork:
                    return GenerateFromNetwork();

                default:
                    return GenerateStaticContent();
            }
        }

        private TooltipContent GenerateStaticContent()
        {
            return new TooltipContent(staticTitle, staticDescription);
        }

        private TooltipContent GenerateFromComponent()
        {
            if (dataSourceComponent == null || string.IsNullOrEmpty(dataMethodName))
            {
                return GenerateStaticContent();
            }

            try
            {
                var method = dataSourceComponent.GetType().GetMethod(dataMethodName);
                if (method != null)
                {
                    var result = method.Invoke(dataSourceComponent, new object[] { });
                    if (result is TooltipContent)
                    {
                        return (TooltipContent)result;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"DynamicTooltipContentGenerator: Failed to invoke method {dataMethodName}: {e.Message}");
            }

            return GenerateStaticContent();
        }

        private TooltipContent GenerateFromDataTable()
        {
            // 示例：从数据表获取内容
            // 你可以根据trigger关联的数据ID从配置表获取数据
            return GenerateStaticContent();
        }

        private TooltipContent GenerateFromNetwork()
        {
            // 示例：从网络获取内容
            // 这里可以发起网络请求获取最新数据
            return GenerateStaticContent();
        }
    }

    /// <summary>
    /// 扩展的Tooltip请求类，支持更多高级功能
    /// </summary>
    public class ExtendedTooltipRequest : TooltipRequest
    {
        /// <summary>
        /// 是否允许点击Tooltip关闭
        /// </summary>
        public bool allowClickToClose = false;

        /// <summary>
        /// 是否显示关闭按钮
        /// </summary>
        public bool showCloseButton = false;

        /// <summary>
        /// Tooltip的最大宽度
        /// </summary>
        public float maxWidth = 400f;

        /// <summary>
        /// Tooltip的最小宽度
        /// </summary>
        public float minWidth = 200f;

        /// <summary>
        /// 自定义背景颜色
        /// </summary>
        public Color? customBackgroundColor;

        /// <summary>
        /// 自定义边框颜色
        /// </summary>
        public Color? customBorderColor;

        /// <summary>
        /// 优先级（用于多Tooltip场景）
        /// </summary>
        public int priority = 0;

        /// <summary>
        /// 生命周期回调
        /// </summary>
        public Action<TooltipBox> onTooltipCreated;

        public ExtendedTooltipRequest()
        {
            base.positionMode = TooltipPositionMode.PivotAnchored;
            base.anchorDirection = TooltipAnchorDirection.Top;
            base.durationMode = TooltipDurationMode.Permanent;
            base.displayDuration = 5f;
        }

        public ExtendedTooltipRequest(TooltipRequest request) : base()
        {
            this.content = request.content;
            this.positionMode = request.positionMode;
            this.anchorDirection = request.anchorDirection;
            this.durationMode = request.durationMode;
            this.displayDuration = request.displayDuration;
            this.fixedPosition = request.fixedPosition;
            this.mouseOffset = request.mouseOffset;
            this.anchorOffset = request.anchorOffset;
            this.targetRect = request.targetRect;
            this.trigger = request.trigger;
            this.onShow = request.onShow;
            this.onHide = request.onHide;
            this.isMetaEnabled = request.isMetaEnabled;
        }
    }

    /// <summary>
    /// Tooltip优先级管理器
    /// 用于处理多个Tooltip同时显示的情况
    /// </summary>
    public class TooltipPriorityManager
    {
        private static TooltipPriorityManager _instance;
        public static TooltipPriorityManager Instance => _instance ?? (_instance = new TooltipPriorityManager());

        private int _currentHighestPriority = int.MinValue;
        private TooltipTrigger _currentTrigger;

        /// <summary>
        /// 请求显示高优先级Tooltip
        /// </summary>
        public void RequestShow(TooltipTrigger trigger, int priority)
        {
            if (priority > _currentHighestPriority)
            {
                _currentHighestPriority = priority;
                _currentTrigger = trigger;
            }
        }

        /// <summary>
        /// 释放Tooltip
        /// </summary>
        public void Release(TooltipTrigger trigger)
        {
            if (_currentTrigger == trigger)
            {
                _currentTrigger = null;
                _currentHighestPriority = int.MinValue;
            }
        }

        /// <summary>
        /// 检查是否可以显示Tooltip
        /// </summary>
        public bool CanShow(TooltipTrigger trigger, int priority)
        {
            if (_currentTrigger == null)
            {
                return true;
            }

            if (_currentTrigger == trigger)
            {
                return true;
            }

            return priority >= _currentHighestPriority;
        }
    }

    /// <summary>
    /// Tooltip缓存池
    /// 用于复用Tooltip对象，减少GC
    /// </summary>
    public class TooltipObjectPool
    {
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly System.Collections.Generic.Queue<TooltipBox> _pool = new();

        public TooltipObjectPool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _parent = parent;

            Prewarm(3);
        }

        public TooltipBox Get()
        {
            if (_pool.Count > 0)
            {
                var tooltip = _pool.Dequeue();
                tooltip.gameObject.SetActive(true);
                return tooltip;
            }

            var newTooltip = UnityEngine.Object.Instantiate(_prefab, _parent).GetComponent<TooltipBox>();
            return newTooltip;
        }

        public void Return(TooltipBox tooltip)
        {
            tooltip.gameObject.SetActive(false);
            _pool.Enqueue(tooltip);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var tooltip = UnityEngine.Object.Instantiate(_prefab, _parent).GetComponent<TooltipBox>();
                tooltip.gameObject.SetActive(false);
                _pool.Enqueue(tooltip);
            }
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var tooltip = _pool.Dequeue();
                UnityEngine.Object.Destroy(tooltip.gameObject);
            }
        }
    }
}
