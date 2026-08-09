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
    public class BallItem : ClassObject, IInventoryItem, IEquatable<BallItem>
    {
        public int ItemId => Def.BallDefId;
        public BallType Type => Def.Type;
        public ItemKind Kind => ItemKind.Ball;

        /// <summary>对关联的 BallDef 缓存（可选，避免反复查表）</summary>
        public BallDef Def;
        public int Level; // 1..MaxLevel
        public Guid Uid; // 升级 / 融合后重新生成

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

        public override void resetProperty()
        {
            base.resetProperty();
            Def = null;
            Level = 1;
            Uid = Guid.Empty;
        }

        /// <summary>工厂方法。系统内部创建都用它。</summary>
        public static BallItem New(BallDef def, int level)
        {
            var item = CLASS<BallItem>();
            item.Def = def;
            item.Level = Math.Max(1, level);
            item.Uid = Guid.NewGuid();
            return item;
        }

        public static void Release(ref BallItem item)
        {
            UN_CLASS(ref item);
        }

        public bool Equals(BallItem other) => other != null && Uid.Equals(other.Uid);
        public override bool Equals(object obj) => obj is BallItem other && Equals(other);
        public override int GetHashCode() => Uid.GetHashCode();
        public override string ToString() => $"{DisplayName} [{Uid:N}]";
    }
}