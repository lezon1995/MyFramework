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

    public void BuildPlayerStats<TData>(List<TData> dataList, Action<PlayerStatItem, TData> onBuild)
    {
        PlayerStatItemPool.newItemList(dataList, (item, data) => onBuild?.Invoke(item, data));
    }

    public BallSlotGroupView SlotGroup => ballSlotGroupView;

    PlayerInfoBinder binder;

    public PlayerInfoBinder initBinder()
    {
        return binder ??= new(this, ballSlotGroupView.initBinder());
    }
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