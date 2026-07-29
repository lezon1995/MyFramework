using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 商店 binder —— 把 ShopController 的 offers 渲染到 ShopView。
    /// 需求：球商品与遗物商品在同一面板（ShopItems 节点）下生成，按 offer 顺序排列。
    /// 不直接处理金币扣 / 加，业务由 ShopController 通过 ShopController.OnPlayerBuyOffer 走。
    /// </summary>
    public sealed class ShopBinder
    {
        ShopView _view;
        ShopController _ctrl;
        APlayer _player;

        // event handler cache —— 让 -= 能成功匹配
        Action<ShopBoardKind> _onBoardRefreshed;
        Action<IPurchasable> _onOfferSold;
        Action<IInventoryItem> _onSoldFromBag;
        Action<int, string> _onGoldEarned;
        Action<int, string> _onGoldSpent;

        List<IPurchasable> _orderedOffers = new();

        public ShopBinder(ShopView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _onBoardRefreshed = _ => Rebuild();
            _onOfferSold = _ => Rebuild();
            _onSoldFromBag = _ => Rebuild();
            _onGoldEarned = (g, _) => RefreshCoin();
            _onGoldSpent = (g, _) => RefreshCoin();
        }

        public event Action<IPurchasable> OfferBuyClicked;
        public event Action RerollClicked;
        public event Action BuyExpClicked;
        public event Action<IInventoryItem> SellDropDetected;

        public ShopController Controller => _ctrl;

        public void Attach(APlayer player, ShopController ctrl)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _ctrl = ctrl ?? throw new ArgumentNullException(nameof(ctrl));

            _view.SetTitle("SHOP");

            _view.BtnReroll.setUGUIButtonClick(OnRerollClicked);
            _view.BtnBuyExp.setUGUIButtonClick(OnBuyExpClicked);

            ShopEvents.OnBoardOpened += _onBoardRefreshed;
            ShopEvents.OnBoardRerolled += _onBoardRefreshed;
            ShopEvents.OnOfferSold += _onOfferSold;
            ShopEvents.OnSoldFromBag += _onSoldFromBag;
            ShopEvents.OnGoldEarned += _onGoldEarned;
            ShopEvents.OnGoldSpent += _onGoldSpent;

            RefreshCoin();
            Rebuild();
        }

        public void Detach()
        {
            if (_ctrl == null) 
                return;

            ShopEvents.OnBoardOpened -= _onBoardRefreshed;
            ShopEvents.OnBoardRerolled -= _onBoardRefreshed;
            ShopEvents.OnOfferSold -= _onOfferSold;
            ShopEvents.OnSoldFromBag -= _onSoldFromBag;
            ShopEvents.OnGoldEarned -= _onGoldEarned;
            ShopEvents.OnGoldSpent -= _onGoldSpent;
            _player = null;
            _ctrl = null;
        }

        public void Rebuild()
        {
            if (_ctrl == null) 
                return;

            RefreshCoin();

            _orderedOffers.Clear();

            _view.BuildBallOffers(_ctrl.BallOffers, (item, offer) =>
            {
                _orderedOffers.Add(offer);
                item.SetName(offer.DisplayName ?? "—");
                item.SetPrice(offer.Price);
                if (offer.Def)
                    item.SetDesc(offer.Def.GetType().Name);
                if (offer.Def.Icon)
                    item.SetIcon(offer.Def.Icon);
                item.SetHovered(false);
                item.SetNewTag(offer.Enabled);
                item.Btn.setInteractable(offer.Enabled);
                item.Btn.setUGUIButtonClick(() => OnOfferClicked(offer));
            });

            _view.BuildRelicOffers(_ctrl.RelicOffers, (item, offer) =>
            {
                _orderedOffers.Add(offer);
                item.SetName(offer.DisplayName ?? "—");
                item.SetPrice(offer.Price);
                if (offer.Def)
                    item.SetDesc(offer.Def.GetType().Name);
                if (offer.Def.Icon)
                    item.SetIcon(offer.Def.Icon);
                item.SetHovered(false);
                item.SetNewTag(offer.Enabled);
                item.Btn.setInteractable(offer.Enabled);
                item.Btn.setUGUIButtonClick(() => OnOfferClicked(offer));
            });
        }

        void OnOfferClicked(IPurchasable offer)
        {
            if (offer == null) 
                return;

            OfferBuyClicked?.Invoke(offer);
        }

        void OnRerollClicked() => RerollClicked?.Invoke();
        void OnBuyExpClicked() => BuyExpClicked?.Invoke();

        public void RefreshCoin()
        {
            int balance = _player?.Wallet?.Balance ?? 0;
            _view.SetRemainCoin(balance);
        }

        public IReadOnlyList<IPurchasable> OrderedOffers => _orderedOffers;
    }
}