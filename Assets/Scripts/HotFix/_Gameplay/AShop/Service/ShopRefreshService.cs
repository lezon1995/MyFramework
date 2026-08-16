using System;
using System.Collections.Generic;

namespace MoreMountains
{
    /// <summary>
    /// 随机刷新面板服务：
    ///   • 从 ShopSystemConfig 的池子里采 N 个不重复的 def，包成 Offer
    ///   • Reroll 会保留"已售"的 Offer 不动，只补齐其余
    /// </summary>
    public sealed class ShopRefreshService
    {
        Random _rng;
        RarityRollService _rollRarityService;
        UniStats.Stat luck;

        public ShopRefreshService(APlayer p, int seed = 0)
        {
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
            _rollRarityService = new();
            p.GetStat(Character.Stat.Luck, out luck);
        }

        public void GenerateMixedOffers(int waveNumber, int count
            , List<BallDef> ballPool, ref List<BallOffer> ballResult
            , List<RelicDef> relicPool, ref List<RelicOffer> relicResult)
        {
            var ballCount = _rng.Next(0, count + 1);
            var relicCount = count - ballCount;

            using var a = new ListScope<BallDef>(out var pickedBalls);
            PickDistinct(waveNumber, ballPool, ballCount, ref pickedBalls);
            foreach (var def in pickedBalls)
            {
                var offer = CLASS<BallOffer>();
                offer.with(def);
                ballResult.Add(offer);
            }

            using var b = new ListScope<RelicDef>(out var pickedRelics);
            PickDistinct(waveNumber, relicPool, relicCount, ref pickedRelics);
            foreach (var def in pickedRelics)
            {
                var offer = CLASS<RelicOffer>();
                offer.with(def);
                relicResult.Add(offer);
            }
        }


        public void GenerateBallOffers(int waveNumber, int count, List<BallDef> pool, ref List<BallOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<BallDef>(out var picked);
            PickDistinct(waveNumber, pool, count, ref picked);
            foreach (var def in picked)
            {
                var offer = CLASS<BallOffer>();
                offer.with(def);
                result.Add(offer);
            }
        }

        public void GenerateRelicOffers(int waveNumber, int count, List<RelicDef> pool, ref List<RelicOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<RelicDef>(out var picked);
            PickDistinct(waveNumber, pool, count, ref picked);
            foreach (var def in picked)
            {
                var offer = CLASS<RelicOffer>();
                offer.with(def);
                result.Add(offer);
            }
        }

        /// <summary>
        /// "重新随机"：保留所有未售出槽位的 def，重新从池子里抽；
        /// 但简单实现：直接生成新一组、复用旧的已售 Offer（位置对齐）。
        /// </summary>
        public void RerollMixedOffers(int waveNumber, int count
            , List<BallDef> ballPool, ref List<BallOffer> ballOffers
            , List<RelicDef> relicPool, ref List<RelicOffer> relicOffers
        )
        {
            foreach (var offer in ballOffers)
                UN_CLASS(offer);

            ballOffers.Clear();
            foreach (var offer in relicOffers)
                UN_CLASS(offer);

            relicOffers.Clear();

            GenerateMixedOffers(waveNumber, count, ballPool, ref ballOffers, relicPool, ref relicOffers);
        }

        public void RerollBallOffers(int waveNumber, List<BallOffer> offers, int count, List<BallDef> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GenerateBallOffers(waveNumber, count, pool, ref offers);
        }

        public void RerollRelicOffers(int waveNumber, List<RelicOffer> offers, int count, List<RelicDef> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GenerateRelicOffers(waveNumber, count, pool, ref offers);
        }

        void PickDistinct<T>(int waveNumber, List<T> pool, int count, ref List<T> result) where T : IRarityObject
        {
            result.Clear();
            int n = count;
            for (int i = 0; i < n; i++)
            {
                var rarity = _rollRarityService.RollItem(waveNumber, luck.Value);

                // 池子够大时采不重复；不够时也允许重复。
                using var _ = new ListScope<T>(out var working);
                foreach (var o in pool)
                {
                    if (o.rarity == rarity)
                        working.Add(o);
                }

                int idx = _rng.Next(working.Count);
                result.Add(working[idx]);
            }
        }
    }
}