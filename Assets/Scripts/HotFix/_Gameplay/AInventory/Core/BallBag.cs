namespace MoreMountains
{
    /// <summary>
    /// 球背包 —— 容量受 InventorySystemConfig.BallBagCapacity 控制。
    /// BallItem 由球管理系统创建，本类只关心增删 / 容量 / 事件。
    /// 内部固定数量 BallInventorySlot,Slot.Item == null 表示空格子。
    /// </summary>
    public sealed class BallBag : InventoryBag<BallItem, BallInventorySlot>
    {
        public BallBag(APlayer p, int capacity, int maxCapacity) : base(p, capacity, maxCapacity, "BallBag")
        {
        }

        protected override BallInventorySlot CreateSlot(int index) => new(index);
        protected override ItemKind GetBagKind() => ItemKind.Ball;
    }
}