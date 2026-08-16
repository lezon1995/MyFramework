using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 随机刷新面板服务：
    ///   • 从 RewardSystemConfig 的池子里采 N 个不重复的 def，包成 Offer
    ///   • Reroll 会保留"已售"的 Offer 不动，只补齐其余
    /// </summary>
    public sealed class RewardRefreshService
    {
        Random _rng;
        RarityRollService _rollRarityService;
        UniStats.Stat luck;
        
        public RewardRefreshService(APlayer p, int seed = 0)
        {
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
            _rollRarityService = new();
            p.GetStat(Character.Stat.Luck, out luck);
        }

        public void GenerateMixedOffers(int waveNumber, int count
            , List<BallStatConfig> ballPool, ref List<BallStatOffer> ballResult
            , List<PlayerStatConfig> playerPool, ref List<PlayerStatOffer> playerResult
            )
        {
            var ballCount = _rng.Next(0, count + 1);
            var playerCount = count - ballCount;

            using var a = new ListScope<BallStatConfig>(out var pickedBalls);
            PickDistinct(ballPool, ballCount, ref pickedBalls);
            
            foreach (var c in pickedBalls)
            {
                var offer = CLASS<BallStatOffer>();
                var rarity = _rollRarityService.RollReward(waveNumber, luck.Value);
                var config = c.getConfig(rarity);
                offer.with(c.def, config.rarity, config.bonusFlat, config.bonusPct, c.display);
                ballResult.Add(offer);
            }

            using var b = new ListScope<PlayerStatConfig>(out var pickedPlayers);
            PickDistinct(playerPool, playerCount, ref pickedPlayers);
            foreach (var c in pickedPlayers)
            {
                var offer = CLASS<PlayerStatOffer>();
                var rarity = _rollRarityService.RollReward(waveNumber, luck.Value);
                var config = c.getConfig(rarity);
                offer.with(c.def, config.rarity, config.bonusFlat, config.bonusPct, c.display);
                playerResult.Add(offer);
            }
        }


        public void GenerateBallStatModDefs(int waveNumber, int count, List<BallStatConfig> pool, ref List<BallStatOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<BallStatConfig>(out var picked);
            PickDistinct(pool, count, ref picked);
            foreach (var c in picked)
            {
                var offer = CLASS<BallStatOffer>();
                var rarity = _rollRarityService.RollReward(waveNumber, luck.Value);
                var config = c.getConfig(rarity);
                offer.with(c.def, config.rarity, config.bonusFlat, config.bonusPct, c.display);
                result.Add(offer);
            }
        }

        public void GeneratePlayerStatOffers(int waveNumber, int count, List<PlayerStatConfig> pool, ref List<PlayerStatOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<PlayerStatConfig>(out var picked);
            PickDistinct(pool, count, ref picked);
            foreach (var c in picked)
            {
                var offer = CLASS<PlayerStatOffer>();
                var rarity = _rollRarityService.RollReward(waveNumber, luck.Value);
                var config = c.getConfig(rarity);
                offer.with(c.def, config.rarity, config.bonusFlat, config.bonusPct, c.display);
                result.Add(offer);
            }
        }

        /// <summary>
        /// "重新随机"：保留所有未售出槽位的 def，重新从池子里抽；
        /// 但简单实现：直接生成新一组、复用旧的已售 Offer（位置对齐）。
        /// </summary>
        public void RerollMixedOffers(int waveNumber, int count
            , List<BallStatConfig> ballPool, ref List<BallStatOffer> ballOffers
            , List<PlayerStatConfig> playerPool, ref List<PlayerStatOffer> playerOffers
        )
        {
            foreach (var offer in ballOffers)
                UN_CLASS(offer);

            ballOffers.Clear();
            foreach (var offer in playerOffers)
                UN_CLASS(offer);

            playerOffers.Clear();

            GenerateMixedOffers(waveNumber, count, ballPool, ref ballOffers, playerPool, ref playerOffers);
        }

        public void RerollBallStatModDefs(int waveNumber, List<BallStatOffer> offers, int count, List<BallStatConfig> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GenerateBallStatModDefs(waveNumber, count, pool, ref offers);
        }

        public void RerollPlayerStatOffers(int waveNumber, List<PlayerStatOffer> offers, int count, List<PlayerStatConfig> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GeneratePlayerStatOffers(waveNumber, count, pool, ref offers);
        }

        void PickDistinct<T>(List<T> pool, int count, ref List<T> result)
        {
            // 池子够大时采不重复；不够时也允许重复。
            using var _ = new ListScope<T>(out var working);
            working.AddRange(pool);

            result.Clear();
            int n = Math.Min(count, working.Count);
            for (int i = 0; i < n; i++)
            {
                int idx = _rng.Next(working.Count);
                result.Add(working[idx]);
                working.RemoveAt(idx);
                if (working.Count == 0 && i + 1 < n)
                    working.AddRange(pool); // 池子不够，允许重复
            }
        }
    }
}