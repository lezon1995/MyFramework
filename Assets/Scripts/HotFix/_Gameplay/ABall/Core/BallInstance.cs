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
        public BallType Type;
        public int Level; // 1..MaxLevel
        public readonly Guid Uid; // 升级 / 融合后重新生成

        /// <summary>对关联的 BallDef 缓存（可选，避免反复查表）</summary>
        public BallDef Def;

        public ItemKind Kind => ItemKind.Ball;
        public string DisplayName => Def ? $"{Def.DisplayName} Lv.{Level}" : $"Ball#{Type} Lv.{Level}";

        public int SellPrice
        {
            get
            {
                if (Def == null)
                    return 0;

                var rate = Mathf.Clamp(50, 0, 100);
                return Math.Max(1, Def.BasePrice * rate / 100);
            }
        }

        int IInventoryItem.ItemId => Def.BallDefId;

        public BallInstance(BallType type, int level)
        {
            Type = type;
            Level = Math.Max(1, level);
            Uid = Guid.NewGuid();
        }

        public BallInstance(BallDef def, int level)
        {
            Def = def;
            Type = def.Type;
            Level = Math.Max(1, level);
            Uid = Guid.NewGuid();
        }

        /// <summary>工厂方法。系统内部创建都用它。</summary>
        public static BallInstance CreateNew(BallType type, int level)
        {
            return new(type, level);
        }

        public static BallInstance CreateNew(BallDef def, int level)
        {
            return new(def, level);
        }

        public bool Equals(BallInstance other) => other != null && Uid.Equals(other.Uid);
        public override bool Equals(object obj) => obj is BallInstance other && Equals(other);
        public override int GetHashCode() => Uid.GetHashCode();
        public override string ToString() => $"{DisplayName} [{Uid:N}]";
    }
}