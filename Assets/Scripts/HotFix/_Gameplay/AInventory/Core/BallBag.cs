namespace MoreMountains
{
    /// <summary>
    /// 球背包 —— 容量受 InventorySystemConfig.BallBagCapacity 控制。
    /// BallInstance 由球管理系统创建，本类只关心增删 / 容量 / 事件。
    /// </summary>
    public sealed class BallBag : InventoryBag<BallItem>
    {
        public BallBag(int capacity, int maxCapacity) : base(capacity, maxCapacity, "BallBag")
        {
        }

        protected override ItemKind GetBagKind() => ItemKind.Ball;
    }
}