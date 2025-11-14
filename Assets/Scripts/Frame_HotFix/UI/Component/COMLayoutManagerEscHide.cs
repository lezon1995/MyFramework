using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static MathUtility;

// 用于处理一些需要监听ESC键来关闭的布局
public class COMLayoutManagerEscHide : GameComponent
{
    protected Comparison<GameLayout> comparison; // 比较布局渲染顺序的函数,避免GC
    protected List<GameLayout> layoutRenderOrderList = new(); // 按渲染顺序排序的布局列表,只有已显示的列表

    public COMLayoutManagerEscHide()
    {
        comparison = compareLayoutRenderOrder;
    }

    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        mInputSystem.listenKeyCurrentDown(KeyCode.Escape, onESCDown, this);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        // mCompareLayoutRenderOrder不重置
        // mCompareLayoutRenderOrder = null;
        layoutRenderOrderList.Clear();
    }

    public void notifyLayoutRenderOrder()
    {
        layoutRenderOrderList.Sort(comparison);
    }

    public void notifyLayoutVisible(bool visible, GameLayout layout)
    {
        if (visible)
        {
            if (layoutRenderOrderList.addUnique(layout))
            {
                layoutRenderOrderList.Sort(comparison);
            }
        }
        else
        {
            layoutRenderOrderList.Remove(layout);
        }
    }

    public override void destroy()
    {
        mInputSystem?.unlistenKey(this);
        base.destroy();
    }

    public void notifyLayoutDestroy(GameLayout layout)
    {
        layoutRenderOrderList.Remove(layout);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onESCDown()
    {
        // 从上往下逐级发送ESC按下事件,有布局处理后就不再传递
        int count = layoutRenderOrderList.Count;
        for (int i = count - 1; i >= 0; --i)
        {
            var layout = layoutRenderOrderList[i];
            if (layout == null)
                continue;

            if (layout.getScript() == null)
            {
                logError(layout.getName() + "已经销毁");
                continue;
            }

            try
            {
                if (layout.getScript().onESCDown())
                    break;
            }
            catch (Exception e)
            {
                logException(e, "layout:" + layout.getName());
                break;
            }
        }
    }

    protected int compareLayoutRenderOrder(GameLayout x, GameLayout y)
    {
        return sign(x.getRenderOrder() - y.getRenderOrder());
    }
}