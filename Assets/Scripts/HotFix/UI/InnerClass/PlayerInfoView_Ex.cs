using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains;

public partial class PlayerInfoView
{
    public void SetLevel(int lv)
    {
        textLevel.setText(lv.IToS());
    }

    public void SetExp(int cur, int max)
    {
        textCurExp.setText(cur.IToS());
        textMaxExp.setText(max.IToS());
        if (expSlider != null)
        {
            float v = max > 0 ? Mathf.Clamp01((float)cur / max) : 0f;
            expSlider.setValue(v);
        }
    }

    public myUGUIObject StatListRoot => itemParent;

    public void BuildPlayerStats<TData>(List<TData> dataList, Action<PlayerStatItem, TData> onBuild)
    {
        PlayerStatItemPool.newItemList(dataList, onBuild);
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
        statIcon.setSpriteOnly(s);
    }

    public void SetName(string s)
    {
        statName.setText(s);
    }

    public void SetValue(string s)
    {
        statValue.setText(s);
    }
}