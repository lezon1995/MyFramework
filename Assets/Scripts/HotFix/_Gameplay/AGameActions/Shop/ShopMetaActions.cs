namespace MoreMountains
{
    /// <summary>出售背包里的球。</summary>
    public sealed class SellBallAction : InstantShopAction, IArgs<BallItem, APlayer>
    {
        APlayer _player;
        BallItem _ball;

        public void onCreate(BallItem b, APlayer p)
        {
            _ball = b;
            _player = p;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _ball = null;
            _player = null;
        }

        protected override void Execute()
        {
            if (_ball == null)
                return;

            _player.BallManagement.Shop.SellToShop(_ball);
        }
    }

    /// <summary>出售背包里的遗物。</summary>
    public sealed class SellRelicAction : InstantShopAction, IArgs<RelicItem, APlayer>
    {
        RelicItem _item;
        APlayer _player;

        public void onCreate(RelicItem b, APlayer p)
        {
            _item = b;
            _player = p;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _item = null;
            _player = null;
        }

        protected override void Execute()
        {
            if (_item == null) 
                return;

            _player.Inventory.RemoveRelic(_item);
            _player.gainGold(_item.SellPrice, EarnType.SELL_RELIC);
            ShopEvents.RaiseGoldEarned(_item.SellPrice, "relic_sell");
            ShopEvents.RaiseSoldFromBag(_item);
        }
    }

    /// <summary>重新随机当前面板。</summary>
    public sealed class RerollShopBoardAction : InstantShopAction, IArgs<APlayer>
    {
        APlayer _player;

        public void onCreate(APlayer p1)
        {
            _player = p1;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _player = null;
        }

        protected override void Execute()
        {
            _player.Shop.Controller.OnPlayerClickReroll();
        }
    }

    /// <summary>玩家点 "下一步"。</summary>
    public sealed class GoNextShopPhaseAction : InstantShopAction, IArgs<APlayer>
    {
        APlayer _player;

        public void onCreate(APlayer p1)
        {
            _player = p1;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _player = null;
        }
        
        protected override void Execute()
        {
            _player.Shop.Controller.OnPlayerClickNext();
        }
    }
}