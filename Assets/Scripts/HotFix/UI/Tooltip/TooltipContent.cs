using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Tooltip系统命名空间
    /// </summary>
    public static class TooltipConsts
    {
        /// <summary>
        /// 默认Tooltip最大宽度
        /// </summary>
        public const float DefaultMaxWidth = 400f;

        /// <summary>
        /// 默认Tooltip最小宽度
        /// </summary>
        public const float DefaultMinWidth = 200f;

        /// <summary>
        /// 默认Tooltip高度
        /// </summary>
        public const float DefaultHeight = 100f;

        /// <summary>
        /// 默认屏幕边距
        /// </summary>
        public const float DefaultScreenPadding = 10f;

        /// <summary>
        /// 默认显示延迟
        /// </summary>
        public const float DefaultShowDelay = 0.5f;

        /// <summary>
        /// 默认显示时长
        /// </summary>
        public const float DefaultDisplayDuration = 5f;

        /// <summary>
        /// 默认淡入时间
        /// </summary>
        public const float DefaultFadeInDuration = 0.15f;

        /// <summary>
        /// 默认淡出时间
        /// </summary>
        public const float DefaultFadeOutDuration = 0.1f;

        /// <summary>
        /// 默认Meta关键字正则表达式
        /// </summary>
        public const string DefaultKeywordPattern = @"\[([^\]]+)\]";
    }

    /// <summary>
    /// Tooltip样式预设
    /// </summary>
    [Serializable]
    public class TooltipStylePreset
    {
        [Tooltip("样式名称")]
        public string name;

        [Tooltip("背景颜色")]
        public Color backgroundColor = new(0.1f, 0.1f, 0.1f, 0.95f);

        [Tooltip("边框颜色")]
        public Color borderColor = new(0.3f, 0.3f, 0.3f, 0.5f);

        [Tooltip("标题颜色")]
        public Color titleColor = Color.white;

        [Tooltip("描述颜色")]
        public Color descriptionColor = new(0.9f, 0.9f, 0.9f);

        [Tooltip("是否使用描边")]
        public bool useOutline = true;

        [Tooltip("描边颜色")]
        public Color outlineColor = new(0, 0, 0, 0.5f);

        [Tooltip("描边宽度")]
        public Vector2 outlineDistance = new(2, 2);

        [Tooltip("最大宽度")]
        public float maxWidth = TooltipConsts.DefaultMaxWidth;

        [Tooltip("最小宽度")]
        public float minWidth = TooltipConsts.DefaultMinWidth;

        [Tooltip("标题字体大小")]
        public float titleFontSize = 18f;

        [Tooltip("描述字体大小")]
        public float descriptionFontSize = 14f;

        [Tooltip("内边距")]
        public Vector4 padding = new(10, 10, 10, 10);

        [Tooltip("元素间距")]
        public float elementSpacing = 8f;

        public static TooltipStylePreset CreateDefault()
        {
            return new()
            {
                name = "Default",
                backgroundColor = new(0.1f, 0.1f, 0.1f, 0.95f),
                borderColor = new(0.3f, 0.3f, 0.3f, 0.5f),
                titleColor = Color.white,
                descriptionColor = new(0.9f, 0.9f, 0.9f),
                useOutline = true,
                outlineColor = new(0, 0, 0, 0.5f),
                outlineDistance = new(2, 2),
                maxWidth = TooltipConsts.DefaultMaxWidth,
                minWidth = TooltipConsts.DefaultMinWidth,
                titleFontSize = 18f,
                descriptionFontSize = 14f,
                padding = new(10, 10, 10, 10),
                elementSpacing = 8f
            };
        }

        public static TooltipStylePreset CreateRarity(int rarity)
        {
            var preset = CreateDefault();

            switch (rarity)
            {
                case 1: // 白色 - 普通
                    preset.name = "Common";
                    preset.borderColor = new(0.6f, 0.6f, 0.6f);
                    break;
                case 2: // 绿色 - 优秀
                    preset.name = "Uncommon";
                    preset.borderColor = new(0.2f, 0.8f, 0.2f);
                    break;
                case 3: // 蓝色 - 稀有
                    preset.name = "Rare";
                    preset.borderColor = new(0.2f, 0.5f, 1f);
                    break;
                case 4: // 紫色 - 史诗
                    preset.name = "Epic";
                    preset.borderColor = new(0.6f, 0.2f, 0.8f);
                    break;
                case 5: // 橙色 - 传说
                    preset.name = "Legendary";
                    preset.borderColor = new(1f, 0.5f, 0.1f);
                    break;
                default:
                    preset.name = "Unknown";
                    break;
            }

            return preset;
        }
    }

    /// <summary>
    /// Tooltip分组信息
    /// 用于分组管理Tooltip
    /// </summary>
    public class TooltipGroup
    {
        public string groupName;
        public List<TooltipTrigger> members = new();
        public int priority;
        public bool isExclusive;

        public TooltipGroup(string name, int priority = 0, bool exclusive = false)
        {
            groupName = name;
            this.priority = priority;
            isExclusive = exclusive;
        }

        public void AddMember(TooltipTrigger trigger)
        {
            if (trigger != null && !members.Contains(trigger))
            {
                members.Add(trigger);
            }
        }

        public void RemoveMember(TooltipTrigger trigger)
        {
            if (trigger != null)
            {
                members.Remove(trigger);
            }
        }

        public void HideAll()
        {
            foreach (var member in members)
            {
                if (member != null && member.isTooltipShown)
                {
                    member.HideTooltip();
                }
            }
        }
    }

    /// <summary>
    /// Tooltip分组管理器
    /// </summary>
    public class TooltipGroupManager
    {
        static TooltipGroupManager _instance;
        public static TooltipGroupManager Instance => _instance ??= new TooltipGroupManager();

        Dictionary<string, TooltipGroup> _groups = new();

        TooltipGroupManager()
        {
        }

        public TooltipGroup CreateGroup(string name, int priority = 0, bool exclusive = false)
        {
            if (_groups.TryGetValue(name, out var existing))
            {
                return existing;
            }

            var group = new TooltipGroup(name, priority, exclusive);
            _groups[name] = group;
            return group;
        }

        public TooltipGroup GetGroup(string name)
        {
            _groups.TryGetValue(name, out var group);
            return group;
        }

        public void RegisterToGroup(string groupName, TooltipTrigger trigger)
        {
            var group = CreateGroup(groupName);
            group.AddMember(trigger);
        }

        public void UnregisterFromGroup(string groupName, TooltipTrigger trigger)
        {
            if (_groups.TryGetValue(groupName, out var group))
            {
                group.RemoveMember(trigger);
            }
        }

        public void HideGroup(string groupName)
        {
            if (_groups.TryGetValue(groupName, out var group))
            {
                group.HideAll();
            }
        }

        public void HideAllGroups()
        {
            foreach (var kvp in _groups)
            {
                kvp.Value.HideAll();
            }
        }

        public void ShowOnlyInGroup(string groupName)
        {
            HideAllGroups();
            ShowGroup(groupName);
        }

        public void ShowGroup(string groupName)
        {
            if (_groups.TryGetValue(groupName, out var group))
            {
                foreach (var member in group.members)
                {
                    if (member != null)
                    {
                        member.ShowTooltip();
                    }
                }
            }
        }

        public void ClearGroup(string groupName)
        {
            if (_groups.ContainsKey(groupName))
            {
                _groups[groupName].members.Clear();
                _groups.Remove(groupName);
            }
        }

        public void ClearAll()
        {
            _groups.Clear();
        }
    }
}