namespace MoreMountains
{
    /// <summary>
    /// 任何"商品卡"实现此接口。
    /// ShopController 只看到 IPurchasable，不知道它是球还是遗物。
    /// </summary>
    public interface IPurchasable
    {
        ItemKind Kind { get; }
        int ItemId { get; } // def id
        string DisplayName { get; }
        int Price { get; }
        bool Sold { get; }

        /// <summary>"我还想要 ¥¥¥" 按钮是否激活（默认 true）</summary>
        bool Enabled { get; }

        void MarkSold();
    }
}