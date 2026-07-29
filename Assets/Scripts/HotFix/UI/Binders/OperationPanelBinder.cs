using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// OperationPanelBinder —— 持有并协调所有子 binder。
    ///
    /// 职责：
    ///   • Bind(APlayer)：把玩家四大系统（BallManagement / Inventory / Wallet / Shop）注入子 binder。
    ///   • Open() / Close()：被 ShoppingPhase.onBegin/onEnd 调用。
    ///   • 处理各 button（Btns 在 OperationPanel 上的 onClick 还没指定 -- 模式：OperationPanel 暴露 Action；
    ///     当前 binder 监听 ShopBinder 的事件，向上抛响应供外部接入按钮）。
    ///
    /// 注意：OperationPanel 是 LayoutScript 的子类，需要在 UI 系统里实例化后才能 assignWindow。
    /// 这里持 IObservedView 引用（实际是 OperationPanel 实例），由外部传入。
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

            // 监听子 binder 的事件，把 UI 操作翻译成对系统的命令
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

        // ------------- 系统操作映射 -------------

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
            // 单一 source of truth：BallSlotGroupBinder.SelectedSlotIndex。
        }

        void OnUpgradeBallRequested(BallItem ball)
        {
            if (_player == null) 
                return;

            // 升级：升级服务 TryUpgrade 期望多个 candidate；这里传入单个，由调用方选择 N-up-1 校验
            // 简化：调用方接 list 后再做。我们先把单球升级视作"无效"返回。
            // 实际应让 UI 多选模式下把 candidate list 传进来：此处仅暴露事件。
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

            // 由 ShopController 暴露的购买接口（依 kind 分发）：
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

                // 反射创建 ARelic；要求 RelicDef.RelicTypeName 有效（策划需填）
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

                var ritem = new RelicItem(underlying, sellPrice: relicOffer.Def.SellRefund);
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
            // 暂未实现 BuyExp action，触发事件给接入方
        }

        void OnWalletChanged(int _) => _shop.RefreshCoin();

        void NextStage()
        {
            _player?.Shop?.Controller.OnPlayerClickNext();
        }

        public APlayer Player => _player;
    }
}