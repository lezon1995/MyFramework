using System.Collections.Generic;
using static FrameBaseUtility;

namespace MoreMountains
{
    /// <summary>
    /// 商店主控制器 —— 状态机 Idle → ShowingBallBoard → ShowingRelicBoard → Done。
    /// 由 WaveSystem 在阶段 = Shopping 时调用 EnterShop()；
    /// 玩家点 "下一步" 切到下一阶段；
    /// 售出 / 购买 / 重新随机都由 UI 调下面的方法。
    /// </summary>
    public sealed class ShopController
    {
        readonly ShopRefreshService _refresh;
        readonly List<BallOffer>  _ballOffers  = new();
        readonly List<RelicOffer> _relicOffers = new();

        public ShopState State { get; private set; } = ShopState.Idle;
        public IReadOnlyList<BallOffer>  BallOffers  => _ballOffers;
        public IReadOnlyList<RelicOffer> RelicOffers => _relicOffers;

        public ShopBoardKind CurrentBoardKind => State switch
        {
            ShopState.ShowingBallBoard  => ShopBoardKind.Ball,
            ShopState.ShowingRelicBoard => ShopBoardKind.Relic,
            _ => ShopBoardKind.Ball,
        };

        public ShopController(ShopRefreshService refresh = null)
        {
            _refresh = refresh ?? new ShopRefreshService();
        }

        public void EnterShop()
        {
            if (State != ShopState.Idle) ExitShopInternal(raiseClosed: false);

            State = ShopState.ShowingBallBoard;
            ShopEvents.RaiseShopOpened();
            OpenBoard(ShopBoardKind.Ball);
        }

        public void OpenBoard(ShopBoardKind kind)
        {
            var cfg = ShopSystemConfig.Instance;
            if (cfg == null) { logError("ShopController: missing ShopSystemConfig"); return; }

            if (kind == ShopBoardKind.Ball)
            {
                _ballOffers.Clear();
                _ballOffers.AddRange(_refresh.GenerateBallOffers(cfg.BallOfferCount, cfg.BallOfferPool));
                State = ShopState.ShowingBallBoard;
                ShopEvents.RaiseBoardOpened(ShopBoardKind.Ball);
            }
            else
            {
                _relicOffers.Clear();
                _relicOffers.AddRange(_refresh.GenerateRelicOffers(cfg.RelicOfferCount, cfg.RelicOfferPool));
                State = ShopState.ShowingRelicBoard;
                ShopEvents.RaiseBoardOpened(ShopBoardKind.Relic);
            }
        }

        public bool OnPlayerClickReroll()
        {
            var cfg = ShopSystemConfig.Instance;
            if (cfg == null) return false;

            int cost = 0;
            if (State == ShopState.ShowingBallBoard)
            {
                cost = cfg.BallBoardRerollCost;
                if (!PlayerWallet.Instance.CanPay(cost)) { logWarning("金币不足以重新随机"); return false; }
                PlayerWallet.Instance.Pay(cost, "shop_reroll_ball");
                _refresh.RerollBallOffers(_ballOffers, cfg.BallOfferCount, cfg.BallOfferPool);
                ShopEvents.RaiseBoardRerolled(ShopBoardKind.Ball);
                return true;
            }
            if (State == ShopState.ShowingRelicBoard)
            {
                cost = cfg.RelicBoardRerollCost;
                if (!PlayerWallet.Instance.CanPay(cost)) { logWarning("金币不足以重新随机"); return false; }
                PlayerWallet.Instance.Pay(cost, "shop_reroll_relic");
                _refresh.RerollRelicOffers(_relicOffers, cfg.RelicOfferCount, cfg.RelicOfferPool);
                ShopEvents.RaiseBoardRerolled(ShopBoardKind.Relic);
                return true;
            }
            return false;
        }

        public bool OnPlayerClickNext()
        {
            if (State == ShopState.ShowingBallBoard)
            {
                OpenBoard(ShopBoardKind.Relic);
                return true;
            }
            if (State == ShopState.ShowingRelicBoard)
            {
                FinishShopAndNotify();
                return true;
            }
            return false;
        }

        public void FinishShopAndNotify()
        {
            ExitShopInternal(raiseClosed: true);
            // 通过 WaveSystem 的入口推进阶段；这里只解耦为统一命令
            // WaveBridge 是一个轻量胶水，请按你项目的实际阶段名替换 GetNextPhaseName()
            WaveBridge.RequestNextPhaseAfterShop();
        }

        void ExitShopInternal(bool raiseClosed)
        {
            _ballOffers.Clear();
            _relicOffers.Clear();
            State = ShopState.Done;
            if (raiseClosed) ShopEvents.RaiseShopClosed();
        }

        // ------------- 出售 -------------

        /// <summary>出售球 — 走球管理服务的 SellToShop 自身事务。</summary>
        public int OnPlayerSellBall(BallInstance ball)
        {
            if (BallManagementSystem.Instance == null) return 0;
            int gold = BallManagementSystem.Instance.Shop.SellToShop(ball);
            if (gold > 0) ShopEvents.RaiseSoldFromBag(ball);
            return gold;
        }

        public int OnPlayerSellRelic(RelicItem item)
        {
            if (item == null) return 0;
            int gold = item.SellPrice;
            if (InventorySystem.Instance != null) InventorySystem.Instance.RemoveRelic(item);
            PlayerWallet.Instance.Earn(gold, "relic_sell");
            ShopEvents.RaiseGoldEarned(gold, "relic_sell");
            ShopEvents.RaiseSoldFromBag(item);
            return gold;
        }
    }

    /// <summary>
    /// WaveSystem 推进阶段的胶水 —— 避免 ShopController 直接依赖 WaveManager 类型。
    /// 项目接入时，把 WaveBridge.RequestNextPhaseAfterShop() 的实现替换为：
    ///   WaveManager.Instance.SetPhase(RoomPhaseType.BATTLE, reason: "shopping_done");
    /// 或者你自己项目里推进阶段的命令。
    /// 当前默认实现为空，由接入方替换。
    /// </summary>
    public static class WaveBridge
    {
        public static System.Action OnShopPhaseFinished;

        public static void RequestNextPhaseAfterShop()
        {
            OnShopPhaseFinished?.Invoke();
        }
    }
}
