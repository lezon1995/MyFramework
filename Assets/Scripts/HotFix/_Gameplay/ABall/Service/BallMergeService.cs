namespace MoreMountains
{
    public enum BallMergeResult
    {
        None,
        SameKind,
        NotMaxLevel,
        GoldInsufficient,
        MergeRecipeMissing,
        HolderMissing,
        Success,
    }

    /// <summary>
    /// 球融合服务 —— 两个不同种类、都满级 + 金币够 → 1 个 Lv.1 融合球。
    /// </summary>
    public sealed class BallMergeService
    {
        BallManagementSystem _owner;

        public BallMergeService(BallManagementSystem owner)
        {
            _owner = owner;
        }

        public bool CanMerge(BallItem src, BallItem dst, out BallMergeResult result)
        {
            result = BallMergeResult.None;
            if (src == null || dst == null)
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (src.Type == dst.Type)
            {
                result = BallMergeResult.SameKind;
                return false;
            }

            var srcDef = src.Def;
            var dstDef = dst.Def;
            int maxLevelA = srcDef.maxLevel;
            int maxLevelB = dstDef.maxLevel;

            if (src.Level < maxLevelA || dst.Level < maxLevelB)
            {
                result = BallMergeResult.NotMaxLevel;
                return false;
            }

            var mergeGoldCost = 1;
            if (!_owner.Player.Wallet.CanPay(mergeGoldCost))
            {
                result = BallMergeResult.GoldInsufficient;
                return false;
            }

            if (!InventoryLocate.FindHolderOf(src, out var srcHolder))
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (!InventoryLocate.FindHolderOf(dst, out var dstHolder))
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (!ballManager.containsMergedDef(src.Type, dst.Type))
            {
                result = BallMergeResult.MergeRecipeMissing;
                return false;
            }

            result = BallMergeResult.Success;
            return true;
        }

        public bool TryMerge(BallItem src, BallItem dst, out BallMergeResult result, out BallItem mergedBallItem)
        {
            result = BallMergeResult.None;
            mergedBallItem = null;

            if (src == null || dst == null)
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (src.Type == dst.Type)
            {
                result = BallMergeResult.SameKind;
                return false;
            }

            var srcDef = src.Def;
            var dstDef = dst.Def;
            int maxLevelA = srcDef.maxLevel;
            int maxLevelB = dstDef.maxLevel;

            if (src.Level < maxLevelA || dst.Level < maxLevelB)
            {
                result = BallMergeResult.NotMaxLevel;
                return false;
            }

            var mergeGoldCost = 1;
            if (!_owner.Player.Wallet.CanPay(mergeGoldCost))
            {
                result = BallMergeResult.GoldInsufficient;
                return false;
            }

            if (!InventoryLocate.FindHolderOf(src, out var srcHolder))
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (!InventoryLocate.FindHolderOf(dst, out var dstHolder))
            {
                result = BallMergeResult.HolderMissing;
                return false;
            }

            if (!ballManager.tryGetMergedDef(src.Type, dst.Type, out var mergedDef))
            {
                result = BallMergeResult.MergeRecipeMissing;
                return false;
            }

            // 扣金币（在拆球前扣，避免回滚麻烦）
            _owner.Player.loseGold(mergeGoldCost, PayType.BALL_MERGE);

            // 拆 a
            if (!srcHolder.TryRemoveByItem(src))
            {
                logError("BallMergeService: failed to remove a");
                return false;
            }

            BallEvents.RaiseDestroyed(src);

            // 拆 b（若 a/b 同 holder，已经移除 a 不会再找到 b；用同样的 holder 再 RemoveByInstance 是 noop）
            int dstIndex;
            if (ReferenceEquals(srcHolder, dstHolder))
            {
                // 同 holder：a 已删，b 仍在；显式 RemoveByInstance
                srcHolder.FindIndex(dst, out dstIndex);
                srcHolder.TryRemoveByItem(dst);
            }
            else
            {
                dstHolder.FindIndex(dst, out dstIndex);
                if (!dstHolder.TryRemoveByItem(dst))
                {
                    logError("BallMergeService: failed to remove b");
                }
            }

            BallEvents.RaiseDestroyed(dst);

            // 创建融合球，Lv.1
            result = BallMergeResult.Success;
            mergedBallItem = BallItem.New(mergedDef, level: 1);
            if (!srcHolder.TryInsertAt(mergedBallItem, dstIndex))
                logError($"BallMergeService: failed to insert merged ball into {srcHolder.Name}");

            BallEvents.RaiseCreated(mergedBallItem);
            BallEvents.RaiseMerged(src, dst, mergedBallItem);
            return true;
        }
    }
}