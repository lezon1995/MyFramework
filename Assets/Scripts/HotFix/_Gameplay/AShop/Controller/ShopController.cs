using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 商店主控制器 —— 状态机 Idle → ShowingBallBoard → ShowingRelicBoard → Done。
    /// 由 WaveSystem 在阶段 = Shopping 时调用 EnterShop()；
    /// 玩家点 "下一步" 切到下一阶段；
    /// 售出 / 购买 / 重新随机都由 UI 调下面的方法。
    /// </summary>
    public class ShopController
    {
        ShopSystem shopSystem;
        ShopRefreshService _refresh;
        List<BallOffer> _ballOffers = new();
        List<RelicOffer> _relicOffers = new();

        public ShopState State { get; private set; } = ShopState.Idle;
        public List<BallOffer> BallOffers => _ballOffers;
        public List<RelicOffer> RelicOffers => _relicOffers;

        public ShopBoardKind CurrentBoardKind => State switch
        {
            ShopState.ShowingMixedBoard => ShopBoardKind.Mixed,
            ShopState.ShowingBallBoard => ShopBoardKind.Ball,
            ShopState.ShowingRelicBoard => ShopBoardKind.Relic,
            _ => ShopBoardKind.Ball,
        };

        public ShopController(ShopSystem system, ShopRefreshService refresh = null)
        {
            shopSystem = system;
            _refresh = refresh ?? new ShopRefreshService();
        }

        public void EnterShop()
        {
            if (State != ShopState.Idle)
                ExitShopInternal(raiseClosed: false);

            State = ShopState.ShowingMixedBoard;
            ShopEvents.RaiseShopOpened();
            OpenBoard(ShopBoardKind.Mixed);
        }

        public void OpenBoard(ShopBoardKind kind)
        {
            var cfg = ShopSystemConfig.Instance;
            if (cfg == null)
            {
                logError("ShopController: missing ShopSystemConfig");
                return;
            }

            switch (kind)
            {
                case ShopBoardKind.Mixed:
                {
                    foreach (var offer in _ballOffers)
                        UN_CLASS(offer);

                    _ballOffers.Clear();

                    foreach (var offer in _relicOffers)
                        UN_CLASS(offer);

                    _relicOffers.Clear();

                    _refresh.GenerateMixedOffers(cfg.MixedOfferCount, cfg.BallOfferPool, ref _ballOffers, cfg.RelicOfferPool, ref _relicOffers);
                    State = ShopState.ShowingMixedBoard;
                    ShopEvents.RaiseBoardOpened(ShopBoardKind.Mixed);
                    break;
                }
                case ShopBoardKind.Ball:
                {
                    foreach (var offer in _ballOffers)
                        UN_CLASS(offer);

                    _ballOffers.Clear();
                    _refresh.GenerateBallOffers(cfg.BallOfferCount, cfg.BallOfferPool, ref _ballOffers);
                    State = ShopState.ShowingBallBoard;
                    ShopEvents.RaiseBoardOpened(ShopBoardKind.Ball);
                    break;
                }
                case ShopBoardKind.Relic:
                {
                    foreach (var offer in _relicOffers)
                        UN_CLASS(offer);

                    _relicOffers.Clear();
                    _refresh.GenerateRelicOffers(cfg.RelicOfferCount, cfg.RelicOfferPool, ref _relicOffers);
                    State = ShopState.ShowingRelicBoard;
                    ShopEvents.RaiseBoardOpened(ShopBoardKind.Relic);
                    break;
                }
            }
        }

        public bool OnPlayerClickReroll()
        {
            var cfg = ShopSystemConfig.Instance;
            if (cfg == null)
                return false;

            int cost;
            switch (State)
            {
                case ShopState.ShowingMixedBoard:
                {
                    cost = cfg.MixedBoardRerollCost;
                    if (!shopSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    shopSystem.Player.loseGold(cost, PayType.MIXED_REROLL);
                    _refresh.RerollMixedOffers(cfg.MixedOfferCount, cfg.BallOfferPool, ref _ballOffers, cfg.RelicOfferPool, ref _relicOffers);
                    ShopEvents.RaiseBoardRerolled(ShopBoardKind.Mixed);
                    return true;
                }

                case ShopState.ShowingBallBoard:
                {
                    cost = cfg.BallBoardRerollCost;
                    if (!shopSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    shopSystem.Player.loseGold(cost, PayType.BALL_REROLL);
                    _refresh.RerollBallOffers(_ballOffers, cfg.BallOfferCount, cfg.BallOfferPool);
                    ShopEvents.RaiseBoardRerolled(ShopBoardKind.Ball);
                    return true;
                }
                case ShopState.ShowingRelicBoard:
                {
                    cost = cfg.RelicBoardRerollCost;
                    if (!shopSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    shopSystem.Player.loseGold(cost, PayType.RELIC_REROLL);
                    _refresh.RerollRelicOffers(_relicOffers, cfg.RelicOfferCount, cfg.RelicOfferPool);
                    ShopEvents.RaiseBoardRerolled(ShopBoardKind.Relic);
                    return true;
                }
                default:
                    return false;
            }
        }

        public bool OnPlayerClickNext()
        {
            switch (State)
            {
                case ShopState.ShowingBallBoard:
                    OpenBoard(ShopBoardKind.Relic);
                    return true;
                case ShopState.ShowingRelicBoard:
                    FinishShopAndNotify();
                    return true;
                case ShopState.ShowingMixedBoard:
                    FinishShopAndNotify();
                    return true;
                default:
                    return false;
            }
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
            foreach (var offer in _ballOffers)
                UN_CLASS(offer);

            _ballOffers.Clear();
            foreach (var offer in _relicOffers)
                UN_CLASS(offer);

            _relicOffers.Clear();
            State = ShopState.Done;
            if (raiseClosed)
                ShopEvents.RaiseShopClosed();
        }

        // ------------- 出售 -------------

        /// <summary>出售球 — 走球管理服务的 SellToShop 自身事务。</summary>
        public int OnPlayerSellBall(APlayer p, BallItem item)
        {
            int gold = p.BallManagement.Shop.SellToShop(item);
            if (gold > 0)
                ShopEvents.RaiseSoldFromBag(item);
            
            BallItem.Release(ref item);
            return gold;
        }

        public int OnPlayerSellRelic(APlayer p, RelicItem item)
        {
            if (item == null)
                return 0;

            int gold = item.SellPrice;

            p.Inventory.RemoveRelic(item);
            p.gainGold(gold, EarnType.SELL_RELIC);
            ShopEvents.RaiseGoldEarned(gold, "relic_sell");
            ShopEvents.RaiseSoldFromBag(item);
            
            RelicItem.Release(ref item);
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
        public static Action OnShopPhaseFinished;

        public static void RequestNextPhaseAfterShop()
        {
            OnShopPhaseFinished?.Invoke();
        }
    }
}