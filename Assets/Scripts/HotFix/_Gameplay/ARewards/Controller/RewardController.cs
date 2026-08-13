using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 升级奖励控制器 —— 状态机 Idle → ShowingBallBoard → ShowingRelicBoard → Done。
    /// 由 WaveSystem 在阶段 = Shopping 时调用 EnterShop()；
    /// 玩家点 "下一步" 切到下一阶段；
    /// 售出 / 购买 / 重新随机都由 UI 调下面的方法。
    /// </summary>
    public class RewardController
    {
        RewardSystem rewardSystem;
        RewardRefreshService _refresh;
        List<BallStatOffer> _ballStatOffers = new();
        List<PlayerStatOffer> _playerStatOffers = new();

        public int WaveNumber { get; private set; }
        public RewardSystemState State { get; private set; } = RewardSystemState.Idle;
        public List<BallStatOffer> BallStatOffers => _ballStatOffers;
        public List<PlayerStatOffer> PlayerStatOffers => _playerStatOffers;

        public RewardBoardKind CurrentBoardKind => State switch
        {
            RewardSystemState.ShowingMixedBoard => RewardBoardKind.Mixed,
            RewardSystemState.ShowingBallStatBoard => RewardBoardKind.BallStat,
            RewardSystemState.ShowingPlayerStatBoard => RewardBoardKind.PlayerStat,
            _ => RewardBoardKind.BallStat,
        };

        public RewardController(RewardSystem system, RewardRefreshService refresh = null)
        {
            rewardSystem = system;
            _refresh = refresh ?? new RewardRefreshService();
        }

        public void EnterReward(int waveNumber)
        {
            if (State != RewardSystemState.Idle)
                ExitRewardInternal(raiseClosed: false);

            WaveNumber =  waveNumber;
            State = RewardSystemState.ShowingMixedBoard;
            RewardEvents.RaiseRewardOpened();
            OpenBoard(RewardBoardKind.Mixed);
        }

        public void OpenBoard(RewardBoardKind kind)
        {
            var cfg = RewardSystemConfig.Instance;
            if (cfg == null)
            {
                logError("RewardController: missing RewardSystemConfig");
                return;
            }

            switch (kind)
            {
                case RewardBoardKind.Mixed:
                {
                    foreach (var offer in _ballStatOffers)
                        UN_CLASS(offer);

                    _ballStatOffers.Clear();

                    foreach (var offer in _playerStatOffers)
                        UN_CLASS(offer);

                    _playerStatOffers.Clear();

                    _refresh.GenerateMixedOffers(WaveNumber, cfg.MixedOfferCount, cfg.BallStatModOfferPool, ref _ballStatOffers, cfg.PlayerStatModOfferPool, ref _playerStatOffers);
                    State = RewardSystemState.ShowingMixedBoard;
                    RewardEvents.RaiseBoardOpened(RewardBoardKind.Mixed);
                    break;
                }
                case RewardBoardKind.BallStat:
                {
                    foreach (var offer in _ballStatOffers)
                        UN_CLASS(offer);

                    _ballStatOffers.Clear();
                    _refresh.GenerateBallStatModDefs(WaveNumber, cfg.BallOfferCount, cfg.BallStatModOfferPool, ref _ballStatOffers);
                    State = RewardSystemState.ShowingBallStatBoard;
                    RewardEvents.RaiseBoardOpened(RewardBoardKind.BallStat);
                    break;
                }
                case RewardBoardKind.PlayerStat:
                {
                    foreach (var offer in _playerStatOffers)
                        UN_CLASS(offer);

                    _playerStatOffers.Clear();
                    _refresh.GeneratePlayerStatOffers(WaveNumber, cfg.RelicOfferCount, cfg.PlayerStatModOfferPool, ref _playerStatOffers);
                    State = RewardSystemState.ShowingPlayerStatBoard;
                    RewardEvents.RaiseBoardOpened(RewardBoardKind.PlayerStat);
                    break;
                }
            }
        }

        public bool OnPlayerClickReroll()
        {
            var cfg = RewardSystemConfig.Instance;
            if (cfg == null)
                return false;

            int cost;
            switch (State)
            {
                case RewardSystemState.ShowingMixedBoard:
                {
                    cost = cfg.MixedBoardRerollCost;
                    if (!rewardSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    rewardSystem.Player.loseGold(cost, PayType.MIXED_REROLL);
                    _refresh.RerollMixedOffers(WaveNumber, cfg.MixedOfferCount, cfg.BallStatModOfferPool, ref _ballStatOffers, cfg.PlayerStatModOfferPool, ref _playerStatOffers);
                    RewardEvents.RaiseBoardRerolled(RewardBoardKind.Mixed);
                    return true;
                }

                case RewardSystemState.ShowingBallStatBoard:
                {
                    cost = cfg.BallBoardRerollCost;
                    if (!rewardSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    rewardSystem.Player.loseGold(cost, PayType.BALL_REROLL);
                    _refresh.RerollBallStatModDefs(WaveNumber, _ballStatOffers, cfg.BallOfferCount, cfg.BallStatModOfferPool);
                    RewardEvents.RaiseBoardRerolled(RewardBoardKind.BallStat);
                    return true;
                }
                case RewardSystemState.ShowingPlayerStatBoard:
                {
                    cost = cfg.RelicBoardRerollCost;
                    if (!rewardSystem.Player.Wallet.CanPay(cost))
                    {
                        logWarning("金币不足以重新随机");
                        return false;
                    }

                    rewardSystem.Player.loseGold(cost, PayType.RELIC_REROLL);
                    _refresh.RerollPlayerStatOffers(WaveNumber, _playerStatOffers, cfg.RelicOfferCount, cfg.PlayerStatModOfferPool);
                    RewardEvents.RaiseBoardRerolled(RewardBoardKind.PlayerStat);
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
                case RewardSystemState.ShowingBallStatBoard:
                    OpenBoard(RewardBoardKind.PlayerStat);
                    return true;
                case RewardSystemState.ShowingPlayerStatBoard:
                    FinishRewardAndNotify();
                    return true;
                case RewardSystemState.ShowingMixedBoard:
                    FinishRewardAndNotify();
                    return true;
                default:
                    return false;
            }
        }

        public void FinishRewardAndNotify()
        {
            ExitRewardInternal(raiseClosed: true);
            // 通过 WaveSystem 的入口推进阶段；这里只解耦为统一命令
            // WaveBridge 是一个轻量胶水，请按你项目的实际阶段名替换 GetNextPhaseName()
            WaveBridge.RequestNextPhaseAfterShop();
        }

        void ExitRewardInternal(bool raiseClosed)
        {
            foreach (var offer in _ballStatOffers)
                UN_CLASS(offer);

            _ballStatOffers.Clear();
            foreach (var offer in _playerStatOffers)
                UN_CLASS(offer);

            _playerStatOffers.Clear();
            State = RewardSystemState.Done;
            if (raiseClosed)
                RewardEvents.RaiseRewardClosed();
        }
    }
}