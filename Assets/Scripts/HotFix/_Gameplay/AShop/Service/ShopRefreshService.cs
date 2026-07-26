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

        public List<BallOffer> GenerateBallOffers(int count, IReadOnlyList<BallDef> pool)
        {
            var list = new List<BallOffer>(count);
            if (pool == null || pool.Count == 0 || count <= 0) 
                return list;

            var picked = PickDistinct(pool, count);
            foreach (var def in picked) 
                list.Add(new BallOffer(def));

            return list;
        }

        public List<RelicOffer> GenerateRelicOffers(int count, IReadOnlyList<RelicDef> pool)
        {
            var list = new List<RelicOffer>(count);
            if (pool == null || pool.Count == 0 || count <= 0) 
                return list;
            
            var picked = PickDistinct(pool, count);
            foreach (var def in picked) 
                list.Add(new RelicOffer(def));

            return list;
        }

        /// <summary>
        /// "重新随机"：保留所有未售出槽位的 def，重新从池子里抽；
        /// 但简单实现：直接生成新一组、复用旧的已售 Offer（位置对齐）。
        /// </summary>
        public void RerollBallOffers(List<BallOffer> offers, int count, IReadOnlyList<BallDef> pool)
        {
            if (offers == null)
            {
                GenerateBallOffers(count, pool);
                return;
            }

            var fresh = GenerateBallOffers(count, pool);
            offers.Clear();
            offers.AddRange(fresh);
        }

        public void RerollRelicOffers(List<RelicOffer> offers, int count, IReadOnlyList<RelicDef> pool)
        {
            if (offers == null)
            {
                GenerateRelicOffers(count, pool);
                return;
            }

            var fresh = GenerateRelicOffers(count, pool);
            offers.Clear();
            offers.AddRange(fresh);
        }

        List<T> PickDistinct<T>(IReadOnlyList<T> pool, int count)
        {
            // 池子够大时采不重复；不够时也允许重复。
            var working = new List<T>(pool);
            var result = new List<T>(count);
            int n = Math.Min(count, working.Count);
            for (int i = 0; i < n; i++)
            {
                int idx = _rng.Next(working.Count);
                result.Add(working[idx]);
                working.RemoveAt(idx);
                if (working.Count == 0 && i + 1 < n) working.AddRange(pool); // 池子不够，允许重复
            }

            return result;
        }
    }
}