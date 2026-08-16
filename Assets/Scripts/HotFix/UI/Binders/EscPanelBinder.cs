using System;

namespace MoreMountains
{
    public sealed class EscPanelBinder
    {
        EscPanel _panel;
        BallInventoryBinder _ballInv;
        RelicInventoryBinder _relicInv;
        PlayerInfoBinder _playerInfo;
        WaveMonsterBinder _waveMonster;
        APlayer _player;

        public EscPanelBinder(
            EscPanel panel,
            BallInventoryBinder ballInv,
            RelicInventoryBinder relicInv,
            PlayerInfoBinder playerInfo,
            WaveMonsterBinder waveMonster
        )
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _ballInv = ballInv ?? throw new ArgumentNullException(nameof(ballInv));
            _relicInv = relicInv ?? throw new ArgumentNullException(nameof(relicInv));
            _playerInfo = playerInfo ?? throw new ArgumentNullException(nameof(playerInfo));
            _waveMonster = waveMonster ?? throw new ArgumentNullException(nameof(waveMonster));
        }

        public BallInventoryBinder BallInventory => _ballInv;
        public RelicInventoryBinder RelicInventory => _relicInv;
        public PlayerInfoBinder PlayerInfo => _playerInfo;

        public void Bind(APlayer player)
        {
            if (ReferenceEquals(_player, player))
                return;

            if (_player)
                Unbind();

            _player = player ?? throw new ArgumentNullException(nameof(player));

            // 子 binder 挂入各自 model
            _playerInfo.Attach(_player);
            _ballInv.Attach(_player.Inventory.BallBag);
            _relicInv.Attach(_player.Inventory.RelicBag);
            _waveMonster.Attach(waveManager.CurWave);
        }

        public void Unbind()
        {
            if (_player == null)
                return;

            _playerInfo.Detach();
            _ballInv.Detach();
            _relicInv.Detach();
            _waveMonster.Detach();

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