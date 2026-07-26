namespace MoreMountains
{
    /// <summary>出售背包里的球。</summary>
    public sealed class SellBallAction : InstantShopAction
    {
        BallInstance _ball;
        public void with(BallInstance b) { _ball = b; }
        public override void resetProperty() { base.resetProperty(); _ball = null; }

        protected override void Execute()
        {
            if (_ball == null) return;
            BallManagementSystem.Instance?.Shop.SellToShop(_ball);
        }
    }

    /// <summary>出售背包里的遗物。</summary>
    public sealed class SellRelicAction : InstantShopAction
    {
        RelicItem _item;
        public void with(RelicItem i) { _item = i; }
        public override void resetProperty() { base.resetProperty(); _item = null; }

        protected override void Execute()
        {
            if (_item == null) return;
            if (InventorySystem.Instance != null) InventorySystem.Instance.RemoveRelic(_item);
            PlayerWallet.Instance?.Earn(_item.SellPrice, "relic_sell");
            ShopEvents.RaiseGoldEarned(_item.SellPrice, "relic_sell");
            ShopEvents.RaiseSoldFromBag(_item);
        }
    }

    /// <summary>重新随机当前面板。</summary>
    public sealed class RerollShopBoardAction : InstantShopAction
    {
        protected override void Execute()
        {
            ShopSystem.Instance?.Controller.OnPlayerClickReroll();
        }
    }

    /// <summary>玩家点 "下一步"。</summary>
    public sealed class GoNextShopPhaseAction : InstantShopAction
    {
        protected override void Execute()
        {
            ShopSystem.Instance?.Controller.OnPlayerClickNext();
        }
    }
}
