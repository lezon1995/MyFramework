using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// OperationPanelBinder —— 持有并协调所有子 binder。
    ///
    /// 职责：
    ///   • Bind(APlayer)：把玩家四大系统（BallManagement / Inventory / Wallet / Shop）注入子 binder。
    ///   • Open() / Close()：被 ShoppingPhase.onBegin/onEnd 调用。
    ///   • 处理按钮事件 + 处理「拖拽释放」事件，把 UI 操作翻译成系统命令。
    ///
    /// 拖拽释放的解析：
    ///   UIDragReleaseEventData.GameObjects 已经按 EventSystem.RaycastAll 的顺序（最前 → 最后）
    ///   给出释放点指针下方所有命中的 GameObject。我们依次看每个 go：
    ///     • 它是哪个 BallSlotItem 的子节点 → 槽位目标
    ///     • 它是哪个 BallInventoryItem 的子节点 → 球背包目标
    ///     • 它是哪个 RelicInventoryItem 的子节点 → 遗物背包目标
    ///     • 它位于 SellZone 子树 → 出售目标
    ///   第一个命中即为目标。
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

            // 子 binder 互相认识:drag 释放时它们能把事件转交回来。
            _ballInv.SetOwner(this);
            _relicInv.SetOwner(this);
            _slotBinder.SetOwner(this);
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

            // 监听子 binder 的事件,把 UI 操作翻译成对系统的命令
            _ballInv.EquipRequested += OnEquipBallRequested;
            _ballInv.UpgradeRequested += OnUpgradeBallRequested;
            _ballInv.SellRequested += OnSellBallRequested;

            _relicInv.SellRequested += OnSellRelicRequested;

            _shop.RerollClicked += OnShopRerollRequested;
            _shop.BuyExpClicked += OnShopBuyExpRequested;
            _shop.OfferBuyClicked += OnShopBuyRequested;

            // 槽位选中 → 装备按钮的 target slot
            _slotBinder.SelectionChanged += OnSlotSelectionChanged;

            // 钱变化时刷新 coin 显示
            _player.Wallet.OnBalanceChanged += OnWalletChanged;

            // Next 按钮: 推进 Shop 阶段
            _panel.BtnNext.setUGUIButtonClick(NextStage);
        }

        public void Unbind()
        {
            if (_player == null)
                return;

            _player.Wallet.OnBalanceChanged -= OnWalletChanged;
            _ballInv.EquipRequested -= OnEquipBallRequested;
            _ballInv.UpgradeRequested -= OnUpgradeBallRequested;
            _ballInv.SellRequested -= OnSellBallRequested;
            _relicInv.SellRequested -= OnSellRelicRequested;
            _shop.RerollClicked -= OnShopRerollRequested;
            _shop.BuyExpClicked -= OnShopBuyExpRequested;
            _shop.OfferBuyClicked -= OnShopBuyRequested;
            _slotBinder.SelectionChanged -= OnSlotSelectionChanged;

            _playerInfo.Detach();
            _slotBinder.Detach();
            _ballInv.Detach();
            _relicInv.Detach();
            _shop.Detach();

            _player = null;
        }

        /// <summary>外部（WaveSystem / ShoppingPhase）调用：本阶段开始。</summary>
        public void Open()
        {
            _panel.setActive(true);
            _player?.Shop?.EnterShop();
        }

        /// <summary>外部调用：本阶段结束。</summary>
        public void Close()
        {
            _panel.setActive(false);
        }

        // ============================================================
        //                 拖拽释放解析:目标识别
        // ============================================================

        // drag 释放时被三个子 binder 转交到这里:
        //   • OnBallInventoryDragReleased: 球背包里的球被拖出去了
        //   • OnRelicInventoryDragReleased: 遗物背包里的遗物被拖出去了
        //   • OnSlotDragReleased:          槽位上的球被拖出去了

        internal void OnBallInventoryDragReleased(BallInventoryItem src, BallItem ball, UIDragReleaseEventData data)
        {
            if (_player == null || ball == null) return;

            // 1. 释放到 sellZone → 出售
            if (TryFindSellZone(data, out _))
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

            // 3. 释放到球背包里的另一个 item → "放回原处 / 合并候选"，暂不做事(交给 Upgrade/Merge 流程)
        }

        internal void OnRelicInventoryDragReleased(RelicInventoryItem src, RelicItem relic, UIDragReleaseEventData data)
        {
            if (_player == null || relic == null) return;

            // 遗物：释放到 sellZone → 出售；否则不处理
            if (TryFindSellZone(data, out _))
            {
                _player.Shop.Controller.OnPlayerSellRelic(relic);
                _relicInv.ClearSelection();
            }
        }

        internal void OnSlotDragReleased(BallSlotItem src, int sourceSlotIndex, BallItem ball, UIDragReleaseEventData data)
        {
            if (_player == null || ball == null) return;

            // 1. 释放到 sellZone → 出售
            //    SellToShop 会通过 InventoryLocate 自动找到 holder(槽位或背包)并移除 + 加金币,
            //    所以不需要先 UnequipBall。
            if (TryFindSellZone(data, out _))
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
            if (TryFindBallInventory(data, out _))
            {
                _player.BallManagement.UnequipBall(sourceSlotIndex);
                return;
            }
        }

        // ------- 释放目标识别 helpers -------

        bool TryFindSellZone(UIDragReleaseEventData data, out GameObject sellZoneGO)
        {
            sellZoneGO = null;
            if (data == null) return false;
            var sellRoot = _panel.Shop.SellZoneRoot;
            if (sellRoot == null) return false;
            var sellTransform = sellRoot.getGameObject().transform;

            foreach (var go in data.GameObjects)
            {
                if (go == null) continue;
                if (go.transform == sellTransform || go.transform.IsChildOf(sellTransform))
                {
                    sellZoneGO = go;
                    return true;
                }
            }
            return false;
        }

        bool TryFindSlotIndex(UIDragReleaseEventData data, out int slotIndex)
        {
            slotIndex = -1;
            if (data == null) return false;

            // 拿当前所有激活的 slot item 列表（顺序 = BallSlotGroup.Slots 的顺序）
            var slots = _player?.BallManagement?.Slots;
            if (slots == null) return false;

            int i = 0;
            foreach (var slot in slots.Slots)
            {
                if (_panel.PlayerInfo.SlotGroup.GetUsedItem(i, out var slotItem) && slotItem != null)
                {
                    var t = slotItem.ItemGO != null ? slotItem.ItemGO.transform : null;
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

        bool TryFindBallInventory(UIDragReleaseEventData data, out BallInventoryItem found)
        {
            found = null;
            if (data == null) return false;
            var bag = _player?.Inventory?.BallBag;
            if (bag == null) return false;

            int i = 0;
            // 按 BallBag 的 slot 顺序遍历（slot.Item == null 也占位），与 View 的 item 顺序一一对应。
            foreach (var slot in bag.SlotList)
            {
                if (_panel.BallInventory.GetUsedItem(i, out var item) && item != null)
                {
                    var t = item.ItemGO != null ? item.ItemGO.transform : null;
                    if (t != null && HitContains(t, data))
                    {
                        found = item;
                        return true;
                    }
                }
                i++;
            }
            return false;
        }

        /// <summary>检查 target transform 是否在释放命中 GameObject 列表里(自己或子节点)。</summary>
        static bool HitContains(Transform target, UIDragReleaseEventData data)
        {
            if (target == null || data == null) return false;
            foreach (var go in data.GameObjects)
            {
                if (go == null) continue;
                var t = go.transform;
                if (t == null) continue;
                if (t == target || t.IsChildOf(target))
                    return true;
            }
            return false;
        }

        // ============================================================
        //                 按钮 / 系统操作
        // ============================================================

        void OnEquipBallRequested(BallItem ball, int slotIndex)
        {
            if (_player == null)
                return;

            // slotIndex 是 UI 调用方传入的目标槽；缺省时回退到 binder 当前选中的槽
            int target = slotIndex >= 0 ? slotIndex : _slotBinder.SelectedSlotIndex;
            if (target < 0)
            {
                // 没指定槽位也没选中槽位 → 走 EquipFirstEmpty
                _player.BallManagement.EquipBall(ball);
            }
            else
            {
                _player.BallManagement.EquipBall(ball, target);
            }

            _ballInv.ClearSelection();
        }

        void OnSlotSelectionChanged(int slotIndex)
        {
            // 槽位被选中后,后续"装备"按钮会以该 slot 为目标
            // 单一 source of truth:BallSlotGroupBinder.SelectedSlotIndex。
        }

        void OnUpgradeBallRequested(BallItem ball)
        {
            if (_player == null)
                return;

            // 简化:调用方接 list 后再做。我们先把单球升级视作"无效"返回。
            var candidates = new List<BallItem> { ball };
            _player.BallManagement.Upgrade.TryUpgrade(candidates, out _);
            _ballInv.ClearSelection();
        }

        void OnSellBallRequested(BallItem ball)
        {
            if (_player == null)
                return;

            _player.Shop.Controller.OnPlayerSellBall(_player, ball);
            _ballInv.ClearSelection();
        }

        void OnSellRelicRequested(RelicItem relic)
        {
            if (_player == null)
                return;

            _player.Shop.Controller.OnPlayerSellRelic(relic);
            _relicInv.ClearSelection();
        }

        void OnShopBuyRequested(IPurchasable offer)
        {
            if (_player == null || offer == null)
                return;

            // 由 ShopController 暴露的购买接口(依 kind 分发):
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

                // 反射创建 ARelic；要求 RelicDef.RelicTypeName 有效(策划需填)
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
            // 暂未实现 BuyExp action,触发事件给接入方
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
