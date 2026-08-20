using System;

namespace MoreMountains
{
    public sealed class OverlayMenuBinder
    {
        OverlayMenu _panel;
        BallInventoryBinder _ballInv;
        CharacterInfoBinder _infoBinder;
        APlayer _player;

        public OverlayMenuBinder(
            OverlayMenu panel,
            BallInventoryBinder ballInv,
            CharacterInfoBinder infoBinder
        )
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _ballInv = ballInv ?? throw new ArgumentNullException(nameof(ballInv));
            _infoBinder = infoBinder ?? throw new ArgumentNullException(nameof(infoBinder));
        }

        public BallInventoryBinder BallInventory => _ballInv;

        public void Bind(APlayer player)
        {
            if (ReferenceEquals(_player, player))
                return;

            if (_player)
                Unbind();

            _player = player ?? throw new ArgumentNullException(nameof(player));

            // 子 binder 挂入各自 model
            _ballInv.Attach(_player.Inventory.BallBag);
            _infoBinder.Attach(_player);
        }

        public void Unbind()
        {
            if (_player == null)
                return;

            _ballInv.Detach();
            _infoBinder.Detach();

            _player = null;
        }

        /// <summary>外部调用:本阶段开始。</summary>
        public void Open()
        {
            _panel.setActive(true);
        }

        /// <summary>外部调用:本阶段结束。</summary>
        public void Close()
        {
            _panel.setActive(false);
        }

        public APlayer Player => _player;
    }
}