// partial 扩展:ShopView 的 sellZone 跟随球操作状态显隐。
// 由于 ShopView 不是 MonoBehaviour,事件订阅和 Start 逻辑都放在 ShopBinder 中。

using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class ShopView
{
    public void SetTitle(string s) => shopTitle?.setText(s ?? string.Empty);
    public void SetRemainCoin(int n) => remainCoin?.setText(n.IToS());
    public myUGUIObject ShopItemsRoot => shopItems;
    public ShopSellZoneView SellZoneRoot => shopSellZoneView;
    public myUGUIButton BtnReroll => btnReroll;
    public myUGUIButton BtnBuyExp => btnBuyExp;

    /// <summary>把现有的 UI 项全部回收,然后按 data 列表重建。每项回调里把数据塞到 UI。</summary>
    public void BuildBallOffers<TData>(List<TData> dataList, Action<BallPurchaseItem, TData> onBuild)
    {
        BallPurchaseItemPool.newItemList(dataList, onBuild);
    }

    public void BuildRelicOffers<TData>(List<TData> dataList, Action<RelicPurchaseItem, TData> onBuild)
    {
        RelicPurchaseItemPool.newItemList(dataList, onBuild);
    }

    ShopBinder binder;

    public ShopBinder initBinder()
    {
        return binder ??= new(this);
    }

    // ==================== 球操作状态中的 sellZone 控制 ====================

    /// <summary>设置 sellZone 是否显示(由 ShopBinder 根据操作状态调用)。</summary>
    public void SetSellZoneVisible(bool visible) => shopSellZoneView.setActive(visible);
    public void SetSellZoneSellPrice(int sellPrice) => shopSellZoneView.SetSellPrice(sellPrice);
}