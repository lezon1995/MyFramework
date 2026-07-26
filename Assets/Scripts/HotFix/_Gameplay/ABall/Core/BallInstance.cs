using System;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 球的运行时实例。实现 IInventoryItem，所以可以直接落背包。
    /// 只在
    ///   BallBag
    ///   BallSlot
    /// 两者之一存在（不可同时持有同一颗）。
    /// </summary>
    [Serializable]
    public sealed class BallInstance : IInventoryItem, IEquatable<BallInstance>
    {
        public readonly int DefId;
        public readonly int Level; // 1..MaxLevel
        public readonly Guid Uid; // 升级 / 融合后重新生成

        /// <summary>对关联的 BallDef 缓存（可选，避免反复查表）</summary>
        public BallDef Def => BallDefLibrary.Instance != null ? BallDefLibrary.Instance.Get(DefId) : null;

        public ItemKind Kind => ItemKind.Ball;
        public string DisplayName => Def != null ? $"{Def.DisplayName} Lv.{Level}" : $"Ball#{DefId} Lv.{Level}";

        public int SellPrice
        {
            get
            {
                if (Def == null) 
                    return 0;
                
                int rate = BallSystemConfig.Instance ? BallSystemConfig.Instance.SellRefundRate : 50;
                rate = Mathf.Clamp(rate, 0, 100);
                return Math.Max(1, Def.BasePrice * rate / 100);
            }
        }

        int IInventoryItem.ItemId => DefId;

        public BallInstance(int defId, int level)
        {
            DefId = defId;
            Level = Math.Max(1, level);
            Uid = Guid.NewGuid();
        }

        /// <summary>工厂方法。系统内部创建都用它。</summary>
        public static BallInstance CreateNew(int defId, int level) => new(defId, level);

        public bool Equals(BallInstance other) => other != null && Uid.Equals(other.Uid);
        public override bool Equals(object obj) => obj is BallInstance other && Equals(other);
        public override int GetHashCode() => Uid.GetHashCode();
        public override string ToString() => $"{DisplayName} [{Uid:N}]";
    }
}