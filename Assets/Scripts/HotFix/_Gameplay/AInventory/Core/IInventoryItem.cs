using System;

namespace MoreMountains
{
    /// <summary>
    /// 背包物品大分类。仅用于：背包拒绝入包提示、商店满格提示。
    /// </summary>
    [Flags]
    public enum ItemKind : byte
    {
        None  = 0,
        Ball  = 1 << 0,
        Relic = 1 << 1,
        BallStatMod = 1 << 2,
        PlayerStatMod = 1 << 3,
    }

    /// <summary>
    /// 任何"可以塞进背包"的东西都实现这个接口。
    /// 背包系统只通过它识别物品，不关心物品内部业务。
    /// </summary>
    public interface IInventoryItem
    {
        /// <summary>配置表 ID（球 defId / 遗物 id）</summary>
        int ItemId { get; }

        /// <summary>多语言键 / 显示名</summary>
        string DisplayName { get; }

        /// <summary>售出时的回收价（半价，已由实现自行计算）</summary>
        int SellPrice { get; }

        /// <summary>球 or 遗物</summary>
        ItemKind Kind { get; }
    }
}
