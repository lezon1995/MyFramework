using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip系统全局配置
    /// </summary>
    [Serializable]
    public class TooltipSettings
    {
        /// <summary>
        /// 全局是否启用Tooltip
        /// </summary>
        [Tooltip("全局是否启用Tooltip")]
        public bool enableTooltip = true;

        /// <summary>
        /// 默认显示延迟时间（秒），0表示立即显示
        /// </summary>
        [Tooltip("鼠标悬停多久后显示Tooltip（秒），0表示立即显示")]
        public float defaultShowDelay = 0.5f;

        /// <summary>
        /// 默认显示时长（秒），仅在DurationMode为Timed时有效
        /// </summary>
        [Tooltip("Tooltip显示时长（秒），仅在DurationMode为Timed时有效")]
        public float defaultDisplayDuration = 5f;

        /// <summary>
        /// 默认显示位置模式
        /// </summary>
        [Tooltip("默认显示位置模式")]
        public TooltipPositionMode defaultPositionMode = TooltipPositionMode.PivotAnchored;

        /// <summary>
        /// 默认锚点方向（用于PivotAnchored模式）
        /// </summary>
        [Tooltip("默认锚点方向（用于PivotAnchored模式）")]
        public TooltipAnchorDirection defaultAnchorDirection = TooltipAnchorDirection.Top;

        /// <summary>
        /// 绝对固定位置（用于Fixed模式）
        /// </summary>
        [Tooltip("绝对固定位置（用于Fixed模式）")]
        public Vector2 fixedPosition = new(100f, -100f);

        /// <summary>
        /// 鼠标位置偏移量（用于MousePosition模式）
        /// </summary>
        [Tooltip("鼠标位置偏移量（用于MousePosition模式）")]
        public Vector2 mouseOffset = new(20f, -20f);

        /// <summary>
        /// 锚点偏移量（用于PivotAnchored模式）
        /// </summary>
        [Tooltip("锚点偏移量（用于PivotAnchored模式）")]
        public Vector2 anchorOffset = new(0f, -10f);

        /// <summary>
        /// TooltipBox与屏幕边缘的最小间距
        /// </summary>
        [Tooltip("TooltipBox与屏幕边缘的最小间距")]
        public float screenEdgePadding = 10f;

        /// <summary>
        /// MetaTooltipBox与TooltipBox的间距
        /// </summary>
        [Tooltip("MetaTooltipBox与TooltipBox的间距")]
        public float metaTooltipSpacing = 8f;

        /// <summary>
        /// 是否自动调整位置以避免超出屏幕
        /// </summary>
        [Tooltip("当Tooltip超出屏幕时是否自动调整位置")]
        public bool autoAdjustPosition = true;

        /// <summary>
        /// 默认FadeIn时间
        /// </summary>
        [Tooltip("FadeIn时间（秒）")]
        public float fadeInDuration = 0.15f;

        /// <summary>
        /// 默认FadeOut时间
        /// </summary>
        [Tooltip("FadeOut时间（秒）")]
        public float fadeOutDuration = 0.1f;

        /// <summary>
        /// Meta关键字的正则表达式匹配模式
        /// 例如：\[([^\]]+)\] 会匹配 [灼烧] 这样的格式
        /// </summary>
        [Tooltip("Meta关键字的正则表达式匹配模式")]
        public string keywordPattern = @"\[([^\]]+)\]";

        /// <summary>
        /// 是否启用MetaTooltip
        /// </summary>
        [Tooltip("是否启用MetaTooltip")]
        public bool enableMetaTooltip = true;

        /// <summary>
        /// 预设的Meta关键字类型映射
        /// </summary>
        [Tooltip("预设的Meta关键字类型映射")]
        public MetaKeywordPreset[] keywordPresets = {
            new() { keyword = "灼烧", type = MetaKeywordType.Buff, displayName = "灼烧" },
            new() { keyword = "冰冻", type = MetaKeywordType.Buff, displayName = "冰冻" },
            new() { keyword = "中毒", type = MetaKeywordType.Buff, displayName = "中毒" },
            new() { keyword = "眩晕", type = MetaKeywordType.Buff, displayName = "眩晕" },
            new() { keyword = "沉默", type = MetaKeywordType.Buff, displayName = "沉默" },
        };
    }

    /// <summary>
    /// Meta关键字预设配置
    /// </summary>
    [Serializable]
    public class MetaKeywordPreset
    {
        [Tooltip("关键字内容")]
        public string keyword;

        [Tooltip("关键字类型")]
        public MetaKeywordType type;

        [Tooltip("显示名称")]
        public string displayName;
    }
}