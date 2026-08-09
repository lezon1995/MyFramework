namespace MoreMountains
{
    public enum BallMergeInvalidReason
    {
        None,
        SameKind,
        NotMaxLevel,
        GoldInsufficient,
        MergeRecipeMissing,
        HolderMissing,
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

        public BallItem TryMerge(BallItem a, BallItem b, out BallMergeInvalidReason reason)
        {
            reason = BallMergeInvalidReason.None;

            if (a == null || b == null)
            {
                reason = BallMergeInvalidReason.HolderMissing;
                logWarning("BallMergeService: null input");
                return null;
            }

            if (a.Type == b.Type)
            {
                reason = BallMergeInvalidReason.SameKind;
                return null;
            }

            var defA = a.Def;
            if (defA == null || defA.MergeResultDefId <= 0)
            {
                reason = BallMergeInvalidReason.MergeRecipeMissing;
                return null;
            }

            var defB = b.Def;
            int maxLevelA = defA.MaxLevel;
            int maxLevelB = defB != null ? defB.MaxLevel : int.MaxValue;

            if (a.Level < maxLevelA || b.Level < maxLevelB)
            {
                reason = BallMergeInvalidReason.NotMaxLevel;
                return null;
            }

            if (!_owner.Player.Wallet.CanPay(defA.MergeGoldCost))
            {
                reason = BallMergeInvalidReason.GoldInsufficient;
                return null;
            }

            if (!InventoryLocate.FindHolderOf(a, out var holderA))
            {
                reason = BallMergeInvalidReason.HolderMissing;
                return null;
            }

            if (!InventoryLocate.FindHolderOf(b, out var holderB))
            {
                reason = BallMergeInvalidReason.HolderMissing;
                return null;
            }

            // 扣金币（在拆球前扣，避免回滚麻烦）
            _owner.Player.loseGold(defA.MergeGoldCost, PayType.BALL_MERGE);

            // 拆 a
            if (!holderA.TryRemoveByItem(a))
            {
                logError("BallMergeService: failed to remove a");
                return null;
            }

            BallEvents.RaiseDestroyed(a);

            // 拆 b（若 a/b 同 holder，已经移除 a 不会再找到 b；用同样的 holder 再 RemoveByInstance 是 noop）
            if (!ReferenceEquals(holderA, holderB))
            {
                if (!holderB.TryRemoveByItem(b))
                {
                    logError("BallMergeService: failed to remove b");
                }
            }
            else
            {
                // 同 holder：a 已删，b 仍在；显式 RemoveByInstance
                holderA.TryRemoveByItem(b);
            }

            BallEvents.RaiseDestroyed(b);

            // 创建融合球，Lv.1
            var merged = BallItem.New(ballManager.getDef(BallType.Normal), level: 1);
            if (!holderA.TryInsert(merged))
                logError($"BallMergeService: failed to insert merged ball into {holderA.Name}");

            BallEvents.RaiseCreated(merged);
            BallEvents.RaiseMerged(a, b, merged);
            return merged;
        }
    }
}