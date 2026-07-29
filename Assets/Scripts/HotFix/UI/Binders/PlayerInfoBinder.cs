using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// PlayerInfo binder —— 负责等级、经验、属性列表显示。
    /// 槽位的显示交给嵌入的 BallSlotGroupView，由 BallSlotGroupBinder 直接驱动。
    /// </summary>
        public sealed class PlayerInfoBinder
    {
        readonly PlayerInfoView _view;
        BallSlotGroupBinder _slotBinder; // 共享，绑定嵌入的 BallSlotGroupView
        APlayer _player;

        public PlayerInfoBinder(PlayerInfoView view, BallSlotGroupBinder slotBinder)
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _slotBinder = slotBinder ?? throw new ArgumentNullException(nameof(slotBinder));
        }

        public BallSlotGroupBinder SlotBinder => _slotBinder;

        public void Attach(APlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));

            // 等级 / 经验 监听可在此处挂 PlayerWallet.OnBalanceChanged 等
            // 当前简化：使用固定占位 1/0，后续接入等级系统替换。
            _view.SetLevel(1);
            _view.SetExp(0, 100);

            // 属性列表占位
            _view.BuildPlayerStats(Array.Empty<PlayerStatRow>(), (item, row) => { /* 留空 */ });
        }

        public void Detach()
        {
            _player = null;
        }

        public void RefreshExp(int cur, int max)        => _view.SetExp(cur, max);
        public void RefreshLevel(int lv)                 => _view.SetLevel(lv);
    }

    public struct PlayerStatRow
    {
        public string Name;
        public string Value;
    }
}
