using System.Collections.Generic;

namespace MoreMountains
{
    public enum BallUpgradeInvalidReason
    {
        None,
        NotEnoughMaterial,
        DifferentType,
        DifferentLevel,
        MaxLevelReached,
        GoldInsufficient,
        MaterialNotFound,
        HolderMissing,
    }

    public enum BallItemUpgradeResult
    {
        None,
        Vanished,
        Downgraded,
    }

    /// <summary>
    /// 球升级服务 —— X 个同种同等级 → 1 个同种 Level+1。
    /// 由 BallManagementSystem 持有；UI / Command 调它。
    /// 不引用任何 UI/Command，是系统的"原子操作"。
    /// </summary>
    public sealed class BallUpgradeService
    {
        BallManagementSystem _owner;

        public BallUpgradeService(BallManagementSystem owner)
        {
            _owner = owner;
        }

        public bool CanUpgradeWith(BallItem src, BallItem dst)
        {
            if (dst == null || src == null)
                return false;

            if (dst == src)
                return false;

            if (dst.Type != src.Type)
                return false;

            if (dst.isMaxLevel() || src.isMaxLevel())
                return false;

            return true;
        }

        public bool TryUpgradeWith(BallInventorySlot src, BallInventorySlot dst, out BallItemUpgradeResult srcResult)
        {
            srcResult = BallItemUpgradeResult.None;
            if (!CanUpgradeWith(src.Item, dst.Item))
                return false;

            var toLevel = dst.Item.Level + src.Item.Level;
            var maxLevel = dst.Item.getMaxLevel();
            if (toLevel <= maxLevel)
            {
                dst.Item.Level = toLevel;
                src.Item.Level = 0;
                srcResult = BallItemUpgradeResult.Vanished;
            }
            else
            {
                var targetLevel = toLevel - maxLevel;
                dst.Item.Level = maxLevel;
                src.Item.Level = targetLevel;
                srcResult = BallItemUpgradeResult.Downgraded;
            }

            return true;
        }


        /// <summary>
        /// 尝试升级。
        /// representative：被选中的"代表球"，升级产物落在它原本所在的 holder 里。
        /// materials：参与合成的材料球（不含 representative，N - 1 个）。
        /// 如果调用方不区分代表球与材料，可以把所有 N 个球全放 materials，由服务自动选出第一个作为代表。
        /// </summary>
        public BallItem TryUpgrade(List<BallItem> candidates, out BallUpgradeInvalidReason reason)
        {
            reason = BallUpgradeInvalidReason.None;
            if (candidates == null || candidates.Count == 0)
            {
                reason = BallUpgradeInvalidReason.MaterialNotFound;
                return Fail("null candidates");
            }

            // representative = 第一个非空元素
            BallItem representative = null;
            for (int i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] != null)
                {
                    representative = candidates[i];
                    break;
                }
            }

            if (representative == null)
            {
                reason = BallUpgradeInvalidReason.MaterialNotFound;
                return Fail("all null candidates");
            }

            var def = representative.Def;
            if (def == null)
            {
                reason = BallUpgradeInvalidReason.MaterialNotFound;
                return Fail("missing def");
            }

            int targetLevel = representative.Level + 1;
            if (targetLevel > def.MaxLevel)
            {
                reason = BallUpgradeInvalidReason.MaxLevelReached;
                return Fail(representative, "max_level");
            }

            int combineCount = def.UpgradeCombineCount > 0 ? def.UpgradeCombineCount : 2;
            if (candidates.Count < combineCount)
            {
                reason = BallUpgradeInvalidReason.NotEnoughMaterial;
                return Fail(representative, "not_enough");
            }

            // 同种类同等级校验（全部）
            for (int i = 0; i < candidates.Count; i++)
            {
                var b = candidates[i];
                if (b == null)
                {
                    reason = BallUpgradeInvalidReason.MaterialNotFound;
                    return Fail(representative, "null_material");
                }

                if (b.Type != representative.Type)
                {
                    reason = BallUpgradeInvalidReason.DifferentType;
                    return Fail(representative, "diff_kind");
                }

                if (b.Level != representative.Level)
                {
                    reason = BallUpgradeInvalidReason.DifferentLevel;
                    return Fail(representative, "diff_level");
                }
            }

            // 校验金币
            if (def.UpgradeGoldCost > 0 && !_owner.Player.Wallet.CanPay(def.UpgradeGoldCost))
            {
                reason = BallUpgradeInvalidReason.GoldInsufficient;
                return Fail(representative, "gold_insufficient");
            }

            // 代表球所在的 holder —— 升级产物回到这里
            if (!InventoryLocate.FindHolderOf(representative, out var repHolder))
            {
                reason = BallUpgradeInvalidReason.HolderMissing;
                return Fail(representative, "holder_missing");
            }

            // 扣金币
            if (def.UpgradeGoldCost > 0)
                _owner.Player.loseGold(def.UpgradeGoldCost, PayType.BALL_UPGRADE);

            // 从各自的 holder 移除 N 个球
            for (int i = 0; i < candidates.Count; i++)
            {
                var b = candidates[i];
                // ball 可能与 representative 在同一 holder 或不同 holder；先精确查询。
                if (InventoryLocate.FindHolderOf(b, out var holder))
                {
                    holder.TryRemoveByItem(b);
                    BallEvents.RaiseDestroyed(b);
                }
            }

            // 创建升级产物
            var upgraded = BallItem.New(def, targetLevel);

            // 插入：优先精确放回 representative 原本所在的精确位置（仅 BallSlotGroup 支持）；
            // 其它实现走 TryInsert 默认行为。
            if (repHolder is BallSlotGroup sg)
            {
                if (sg.FindIndex(representative, out var slotIndex))
                {
                    sg.ReplaceAt(slotIndex, upgraded);
                    BallEvents.RaiseCreated(upgraded);
                    BallEvents.RaiseUpgraded(representative, upgraded);
                    return upgraded;
                }
            }

            // 否则默认插入到代表球的 holder
            if (!repHolder.TryInsert(upgraded))
                logError($"BallUpgradeService: failed to insert upgraded ball into {repHolder.Name}");

            BallEvents.RaiseCreated(upgraded);
            BallEvents.RaiseUpgraded(representative, upgraded);
            return upgraded;
        }

        static BallItem Fail(BallItem b, string why)
        {
            logWarning($"BallUpgradeService: invalid ({why}) on def {b?.Type} lv {b?.Level}");
            return null;
        }

        static BallItem Fail(string why)
        {
            logWarning($"BallUpgradeService: invalid ({why})");
            return null;
        }
    }
}