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
            if (def == null) return null;
            var relic = CreateRelicByTypeName(def.RelicTypeName, def.DisplayName, def.RelicDefId);
            if (relic == null)
            {
                logError($"RelicService: cannot create relic, RelicTypeName missing: {def.RelicTypeName}");
                return null;
            }

            int refund = def.SellRefund > 0 ? def.SellRefund : Math.Max(1, def.BasePrice / 2);
            return new RelicItem(def);
        }

        static ARelic CreateRelicByTypeName(string typeName, string displayName, int defId)
        {
            if (string.IsNullOrEmpty(typeName)) return null;
            var type = Type.GetType(typeName) ?? Type.GetType(typeName + ", Assembly-CSharp");
            if (type == null) return null;
            try
            {
                if (Activator.CreateInstance(type) is ARelic relic)
                {
                    relic.relicId = displayName ?? relic.relicId ?? type.Name;
                    relic.name = displayName ?? relic.name ?? relic.relicId;
                    relic.cost = defId;
                    return relic;
                }
            }
            catch (Exception ex)
            {
                logError($"RelicService.CreateRelicByTypeName failed: {ex.Message}");
            }

            return null;
        }
    }
}