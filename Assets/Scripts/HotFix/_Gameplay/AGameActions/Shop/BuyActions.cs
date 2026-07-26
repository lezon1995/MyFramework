using UnityEngine;

namespace MoreMountains
{
    /// <summary>购买球 — 校验 + 扣金币 + 入包。</summary>
    public sealed class BuyBallAction : InstantShopAction
    {
        BallOffer _offer;

        public void with(BallOffer offer)
        {
            _offer = offer;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _offer = null;
        }

        protected override void Execute()
        {
            if (_offer == null || _offer.Sold) 
                return;

            // 1) 背包满
            var inv = InventorySystem.Instance;
            if (inv == null || !inv.CanAddBall())
            {
                Debug.LogWarning("球背包已满，请先出售");
                return;
            }

            // 2) 金币
            if (!PlayerWallet.Instance.CanPay(_offer.Price))
            {
                Debug.LogWarning("金币不足");
                return;
            }

            // 3) 入包
            PlayerWallet.Instance.Pay(_offer.Price, "shop_buy_ball");
            ShopEvents.RaiseGoldSpent(_offer.Price, "shop_buy_ball");
            BallManagementSystem.Instance.Shop.PurchaseAndStore(_offer.Def.BallDefId);

            // 4) 标记 + 事件
            _offer.MarkSold();
            ShopEvents.RaiseOfferSold(_offer);
        }
    }

    /// <summary>购买遗物。</summary>
    public sealed class BuyRelicAction : InstantShopAction
    {
        RelicOffer _offer;

        public void with(RelicOffer offer)
        {
            _offer = offer;
        }

        public override void resetProperty()
        {
            base.resetProperty();
            _offer = null;
        }

        protected override void Execute()
        {
            if (_offer == null || _offer.Sold) return;

            var inv = InventorySystem.Instance;
            if (inv == null || !inv.CanAddRelic())
            {
                Debug.LogWarning("遗物背包已满，请先出售");
                return;
            }

            if (!PlayerWallet.Instance.CanPay(_offer.Price))
            {
                Debug.LogWarning("金币不足");
                return;
            }

            PlayerWallet.Instance.Pay(_offer.Price, "shop_buy_relic");
            ShopEvents.RaiseGoldSpent(_offer.Price, "shop_buy_relic");

            // 通过 RelicService 包装成 RelicItem 入包
            RelicItem item = RelicService.CreateItem(_offer.Def);
            if (!inv.AddRelic(item))
            {
                // 入包失败，回滚金币
                PlayerWallet.Instance.Earn(_offer.Price, "shop_buy_relic_rollback");
                return;
            }

            _offer.MarkSold();
            ShopEvents.RaiseOfferSold(_offer);
        }
    }
}