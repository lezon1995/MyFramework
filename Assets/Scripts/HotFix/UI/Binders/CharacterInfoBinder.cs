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
    public sealed class CharacterInfoBinder
    {
        CharacterInfoView _view;
        APlayer _player;

        Action<int> onLevelUp;
        Action<int, int> onLevelChanged;
        Action<int, int> onExpChanged;
        Action<int, int> onHealthChanged;
        Action<int, int> onExpRequiredChanged;
        Dictionary<string, IDisposable> statsDisposables = new();

        CharacterInfoBinder()
        {
            onLevelUp = OnLevelUpAction;
            onLevelChanged = OnLevelChanged;
            onExpChanged = OnExpChanged;
            onExpRequiredChanged = OnExpRequiredChanged;
            onHealthChanged = OnHealthChanged;
        }

        public CharacterInfoBinder(CharacterInfoView view) : this()
        {
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public void Attach(APlayer player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));

            // 等级 / 经验 监听可在此处挂 PlayerWallet.OnBalanceChanged 等
            // 当前简化：使用固定占位 1/0，后续接入等级系统替换。
            _view.CharacterExpView.SetLevel(player.Exp.Level);
            _view.CharacterExpView.SetExp(player.Exp.currentExp, player.Exp.currentLevelRequiredExp);
            _view.CharacterHealthView.SetHealth(player.currentHealth, player.maxHealth);

            player.Exp.onLevelUp += onLevelUp;
            player.Exp.onLevelChanged += onLevelChanged;
            player.Exp.onExpChanged += onExpChanged;
            player.Exp.onExpRequiredChanged += onExpRequiredChanged;
            player.Health.onHealthChanged += onHealthChanged;

            // 属性列表占位
            using var _ = new ListScope<UniStats.Stat>(out var statList);
            statList.Add(player.GetStat(Character.Stat.AD));
            statList.Add(player.GetStat(Character.Stat.AP));
            statList.Add(player.GetStat(Character.Stat.AR));
            statList.Add(player.GetStat(Character.Stat.MR));
            statList.Add(player.GetStat(Character.Stat.AS));
            statList.Add(player.GetStat(Character.Stat.LifeSteal));
            statList.Add(player.GetStat(Character.Stat.CritChance));
            statList.Add(player.GetStat(Character.Stat.MS));
            // statList.Add(player.GetStat(Character.Stat.Range));
            // statList.Add(player.GetStat(Character.Stat.DodgeChance));
            // statList.Add(player.GetStat(Character.Stat.BallisticSpeed));
            // statList.Add(player.GetStat(Character.Stat.HitEffectChance));
            // statList.Add(player.GetStat(Character.Stat.Knockback));
            // statList.Add(player.GetStat(Character.Stat.Duration));
            // statList.Add(player.GetStat(Character.Stat.Luck));
            // statList.Add(player.GetStat(Character.Stat.Greed));
            _view.CharacterStatsView.BuildPlayerStats(statList, (item, stat) =>
            {
                // item.SetIcon();
                var enhanced = gameDesign.universalColor.enhanced;
                var reduced = gameDesign.universalColor.reduced;
                var unchanged = Color.white;
                // item.setStringReference("Stats", stat.Name);
                Color color;
                if (stat.BonusValue > 0)
                    color = enhanced;
                else if (stat.BonusValue < 0)
                    color = reduced;
                else
                    color = unchanged;
                // item.SetNameColor(color);
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
        
        void OnHealthChanged(int cur, int max)
        {
            RefreshHealth(cur, max);
        }

        public void Detach()
        {
            if (_player)
            {
                _player.Exp.onLevelUp -= onLevelUp;
                _player.Exp.onLevelChanged -= onLevelChanged;
                _player.Exp.onExpChanged -= onExpChanged;
                _player.Exp.onExpRequiredChanged -= onExpRequiredChanged;
                _player.Health.onHealthChanged -= onHealthChanged;
            }

            foreach (var (statName, disposable) in statsDisposables)
                disposable.Dispose();

            statsDisposables.Clear();

            _player = null;
        }

        public void RefreshExp(int cur, int max) => _view.CharacterExpView.SetExp(cur, max);
        public void RefreshHealth(int cur, int max) => _view.CharacterHealthView.SetHealth(cur, max);
        public void RefreshLevel(int lv) => _view.CharacterExpView.SetLevel(lv);
    }
}