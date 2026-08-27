using System;
using System.Collections.Generic;
using UniStats;

namespace MoreMountains
{
    /// <summary>
    /// OperationPanelBinder —— 持有并协调所有子 binder。
    ///
    /// 职责：
    ///   • Bind(APlayer)：把玩家四大系统注入子 binder。
    ///   • Open() / Close()：被 ShoppingPhase.onBegin/onEnd 调用。
    ///   • 处理球操作状态中的点击操作(替代原有拖拽)。
    ///
    /// 新的点击操作流程:
    ///   左键点击 BallSlotItem/BallInventoryItem → 进入操作状态(icon 跟随鼠标)
    ///   → 在 BallSlotItem/BallInventoryItem 上左键点击 → 执行 Equip/Swap/Unequip
    ///   → 在 SellZone 上左键点击 → 出售
    ///   → 在空白区域左键点击 → 退出操作状态
    ///   → 右键点击 → 退出操作状态
    /// </summary>
    public sealed class OperationPanelBinder
    {
        public OperationPanel Panel => _panel;
        
        OperationPanel _panel;
        BallInventoryBinder _ballInv;
        RelicInventoryBinder _relicInv;
        BallSlotGroupBinder _slotBinder;
        ShopBinder _shop;
        RewardChooseBinder _rewardChoose;
        PlayerInfoBinder _playerInfo;
        WaveMonsterBinder _waveMonster;
        APlayer _player;

        // BallOperationStateManager 事件处理
        Action<IItemOperationTarget> _onBallOperationConfirmed;
        Action _onOperationCancelled;

        // RelicOperationStateManager 事件处理
        Action<IItemOperationTarget> _onRelicOperationConfirmed;
        Action _onRelicOperationCancelled;

        public OperationPanelBinder(
            OperationPanel panel,
            BallInventoryBinder ballInv,
            RelicInventoryBinder relicInv,
            BallSlotGroupBinder slotBinder,
            ShopBinder shop,
            RewardChooseBinder rewardChoose,
            PlayerInfoBinder playerInfo,
            WaveMonsterBinder waveMonster
        )
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _ballInv = ballInv ?? throw new ArgumentNullException(nameof(ballInv));
            _relicInv = relicInv ?? throw new ArgumentNullException(nameof(relicInv));
            _slotBinder = slotBinder ?? throw new ArgumentNullException(nameof(slotBinder));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _rewardChoose = rewardChoose ?? throw new ArgumentNullException(nameof(rewardChoose));
            _playerInfo = playerInfo ?? throw new ArgumentNullException(nameof(playerInfo));
            _waveMonster = waveMonster ?? throw new ArgumentNullException(nameof(waveMonster));

            // 子 binder 互相认识
            _slotBinder.SetOwner(this);
            _shop.SetOwner(this);
            _rewardChoose.SetOwner(this);

            // 预分配事件处理,避免在 Subscribe 时创建 lambda
            _onBallOperationConfirmed = HandleOperationConfirmed;
            _onOperationCancelled = HandleOperationCancelled;
            _onRelicOperationConfirmed = HandleRelicOperationConfirmed;
            _onRelicOperationCancelled = HandleRelicOperationCancelled;
        }

        public BallInventoryBinder BallInventory => _ballInv;
        public RelicInventoryBinder RelicInventory => _relicInv;
        public BallSlotGroupBinder SlotGroup => _slotBinder;
        public ShopBinder Shop => _shop;
        public RewardChooseBinder RewardChoose => _rewardChoose;
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
            _slotBinder.Attach(_player.BallManagement.Slots);
            _ballInv.Attach(_player, _player.Inventory.BallBag);
            _relicInv.Attach(_player, _player.Inventory.RelicBag);
            _shop.Attach(_player, _player.Shop.Controller);
            _rewardChoose.Attach(_player, _player.RewardSystem.Controller);
            _waveMonster.Attach(waveManager.NextWave);

            // 监听子 binder 的事件
            _ballInv.EquipRequested += OnEquipBallRequested;
            _ballInv.UpgradeRequested += OnUpgradeBallRequested;
            _ballInv.SellRequested += OnSellBallRequested;

            _relicInv.SellRequested += OnSellRelicRequested;

            _shop.RerollClicked += OnShopRerollRequested;
            _shop.BuyExpClicked += OnShopBuyExpRequested;
            _shop.OfferBuyClicked += OnShopBuyRequested;

            _rewardChoose.RerollClicked += OnRewardRerollRequested;
            _rewardChoose.OfferBuyClicked += OnRewardBuyRequested;

            _slotBinder.SelectionChanged += OnSlotSelectionChanged;

            // 订阅球操作状态管理器
            BallOperationStateManager.Instance.OperationConfirmed += _onBallOperationConfirmed;
            BallOperationStateManager.Instance.OperationCancelled += _onOperationCancelled;

