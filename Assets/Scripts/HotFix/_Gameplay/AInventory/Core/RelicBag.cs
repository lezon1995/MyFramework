namespace MoreMountains
{
    /// <summary>
    /// 遗物背包 —— 容量受 InventorySystemConfig.RelicBagCapacity 控制。
    /// 当前项目里 ARelic 是 abstract partial class，本类用 dynamic 边界以避免强耦合到它的具体子类层级。
    /// 实际放入 RelicBag 的可以是任何继承 ARelic 的对象（需要保证它实现了 IInventoryItem）。
    /// </summary>
    public sealed class RelicBag : InventoryBag<RelicItem>
    {
        public RelicBag(int capacity, int maxCapacity) : base(capacity, maxCapacity, "RelicBag")
        {
        }

        protected override ItemKind GetBagKind() => ItemKind.Relic;
    }
}