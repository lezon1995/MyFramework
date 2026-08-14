using System;
using System.Collections.Generic;

namespace MoreMountains
{
    public sealed class RewardChooseBinder
    {
        OperationPanelBinder _owner;
        RewardChooseView _view;
        RewardController _ctrl;
        APlayer _player;

        // event handler cache —— 让 -= 能成功匹配
        Action<RewardBoardKind> _onBoardRefreshed;
        Action<IPurchasable> _onOfferSold;
        List<IPurchasable> _orderedOffers = new();

        public RewardChooseBinder(RewardChooseView view)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _onBoardRefreshed = _ => Rebuild();
        }

        public event Action<IPurchasable> OfferBuyClicked;
        public event Action RerollClicked;

        public RewardController Controller => _ctrl;

        public void Attach(APlayer player, RewardController ctrl)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _ctrl = ctrl ?? throw new ArgumentNullException(nameof(ctrl));
            _onOfferSold = _ => Rebuild();

            _view.SetTitle("Rewards");
            _view.BtnReroll.setUGUIButtonClick(OnRerollClicked);

            RewardEvents.OnBoardOpened += _onBoardRefreshed;
            RewardEvents.OnBoardRerolled += _onBoardRefreshed;
            RewardEvents.OnOfferSold += _onOfferSold;

            Rebuild();
        }

        public void Detach()
        {
            if (_ctrl == null)
                return;

            RewardEvents.OnBoardOpened -= _onBoardRefreshed;
            RewardEvents.OnBoardRerolled -= _onBoardRefreshed;
            RewardEvents.OnOfferSold -= _onOfferSold;

            _player = null;
            _ctrl = null;
        }
        
        public void SetOwner(OperationPanelBinder owner)
        {
            _owner = owner;
        }

        public void Rebuild()
        {
            if (_ctrl == null)
                return;

            _orderedOffers.Clear();

            _view.BuildBallStatOffers(_ctrl.BallStatOffers, (item, offer) =>
            {
                _orderedOffers.Add(offer);
                item.SetName(offer.DisplayName ?? "—");
                if (offer.Def)
                    item.SetDesc(offer);
                if (offer.Def.Icon)
                    item.SetIcon(offer.Def.Icon);
                item.SetRarity(offer.Rarity);
                item.SetHovered(false);
                item.SetSold(offer.Sold);
                item.Btn.setInteractable(offer.Enabled);
                item.Btn.setUGUIButtonClick(() => OnOfferClicked(offer));
            });
            
            _view.BuildPlayerStatOffers(_ctrl.PlayerStatOffers, (item, offer) =>
            {
                _orderedOffers.Add(offer);
                item.SetName(offer.DisplayName ?? "—");
                if (offer.Def)
                    item.SetDesc(offer);
                if (offer.Def.Icon)
                    item.SetIcon(offer.Def.Icon);
                item.SetRarity(offer.Rarity);
                item.SetHovered(false);
                item.SetSold(offer.Sold);
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
        
        public void SetViewActive(bool active) => _view.setActive(active);
    }
}