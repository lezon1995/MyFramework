// partial 扩展:ShopView 的 sellZone 跟随球操作状态显隐。
// 由于 ShopView 不是 MonoBehaviour,事件订阅和 Start 逻辑都放在 ShopBinder 中。

using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace MoreMountains;

public partial class RewardChooseView
{
    public void SetTitle(string s) => textTitle?.setText(s ?? string.Empty);
    public myUGUIObject RewardItemsRoot => rewardItems;
    public myUGUIButton BtnReroll => btnReroll;

    /// <summary>把现有的 UI 项全部回收,然后按 data 列表重建。每项回调里把数据塞到 UI。</summary>
    public void BuildBallStatOffers<TData>(List<TData> dataList, Action<RewardChooseItem, TData> onBuild)
    {
        RewardChooseItemPool.newItemList(dataList, onBuild);
    }

    public void BuildPlayerStatOffers<TData>(List<TData> dataList, Action<RewardChooseItem, TData> onBuild)
    {
        RewardChooseItemPool.newItemList(dataList, onBuild);
    }

    RewardChooseBinder binder;

    public RewardChooseBinder initBinder()
    {
        return binder ??= new(this);
    }
}