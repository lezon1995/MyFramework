using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// 遗物 def（SO）。当前项目里遗物是用 ARelic 类层级 + JSON 注册实现，
    /// 这里给出 SO 入口，方便策划在 Inspector 配商店面板的"可售遗物池"。
    /// RelicService 可以用这层把可售遗物包装成 RelicItem 入包。
    /// </summary>
    [CreateAssetMenu(menuName = "MyFramework/Gameplay/RelicDef")]
    public sealed class RelicDef : ScriptableObject
    {
        public int RelicDefId;
        public string DisplayName = "Relic";
        public int BasePrice = 50;        // 商店售价
        public int SellRefund = 25;       // 售出回收价（也可由 Seller 配置比例）
        public Sprite Icon;
        /// <summary>对应 ARelic 子类的 Type 名（FullName）。RelicService 据此反射创建。</summary>
        public string RelicTypeName;
    }
}
