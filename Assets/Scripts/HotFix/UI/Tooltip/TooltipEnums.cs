namespace MoreMountains
{
    /// <summary>
    /// Tooltip显示位置模式
    /// </summary>
    public enum TooltipPositionMode
    {
        /// <summary>
        /// 绝对固定位置，由TooltipSettings.fixedPosition指定
        /// </summary>
        Fixed,
        /// <summary>
        /// 跟随鼠标指针位置
        /// </summary>
        MousePosition,
        /// <summary>
        /// 相对于目标UI元素的锚点位置
        /// </summary>
        PivotAnchored
    }

    /// <summary>
    /// Tooltip锚点方向（用于PivotAnchored模式）
    /// </summary>
    public enum TooltipAnchorDirection
    {
        /// <summary>
        /// 目标元素下方
        /// </summary>
        Bottom,
        /// <summary>
        /// 目标元素上方
        /// </summary>
        Top,
        /// <summary>
        /// 目标元素左侧
        /// </summary>
        Left,
        /// <summary>
        /// 目标元素右侧
        /// </summary>
        Right,
        /// <summary>
        /// 目标元素左下方
        /// </summary>
        BottomLeft,
        /// <summary>
        /// 目标元素右下方
        /// </summary>
        BottomRight,
        /// <summary>
        /// 目标元素左上方
        /// </summary>
        TopLeft,
        /// <summary>
        /// 目标元素右上方
        /// </summary>
        TopRight,
        /// <summary>
        /// 居中显示
        /// </summary>
        Center
    }

    /// <summary>
    /// Tooltip显示时长模式
    /// </summary>
    public enum TooltipDurationMode
    {
        /// <summary>
        /// 永久显示，直到鼠标移出目标UI元素
        /// </summary>
        Permanent,
        /// <summary>
        /// 固定时长显示
        /// </summary>
        Timed
    }

    /// <summary>
    /// MetaTooltip关键字类型
    /// </summary>
    public enum MetaKeywordType
    {
        /// <summary>
        /// Buff/减益效果关键字
        /// </summary>
        Buff,
        /// <summary>
        /// 技能关键字
        /// </summary>
        Skill,
        /// <summary>
        /// 道具关键字
        /// </summary>
        Item,
        /// <summary>
        /// 状态关键字
        /// </summary>
        Status,
        /// <summary>
        /// 自定义类型
        /// </summary>
        Custom
    }
}
