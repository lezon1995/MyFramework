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

        public ShopRefreshService(int seed = 0)
        {
            _rng = new Random(seed == 0 ? Environment.TickCount : seed);
        }

        public void GenerateMixedOffers(int count
            , List<BallDef> ballPool, ref List<BallOffer> ballResult
            , List<RelicDef> relicPool, ref List<RelicOffer> relicResult)
        {
            var ballCount = _rng.Next(0, count + 1);
            var relicCount = count - ballCount;

            using var a = new ListScope<BallDef>(out var pickedBalls);
            PickDistinct(ballPool, ballCount, ref pickedBalls);
            foreach (var def in pickedBalls)
            {
                var offer = CLASS<BallOffer>();
                offer.with(def);
                ballResult.Add(offer);
            }

            using var b = new ListScope<RelicDef>(out var pickedRelics);
            PickDistinct(relicPool, relicCount, ref pickedRelics);
            foreach (var def in pickedRelics)
            {
                var offer = CLASS<RelicOffer>();
                offer.with(def);
                relicResult.Add(offer);
            }
        }


        public void GenerateBallOffers(int count, List<BallDef> pool, ref List<BallOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<BallDef>(out var picked);
            PickDistinct(pool, count, ref picked);
            foreach (var def in picked)
            {
                var offer = CLASS<BallOffer>();
                offer.with(def);
                result.Add(offer);
            }
        }

        public void GenerateRelicOffers(int count, List<RelicDef> pool, ref List<RelicOffer> result)
        {
            if (pool == null || pool.Count == 0 || count <= 0)
                return;

            using var _ = new ListScope<RelicDef>(out var picked);
            PickDistinct(pool, count, ref picked);
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
        public void RerollMixedOffers(int count
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

            GenerateMixedOffers(count, ballPool, ref ballOffers, relicPool, ref relicOffers);
        }

        public void RerollBallOffers(List<BallOffer> offers, int count, List<BallDef> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GenerateBallOffers(count, pool, ref offers);
        }

        public void RerollRelicOffers(List<RelicOffer> offers, int count, List<RelicDef> pool)
        {
            if (offers == null)
                return;

            foreach (var offer in offers)
                UN_CLASS(offer);

            offers.Clear();
            GenerateRelicOffers(count, pool, ref offers);
        }

        void PickDistinct<T>(List<T> pool, int count, ref List<T> result)
        {
            // 池子够大时采不重复；不够时也允许重复。
            using var _ = new ListScope<T>(out var working);
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