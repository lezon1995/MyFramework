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
    public class BallItem : ClassObject, IInventoryItem
    {
        public int ItemId => Def.BallDefId;
        public BallType Type => Def.Type;
        public ItemKind Kind => ItemKind.Ball;

        /// <summary>对关联的 BallDef 缓存（可选，避免反复查表）</summary>
        public BallDef Def;

        public int Level; // 1..MaxLevel
        public int levelIndex => Level - 1; // 1..MaxLevel
        public Guid Uid; // 升级 / 融合后重新生成

        public string DisplayName => Def ? $"{Def.DisplayName.GetLocalizedString()} Lv.{Level}" : $"Ball#{Type} Lv.{Level}";

        public int BuyPrice => Def.BasePrice * Level;

        public int SellPrice
        {
            get
            {
                if (Def == null)
                    return 0;

                var rate = Mathf.Clamp(50, 0, 100);
                return Math.Max(1, BuyPrice * rate / 100);
            }
        }

        public bool isMaxLevel() => Level >= getMaxLevel();
        public int getMaxLevel() => 4;

        public ItemRarity getLevelToRarity()
        {
            var rarity = Mathf.Clamp(levelIndex, 0, 3);
            return (ItemRarity)rarity;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            Def = null;
            Level = 1;
            Uid = Guid.Empty;
        }

        /// <summary>工厂方法。系统内部创建都用它。</summary>
        public static BallItem New(BallDef def)
        {
            var item = CLASS<BallItem>();
            item.Def = def;
            item.Level = (int)def.Rarity + 1;
            item.Uid = Guid.NewGuid();
            return item;
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

        public static void Release(BallItem item)
        {
            UN_CLASS(item);
        }

        public override string ToString() => $"{DisplayName} [{Uid:N}]";
    }
}