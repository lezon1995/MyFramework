using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 跨容器查找 —— 给"球在哪个 holder"一个统一的查询入口。
    /// 注册当前对玩家生效的所有 holder（BallBag、RelicBag、BallSlotGroup、挂起奖励队列等）。
    /// </summary>
    public static class InventoryLocate
    {
        static readonly List<IInventoryHolder> Holders = new();

        public static void Register(IInventoryHolder h)
        {
            if (h != null && !Holders.Contains(h))
                Holders.Add(h);
        }

        public static void Unregister(IInventoryHolder h)
        {
            Holders.Remove(h);
        }

        public static void Clear() => Holders.Clear();

        /// <summary>
        /// 找到持有该物品的 holder；找不到返回 null。
        /// 只读查询，绝不修改任何 holder。
        /// </summary>
        public static bool FindHolderOf<T>(T item, out IInventoryHolder<T> holder) where T : IInventoryItem
        {
            holder = null;
            if (item == null)
                return false;

            foreach (var h in Holders)
            {
                if (h is IInventoryHolder<T> a)
                {
                    if (a.FindIndex(item, out _))
                    {
                        holder = a;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}