            // 订阅遗物操作状态管理器
            RelicOperationStateManager.Instance.OperationConfirmed += _onRelicOperationConfirmed;
            RelicOperationStateManager.Instance.OperationCancelled += _onRelicOperationCancelled;

            // 钱变化时刷新 coin 显示
            _player.Wallet.OnBalanceChanged += OnWalletChanged;

            // Next 按钮
            _panel.BtnNext.setUGUIButtonClick(NextStage);
        }

        public void Unbind()
        {
            if (_player == null)
                return;

            _player.Wallet.OnBalanceChanged -= OnWalletChanged;
            BallOperationStateManager.Instance.OperationConfirmed -= _onBallOperationConfirmed;
            BallOperationStateManager.Instance.OperationCancelled -= _onOperationCancelled;
            RelicOperationStateManager.Instance.OperationConfirmed -= _onRelicOperationConfirmed;
            RelicOperationStateManager.Instance.OperationCancelled -= _onRelicOperationCancelled;

            _ballInv.EquipRequested -= OnEquipBallRequested;
            _ballInv.UpgradeRequested -= OnUpgradeBallRequested;
            _ballInv.SellRequested -= OnSellBallRequested;
            _relicInv.SellRequested -= OnSellRelicRequested;
            _shop.RerollClicked -= OnShopRerollRequested;
            _shop.BuyExpClicked -= OnShopBuyExpRequested;
            _shop.OfferBuyClicked -= OnShopBuyRequested;
            _rewardChoose.RerollClicked -= OnRewardRerollRequested;
            _rewardChoose.OfferBuyClicked -= OnRewardBuyRequested;
            _slotBinder.SelectionChanged -= OnSlotSelectionChanged;

            _playerInfo.Detach();
            _slotBinder.Detach();
            _ballInv.Detach();
            _relicInv.Detach();
            _shop.Detach();
            _rewardChoose.Detach();
            _waveMonster.Detach();

            _player = null;
        }

        /// <summary>外部调用:本阶段开始。</summary>
        public void Open()
        {
            _panel.setActive(true);
        }

        public void EnterReward(int waveNumber)
        {
            _panel.RefreshTitle(waveNumber + 1);
            _player?.RewardSystem?.EnterReward(waveNumber);

            _rewardChoose.SetViewActive(true);
            _ballInv.SetViewActive(false);
            _relicInv.SetViewActive(false);
            _shop.SetViewActive(false);
        }

        public void EnterShop(int waveNumber)
        {
            _player?.Shop?.EnterShop(waveNumber);

            _rewardChoose.SetViewActive(false);
            _ballInv.SetViewActive(true);
            _relicInv.SetViewActive(true);
            _shop.SetViewActive(true);
        }

        /// <summary>外部调用:本阶段结束。</summary>
        public void Close()
        {
            _panel.setActive(false);
        }

        // ============================================================
        //         球操作状态事件处理(替代原有拖拽释放)
        // ============================================================

        void HandleOperationConfirmed(IItemOperationTarget hoveredTarget)
        {
            if (_player == null)
                return;

            var source = BallOperationStateManager.Instance.CurrentSource;
            if (source == null)
                return;

            // hoveredTarget == null → 空白区域点击 → 退出(不执行任何操作)
            if (hoveredTarget == null)
                return;

            // 执行操作
            source.ExecuteOperation(hoveredTarget);
        }

        void HandleOperationCancelled()
        {
            // 右键取消,什么都不做
        }

        // ============================================================
        //         遗物操作状态事件处理
        // ============================================================

        void HandleRelicOperationConfirmed(IItemOperationTarget hoveredTarget)
        {
            if (_player == null)
                return;

            var source = RelicOperationStateManager.Instance.CurrentSource;
            if (source == null)
                return;

            if (hoveredTarget == null)
                return;

            source.ExecuteOperation(hoveredTarget);
        }

        void HandleRelicOperationCancelled()
        {
            // 右键取消,什么都不做
        }

        /// <summary>
        /// BallSlotItem 上左键点击时的操作。
        /// source 是槽位上的球,hoveredTarget 是点击的目标。
        ///   • hoveredTarget 是 BallSlotItem → Swap(若目标槽非空则交换,空则移动)
        ///   • hoveredTarget 是 BallInventoryItem → Unequip 到背包
        /// </summary>
        public void OnSlotOperationConfirmed(int sourceSlotIndex)
        {
            if (_player == null)
                return;

            var source = BallOperationStateManager.Instance.CurrentSource;
            if (source is not BallSlotItem sourceSlotItem)
                return;

            var hovered = BallOperationStateManager.Instance.CurrentHovered;

            if (hovered is BallSlotItem targetSlotItem)
            {
                // Swap / Move
                _slotBinder.GetSlotIndexForItem(targetSlotItem, out var targetSlotIndex);
                if (targetSlotIndex >= 0 && targetSlotIndex != sourceSlotIndex)
                    _player.BallManagement.SwapSlots(sourceSlotIndex, targetSlotIndex);
            }
            else if (hovered is BallInventoryItem)
            {
                // Unequip 到背包
                _player.BallManagement.UnequipBall(sourceSlotIndex);
            }
        }

        // ============================================================
        //         SellZone 点击(球操作状态中)
        // ============================================================

        // ============================================================
        //         按钮 / 系统操作(保留,非操作状态时通过原有方式触发)
        // ============================================================

        void OnEquipBallRequested(BallItem ball, int slotIndex)
        {
            if (_player == null)
                return;

            int target = slotIndex >= 0 ? slotIndex : _slotBinder.SelectedSlotIndex;
            if (target < 0)
                _player.BallManagement.EquipBall(ball);
            else
                _player.BallManagement.EquipBall(ball, target);

            _ballInv.ClearSelection();
        }

        void OnSlotSelectionChanged(int slotIndex)
        {
            // 槽位选中后,后续装备按钮以该槽为目标
        }

        void OnUpgradeBallRequested(BallItem ball)
        {
            if (_player == null)
                return;

            var candidates = new List<BallItem> { ball };
            _player.BallManagement.Upgrade.TryUpgrade(candidates, out _);
            _ballInv.ClearSelection();
        }

        public void OnSellBallRequested(BallItem ball)
        {
            if (_player == null)
                return;

            _player.Shop.Controller.OnPlayerSellBall(_player, ball);
            _ballInv.ClearSelection();
        }

        public void OnSellRelicRequested(RelicItem relic)
        {
            if (_player == null)
                return;

            _player.Shop.Controller.OnPlayerSellRelic(_player, relic);
            _relicInv.ClearSelection();
        }

        void OnShopBuyRequested(IPurchasable offer)
        {
            if (_player == null || offer == null)
                return;

            if (offer is BallOffer ballOffer && ballOffer.Def)
            {
                int price = ballOffer.Price;
                if (!_player.Wallet.Pay(price, PayType.BALL_BUY))
                    return;

                var created = _player.BallManagement.Shop.PurchaseAndStore(ballOffer.Def);
                if (created != null)
                    ballOffer.MarkSold();

                ShopEvents.RaiseOfferSold(ballOffer);
            }
            else if (offer is RelicOffer relicOffer && relicOffer.Def != null)
            {
                int price = relicOffer.Price;
                if (!_player.Wallet.Pay(price, PayType.RELIC_BUY))
                    return;

                if (relicOffer.Def.Type == RelicType.None)
                {
                    logError($"OperationPanelBinder: RelicDef '{relicOffer.Def.name}' missing RelicTypeName; cannot instantiate.");
                    _player.Wallet.Earn(price, EarnType.OTHER, "rollback_buy_relic");
                    return;
                }
                
                if (!_player.Inventory.AddRelic(relicOffer.Def))
                {
                    _player.Wallet.Earn(price, EarnType.OTHER, "rollback_buy_relic");
                    return;
                }

                relicOffer.MarkSold();
                ShopEvents.RaiseOfferSold(relicOffer);
            }

            _shop.RefreshCoin();
        }

        void OnShopRerollRequested()
        {
            _player?.Shop?.Controller.OnPlayerClickReroll();
        }

        void OnShopBuyExpRequested()
        {
            var baseExp = gameDesign.baseExpStandard;
            var totalExp = baseExp;
            _player.gainExp(totalExp);
        }

        void OnWalletChanged(int _) => _shop.RefreshCoin();

        void NextStage()
        {
            _player?.Shop?.Controller.OnPlayerClickNext();
        }

        public APlayer Player => _player; 

        void OnRewardRerollRequested()
        {
            _player?.RewardSystem?.Controller.OnPlayerClickReroll();
        }

        void OnRewardBuyRequested(IPurchasable offer)
        {
            if (_player == null || offer == null)
                return;

            switch (offer)
            {
                case BallStatOffer ballStatOffer:
                    ballStatOffer.MarkSold();
                    RewardEvents.RaiseOfferSold(ballStatOffer);
                    break;
                case PlayerStatOffer playerStatOffer:
                    playerStatOffer.MarkSold();
                    RewardEvents.RaiseOfferSold(playerStatOffer);

                    if (_player.GetStat(playerStatOffer.Def.stat, out var stat))
                    {
                        if (playerStatOffer.BonusFlat > 0)
                        {
                            stat.BonusFlat.AddFlat(playerStatOffer.BonusFlat);
                        }

                        if (playerStatOffer.BonusPct > 0)
                        {
                            stat.BonusPct.AddFlat(playerStatOffer.BonusPct);
                        }
                    }

                    break;
            }
        }
    }
}