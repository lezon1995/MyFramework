/*using UnityEngine;

namespace MoreMountains
{
    /// <summary>购买球 — 校验 + 扣金币 + 入包。</summary>
    public sealed class BuyBallAction : InstantShopAction, IArgs<BallOffer, APlayer>
    {
        BallOffer _offer;
        APlayer _player;

        public void onCreate(BallOffer offer, APlayer p)
        {
            _offer = offer;
            _player = p;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _offer = null;
            _player = null;
        }

        protected override void Execute()
        {
            if (_offer == null || _offer.Sold) 
                return;

            // 1) 背包满
            if (!_player.Inventory.CanAddBall())
            {
                Debug.LogWarning("球背包已满，请先出售");
                return;
            }

            // 2) 金币
            if (!_player.Wallet.CanPay(_offer.Price))
            {
                Debug.LogWarning("金币不足");
                return;
            }

            // 3) 入包
            _player.loseGold(_offer.Price, PayType.BALL_BUY);
            ShopEvents.RaiseGoldSpent(_offer.Price, "shop_buy_ball");
            _player.BallManagement.Shop.PurchaseAndStore(_offer.Def);

            // 4) 标记 + 事件
            _offer.MarkSold();
            ShopEvents.RaiseOfferSold(_offer);
        }
    }

    /// <summary>购买遗物。</summary>
    public sealed class BuyRelicAction : InstantShopAction, IArgs<RelicOffer, APlayer>
    {
        RelicOffer _offer;
        APlayer _player;

        public void onCreate(RelicOffer offer, APlayer p)
        {
            _offer = offer;
            _player = p;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _offer = null;
            _player = null;
        }

        protected override void Execute()
        {
            if (_offer == null || _offer.Sold) 
                return;

            if (!_player.Inventory.CanAddRelic())
            {
                Debug.LogWarning("遗物背包已满，请先出售");
                return;
            }

            if (!_player.Wallet.CanPay(_offer.Price))
            {
                Debug.LogWarning("金币不足");
                return;
            }

            _player.loseGold(_offer.Price, PayType.RELIC_BUY);
            ShopEvents.RaiseGoldSpent(_offer.Price, "shop_buy_relic");

            // 通过 RelicService 包装成 RelicItem 入包
            RelicItem item = RelicService.CreateItem(_offer.Def);
            if (!_player.Inventory.AddRelic(item))
            {
                // 入包失败，回滚金币
                // _player.gainGold(_offer.Price, "shop_buy_relic_rollback");
                return;
            }

            _offer.MarkSold();
            ShopEvents.RaiseOfferSold(_offer);
        }
    }
}*/