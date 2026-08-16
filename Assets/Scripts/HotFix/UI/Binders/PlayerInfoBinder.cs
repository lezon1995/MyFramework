using System;
using System.Collections.Generic;
using UniStats;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// PlayerInfo binder —— 负责等级、经验、属性列表显示。
    /// 槽位的显示交给嵌入的 BallSlotGroupView，由 BallSlotGroupBinder 直接驱动。
    /// </summary>
    public sealed class PlayerInfoBinder
    {
        PlayerInfoView _view;
        BallSlotGroupBinder _slotBinder; // 共享，绑定嵌入的 BallSlotGroupView
        APlayer _player;

        Action<int> onLevelUp;
        Action<int, int> onLevelChanged;
        Action<int, int> onExpChanged;
        Action<int, int> onExpRequiredChanged;
        Dictionary<string, IDisposable> statsDisposables = new();

        PlayerInfoBinder()
        {
            onLevelUp = OnLevelUpAction;
            onLevelChanged = OnLevelChanged;
            onExpChanged = OnExpChanged;
            onExpRequiredChanged = OnExpRequiredChanged;
        }

        public PlayerInfoBinder(PlayerInfoView view, BallSlotGroupBinder slotBinder) : this()
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _slotBinder = slotBinder ?? throw new ArgumentNullException(nameof(slotBinder));
        }

        public void Attach(APlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));

            // 等级 / 经验 监听可在此处挂 PlayerWallet.OnBalanceChanged 等
            // 当前简化：使用固定占位 1/0，后续接入等级系统替换。
            _view.SetLevel(player.Exp.Level);
            _view.SetExp(player.Exp.currentExp, player.Exp.currentLevelRequiredExp);

            player.Exp.onLevelUp = onLevelUp;
            player.Exp.onLevelChanged = onLevelChanged;
            player.Exp.onExpChanged = onExpChanged;
            player.Exp.onExpRequiredChanged = onExpRequiredChanged;

            // 属性列表占位
            using var _ = new ListScope<UniStats.Stat>(out var statList);
            statList.Add(player.GetStat(Character.Stat.HealthMax));
            statList.Add(player.GetStat(Character.Stat.HealthRegen));
            statList.Add(player.GetStat(Character.Stat.AD));
            // statList.Add(player.GetStat(Character.Stat.AD_PT));
            // statList.Add(player.GetStat(Character.Stat.AD_PT_Rate));
            statList.Add(player.GetStat(Character.Stat.AP));
            statList.Add(player.GetStat(Character.Stat.AS));
            // statList.Add(player.GetStat(Character.Stat.AP_PT));
            // statList.Add(player.GetStat(Character.Stat.AP_PT_Rate));
            statList.Add(player.GetStat(Character.Stat.CritChance));
            statList.Add(player.GetStat(Character.Stat.CritDamage));
            statList.Add(player.GetStat(Character.Stat.DmgRate));
            statList.Add(player.GetStat(Character.Stat.AR));
            statList.Add(player.GetStat(Character.Stat.MS));
            statList.Add(player.GetStat(Character.Stat.LifeSteal));
            statList.Add(player.GetStat(Character.Stat.Range));
            statList.Add(player.GetStat(Character.Stat.DodgeChance));
            statList.Add(player.GetStat(Character.Stat.BallisticSpeed));
            statList.Add(player.GetStat(Character.Stat.HitEffectChance));
            statList.Add(player.GetStat(Character.Stat.Knockback));
            statList.Add(player.GetStat(Character.Stat.Duration));
            statList.Add(player.GetStat(Character.Stat.Luck));
            statList.Add(player.GetStat(Character.Stat.Greed));
            _view.BuildPlayerStats(statList, (item, stat) =>
            {
                // item.SetIcon();
                var enhanced = gameDesign.universalColor.enhanced;
                var reduced = gameDesign.universalColor.reduced;
                var unchanged = Color.white;
                item.setStringReference("Stats", stat.Name);
                Color color;
                if (stat.BonusValue > 0)
                    color = enhanced;
                else if (stat.BonusValue < 0)
                    color = reduced;
                else
                    color = unchanged;
                item.SetNameColor(color);
                item.SetValueColor(color);
                item.SetValue(stat.DisplayValueGetter());
                item.SetIcon(stat.DisplayIcon);
                var disposable = stat.OnChange(v =>
                {
                    Color color;
                    if (stat.BonusValue > 0)
                        color = enhanced;
                    else if (stat.BonusValue < 0)
                        color = reduced;
                    else
                        color = unchanged;
                    item.SetNameColor(color);
                    item.SetValueColor(color);
                    item.SetValue(stat.DisplayValueGetter());
                });

                statsDisposables[stat.Name] = disposable;
            });
        }

        void OnLevelUpAction(int curLevel)
        {
            _player.BallManagement.ExpandSlots(1);
        }

        void OnLevelChanged(int pre, int cur)
        {
            RefreshLevel(cur);
        }

        void OnExpChanged(int xp, int xpRequired)
        {
            RefreshExp(xp, xpRequired);
        }

        void OnExpRequiredChanged(int xp, int xpRequired)
        {
            RefreshExp(xp, xpRequired);
        }

        public void Detach()
        {
            if (_player)
            {
                _player.Exp.onLevelChanged = null;
                _player.Exp.onExpChanged = null;
                _player.Exp.onExpRequiredChanged = null;
            }

            foreach (var (statName, disposable) in statsDisposables)
                disposable.Dispose();

            statsDisposables.Clear();

            _player = null;
        }

        public void RefreshExp(int cur, int max) => _view.SetExp(cur, max);
        public void RefreshLevel(int lv) => _view.SetLevel(lv);
    }

    public struct PlayerStatRow
    {
        public string Name;
        public string Value;
    }
}