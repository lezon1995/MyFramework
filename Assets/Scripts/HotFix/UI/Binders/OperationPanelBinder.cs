using System;
using System.Collections.Generic;
using UnityEngine;

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
        OperationPanel _panel;
        BallInventoryBinder _ballInv;
        RelicInventoryBinder _relicInv;
        BallSlotGroupBinder _slotBinder;
        ShopBinder _shop;
        PlayerInfoBinder _playerInfo;
        APlayer _player;

        // BallOperationStateManager 事件处理
        Action<IBallOperationTarget> _onOperationConfirmed;
        Action _onOperationCancelled;

        public OperationPanelBinder(
            OperationPanel panel,
            BallInventoryBinder ballInv,
            RelicInventoryBinder relicInv,
            BallSlotGroupBinder slotBinder,
            ShopBinder shop,
            PlayerInfoBinder playerInfo)
        {
            _panel = panel ?? throw new ArgumentNullException(nameof(panel));
            _ballInv = ballInv ?? throw new ArgumentNullException(nameof(ballInv));
            _relicInv = relicInv ?? throw new ArgumentNullException(nameof(relicInv));
            _slotBinder = slotBinder ?? throw new ArgumentNullException(nameof(slotBinder));
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _playerInfo = playerInfo ?? throw new ArgumentNullException(nameof(playerInfo));

            // 子 binder 互相认识
            _ballInv.SetOwner(this);
            _relicInv.SetOwner(this);
            _slotBinder.SetOwner(this);

            // 预分配事件处理,避免在 Subscribe 时创建 lambda
            _onOperationConfirmed = HandleOperationConfirmed;
            _onOperationCancelled = HandleOperationCancelled;
        }

        public BallInventoryBinder BallInventory => _ballInv;
        public RelicInventoryBinder RelicInventory => _relicInv;
        public BallSlotGroupBinder SlotGroup => _slotBinder;
        public ShopBinder Shop => _shop;
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
            _ballInv.Attach(_player.Inventory.BallBag);
            _relicInv.Attach(_player.Inventory.RelicBag);
            _shop.Attach(_player, _player.Shop.Controller);

            // 监听子 binder 的事件
            _ballInv.EquipRequested += OnEquipBallRequested;
            _ballInv.UpgradeRequested += OnUpgradeBallRequested;
            _ballInv.SellRequested += OnSellBallRequested;

            _relicInv.SellRequested += OnSellRelicRequested;

            _shop.RerollClicked += OnShopRerollRequested;
            _shop.BuyExpClicked += OnShopBuyExpRequested;
            _shop.OfferBuyClicked += OnShopBuyRequested;
            _shop.SellZoneClicked += OnShopSellZoneClicked;

            _slotBinder.SelectionChanged += OnSlotSelectionChanged;

            // 订阅球操作状态管理器
            BallOperationStateManager.Instance.OperationConfirmed += _onOperationConfirmed;
            BallOperationStateManager.Instance.OperationCancelled += _onOperationCancelled;

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
            BallOperationStateManager.Instance.OperationConfirmed -= _onOperationConfirmed;
            BallOperationStateManager.Instance.OperationCancelled -= _onOperationCancelled;

            _ballInv.EquipRequested -= OnEquipBallRequested;
            _ballInv.UpgradeRequested -= OnUpgradeBallRequested;
            _ballInv.SellRequested -= OnSellBallRequested;
            _relicInv.SellRequested -= OnSellRelicRequested;
            _shop.RerollClicked -= OnShopRerollRequested;
            _shop.BuyExpClicked -= OnShopBuyExpRequested;
            _shop.OfferBuyClicked -= OnShopBuyRequested;
            _shop.SellZoneClicked -= OnShopSellZoneClicked;
            _slotBinder.SelectionChanged -= OnSlotSelectionChanged;

            _playerInfo.Detach();
            _slotBinder.Detach();
            _ballInv.Detach();
            _relicInv.Detach();
            _shop.Detach();

            _player = null;
        }

        /// <summary>外部调用:本阶段开始。</summary>
        public void Open()
        {
            _panel.setActive(true);
            _player?.Shop?.EnterShop();
        }

        /// <summary>外部调用:本阶段结束。</summary>
        public void Close()
        {
            _panel.setActive(false);
        }

        // ============================================================
        //         球操作状态事件处理(替代原有拖拽释放)
        // ============================================================

        void HandleOperationConfirmed(IBallOperationTarget hoveredTarget)
        {
            if (_player == null) return;

            var source = BallOperationStateManager.Instance.CurrentSource;
            if (source == null) return;

            // hoveredTarget == null → 空白区域点击 → 退出(不执行任何操作)
            if (hoveredTarget == null) return;

            // 执行操作
            source.ExecuteOperation(hoveredTarget);
        }

        void HandleOperationCancelled()
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
            if (_player == null) return;

            var source = BallOperationStateManager.Instance.CurrentSource;
            if (source is not BallSlotItem sourceSlotItem) return;

            var hovered = BallOperationStateManager.Instance.CurrentHovered;

            if (hovered is BallSlotItem targetSlotItem)
            {
                // Swap / Move
                int targetSlotIndex = -1;
                _slotBinder.GetSlotIndexForItem(targetSlotItem, out targetSlotIndex);
                if (targetSlotIndex >= 0 && targetSlotIndex != sourceSlotIndex)
                    _player.BallManagement.SwapSlots(sourceSlotIndex, targetSlotIndex);
            }
            else if (hovered is BallInventoryItem)
            {
                // Unequip 到背包
                _player.BallManagement.UnequipBall(sourceSlotIndex);
            }
        }

        /// <summary>
        /// BallInventoryItem 上左键点击时的操作。
        /// source 是背包里的球,hoveredTarget 是点击的目标。
        ///   • hoveredTarget 是 BallSlotItem → Equip
        ///   • hoveredTarget 是 BallInventoryItem → 无效
        /// </summary>
        public void OnInventoryOperationConfirmed(int sourceSlotIndex)
        {
            if (_player == null) return;

            var hovered = BallOperationStateManager.Instance.CurrentHovered;

            if (hovered is BallSlotItem targetSlotItem)
            {
                // Equip 到槽位
                int targetSlotIndex = -1;
                _slotBinder.GetSlotIndexForItem(targetSlotItem, out targetSlotIndex);
                var bag = _player.Inventory.BallBag;
                if (bag != null && sourceSlotIndex >= 0 && sourceSlotIndex < bag.SlotList.Count)
                {
                    var ball = bag.SlotList[sourceSlotIndex].Item;
                    if (ball != null)
                    {
                        if (targetSlotIndex >= 0)
                            _player.BallManagement.EquipBall(ball, targetSlotIndex);
                        else
                            _player.BallManagement.EquipBall(ball);
                    }
                }
            }
        }

        // ============================================================
        //         SellZone 点击(球操作状态中)
        // ============================================================

        void OnShopSellZoneClicked()
        {
            if (_player == null) return;

            var source = BallOperationStateManager.Instance.CurrentSource;
            if (source == null) return;

            // 根据 source 类型决定出售什么
            if (source is BallSlotItem slotItem)
            {
                int slotIndex = -1;
                _slotBinder.GetSlotIndexForItem(slotItem, out slotIndex);
                var slots = _player.BallManagement.Slots;
                if (slots != null && slotIndex >= 0 && slotIndex < slots.Slots.Count)
                {
                    var ball = slots.Slots[slotIndex].Item;
                    if (ball != null)
                        _player.Shop.Controller.OnPlayerSellBall(_player, ball);
                }
            }
            else if (source is BallInventoryItem invItem)
            {
                int slotIndex = -1;
                _ballInv.GetSlotIndexForItem(invItem, out slotIndex);
                var bag = _player.Inventory.BallBag;
                if (bag != null && slotIndex >= 0 && slotIndex < bag.SlotList.Count)
                {
                    var ball = bag.SlotList[slotIndex].Item;
                    if (ball != null)
                        _player.Shop.Controller.OnPlayerSellBall(_player, ball);
                }
            }
        }

        // ============================================================
        //         原有拖拽释放(保留:RelicInventory 仍用拖拽)
        // ============================================================

        /// <summary>球背包里的球被拖出去,转发到此处理。</summary>
        internal void OnBallInventoryDragReleased(BallInventoryItem src, BallItem ball, UIDragReleaseEventData data)
        {
            if (_player == null || ball == null) return;

            // 1. 释放到 sellZone → 出售
            if (TryFindSellZone(data))
            {
                _player.Shop.Controller.OnPlayerSellBall(_player, ball);
                _ballInv.ClearSelection();
                return;
            }

            // 2. 释放到某个槽位 → 装备到该槽位
            if (TryFindSlotIndex(data, out int slotIndex))
            {
                _player.BallManagement.EquipBall(ball, slotIndex);
                _ballInv.ClearSelection();
                return;
            }
        }

        /// <summary>遗物背包里的遗物被拖出去,转发到此处理。</summary>
        internal void OnRelicInventoryDragReleased(RelicInventoryItem src, RelicItem relic, UIDragReleaseEventData data)
        {
            if (_player == null || relic == null) return;

            // 遗物:释放到 sellZone → 出售;否则不处理
            if (TryFindSellZone(data))
            {
                _player.Shop.Controller.OnPlayerSellRelic(relic);
                _relicInv.ClearSelection();
            }
        }

        /// <summary>槽位上的球被拖出去,转发到此处理。</summary>
        internal void OnSlotDragReleased(BallSlotItem src, int sourceSlotIndex, BallItem ball, UIDragReleaseEventData data)
        {
            if (_player == null || ball == null) return;

            // 1. 释放到 sellZone → 出售
            if (TryFindSellZone(data))
            {
                _player.Shop.Controller.OnPlayerSellBall(_player, ball);
                return;
            }

            // 2. 释放到另一个槽位 → Swap
            if (TryFindSlotIndex(data, out int targetSlotIndex))
            {
                if (targetSlotIndex != sourceSlotIndex)
                    _player.BallManagement.SwapSlots(sourceSlotIndex, targetSlotIndex);
                return;
            }

            // 3. 释放到球背包某个 item 上 → 卸下到背包
            if (TryFindBallInventory(data))
            {
                _player.BallManagement.UnequipBall(sourceSlotIndex);
            }
        }

        // ------- 拖拽释放目标识别 helpers -------

        bool TryFindSellZone(UIDragReleaseEventData data)
        {
            if (data == null) return false;
            var sellRoot = _panel.Shop.SellZoneRoot;
            if (sellRoot == null) return false;
            var sellTransform = sellRoot.getGameObject().transform;

            foreach (var go in data.GameObjects)
            {
                if (go == null) continue;
                if (go.transform == sellTransform || go.transform.IsChildOf(sellTransform))
                    return true;
            }
            return false;
        }

        bool TryFindSlotIndex(UIDragReleaseEventData data, out int slotIndex)
        {
            slotIndex = -1;
            if (data == null) return false;

            var slots = _player?.BallManagement?.Slots;
            if (slots == null) return false;

            int i = 0;
            foreach (var slot in slots.Slots)
            {
                if (_panel.PlayerInfo.SlotGroup.GetUsedItem(i, out var slotItem) && slotItem != null)
                {
                    var t = slotItem.ItemGO?.transform;
                    if (t != null && HitContains(t, data))
                    {
                        slotIndex = slot.Index;
                        return true;
                    }
                }
                i++;
            }
            return false;
        }

        bool TryFindBallInventory(UIDragReleaseEventData data)
        {
            if (data == null) return false;
            var bag = _player?.Inventory?.BallBag;
            if (bag == null) return false;

            int i = 0;
            foreach (var slot in bag.SlotList)
            {
                if (_panel.BallInventory.GetUsedItem(i, out var item) && item != null)
                {
                    var t = item.ItemGO?.transform;
                    if (t != null && HitContains(t, data))
                        return true;
                }
                i++;
            }
            return false;
        }

        static bool HitContains(Transform target, UIDragReleaseEventData data)
        {
            if (target == null || data == null) return false;
            foreach (var go in data.GameObjects)
            {
                if (go == null) continue;
                var t = go.transform;
                if (t != null && (t == target || t.IsChildOf(target)))
                    return true;
            }
            return false;
        }

        // ============================================================
        //         按钮 / 系统操作(保留,非操作状态时通过原有方式触发)
        // ============================================================

        void OnEquipBallRequested(BallItem ball, int slotIndex)
        {
            if (_player == null) return;

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
            if (_player == null) return;

            var candidates = new List<BallItem> { ball };
            _player.BallManagement.Upgrade.TryUpgrade(candidates, out _);
            _ballInv.ClearSelection();
        }

        void OnSellBallRequested(BallItem ball)
        {
            if (_player == null) return;

            _player.Shop.Controller.OnPlayerSellBall(_player, ball);
            _ballInv.ClearSelection();
        }

        void OnSellRelicRequested(RelicItem relic)
        {
            if (_player == null) return;

            _player.Shop.Controller.OnPlayerSellRelic(relic);
            _relicInv.ClearSelection();
        }

        void OnShopBuyRequested(IPurchasable offer)
        {
            if (_player == null || offer == null) return;

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

                ARelic underlying = null;
                if (string.IsNullOrEmpty(relicOffer.Def.RelicTypeName))
                {
                    logError($"OperationPanelBinder: RelicDef '{relicOffer.Def.name}' missing RelicTypeName; cannot instantiate.");
                    _player.Wallet.Earn(price, EarnType.OTHER, "rollback_buy_relic");
                    return;
                }

                try
                {
                    var t = Type.GetType(relicOffer.Def.RelicTypeName);
                    if (t != null && typeof(ARelic).IsAssignableFrom(t))
                        underlying = (ARelic)Activator.CreateInstance(t);
                }
                catch (Exception ex)
                {
                    logError($"OperationPanelBinder: failed to create relic '{relicOffer.Def.RelicTypeName}': {ex.Message}");
                }

                if (underlying == null)
                {
                    _player.Wallet.Earn(price, EarnType.OTHER, "rollback_buy_relic");
                    return;
                }

                var ritem = new RelicItem(relicOffer.Def);
                if (!_player.Inventory.AddRelic(ritem))
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
            _shop.Rebuild();
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
    }
}