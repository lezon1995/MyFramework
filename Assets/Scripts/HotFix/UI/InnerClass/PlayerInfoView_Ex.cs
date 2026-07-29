using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public partial class PlayerInfoView
{
    public void SetLevel(int lv)
    {
        textLevel.setText(IToS(lv));
    }

    public void SetExp(int cur, int max)
    {
        textCurExp.setText(IToS(cur));
        textMaxExp.setText(IToS(max));
        if (expSlider != null)
        {
            float v = max > 0 ? Mathf.Clamp01((float)cur / max) : 0f;
            expSlider.setValue(v);
        }
    }

    public myUGUIObject StatListRoot => itemParent;

    public void BuildPlayerStats<TData>(IList<TData> dataList, Action<PlayerStatItem, TData> onBuild)
    {
        var list = dataList is List<TData> l ? l : new List<TData>(dataList);
        PlayerStatItemPool.newItemList(list, (item, data) => onBuild?.Invoke(item, data));
    }

    public BallSlotGroupView SlotGroup => ballSlotGroupView;
}

public partial class PlayerStatItem
{
    public void SetIcon(Sprite s)
    {
        /* statIcon 占位，预留贴图迭代阶段使用 */
    }

    public void SetName(string s)
    {
        /* statName 占位 */
    }

    public void SetValue(string s)
    {
        /* statValue 占位 */
    }
}