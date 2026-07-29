// partial 扩展：把 protected 字段升级成 setter（在 binder 里能写显示用）
// 不动 auto generate 内容；只新增字段读写方法与池填充 API。

using System;
using System.Collections.Generic;

namespace MoreMountains;

public partial class ShopView
{
    public void SetTitle(string s) => shopTitle.setText(s ?? string.Empty);
    public void SetRemainCoin(int n) => remainCoin.setText(IToS(n));
    public myUGUIObject ShopItemsRoot => shopItems;
    public myUGUIObject SellZoneRoot => sellZone;
    public myUGUIButton BtnReroll => btnReroll;
    public myUGUIButton BtnBuyExp => btnBuyExp;

    /// <summary>把现有的 UI 项全部回收，然后按 data 列表重建。每项回调里把数据塞到 UI。</summary>
    public void BuildBallOffers<TData>(IList<TData> dataList, Action<BallPurchaseItem, TData> onBuild)
    {
        var list = dataList is List<TData> l ? l : new List<TData>(dataList);
        BallPurchaseItemPool.newItemList(list, (item, data) => onBuild?.Invoke(item, data));
    }

    public void BuildRelicOffers<TData>(IList<TData> dataList, Action<RelicPurchaseItem, TData> onBuild)
    {
        var list = dataList is List<TData> l ? l : new List<TData>(dataList);
        RelicPurchaseItemPool.newItemList(list, (item, data) => onBuild?.Invoke(item, data));
    }
}