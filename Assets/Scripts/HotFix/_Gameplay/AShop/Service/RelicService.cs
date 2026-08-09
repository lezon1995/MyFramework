using System;

namespace MoreMountains
{
    /// <summary>
    /// 遗物相关的轻量服务。
    /// 主要是把 RelicDef 包装成 RelicItem（背后指向真实 ARelic 实例），
    /// 从而 RelicBag 只需要 RelicItem 一种类型。
    /// 实际 ARelic 子类创建由本服务通过反射实现，匹配 RelicDef.RelicTypeName。
    /// </summary>
    public static class RelicService
    {
        /// <summary>
        /// 由商店流程在玩家购买时调用。
        /// 创建 ARelic → 包装成 RelicItem。
        /// </summary>
        public static RelicItem CreateItem(RelicDef def)
        {
            if (def == null) 
                return null;

            var relic = RelicLibrary.getRelic(def.Type);
            if (relic == null)
            {
                logError($"RelicService: cannot create relic, RelicTypeName missing: {def.Type}");
                return null;
            }

            int refund = def.SellRefund > 0 ? def.SellRefund : Math.Max(1, def.BasePrice / 2);
            return RelicItem.New(def, relic);
        }
    }
}