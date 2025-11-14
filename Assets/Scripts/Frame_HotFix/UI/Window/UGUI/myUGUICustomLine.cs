using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;

// 可用于在UI上画线条的窗口
public class myUGUICustomLine : myUGUIObject
{
    protected CustomLine line; // 自定义的代替LineRenderer的组件

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out line))
        {
            if (!isNewObject)
            {
                logError("需要添加一个CustomLine组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            line = go.AddComponent<CustomLine>();
            // 添加UGUI组件后需要重新获取RectTransform,这里由于是自定义组件,不一定需要
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public void setWidth(float width)
    {
        line.setWidth(width);
    }

    public void setPointList(List<Vector3> list)
    {
        line.setPointList(list);
    }

    public void setPointList(Span<Vector3> list)
    {
        line.setPointList(list);
    }

    public void setPointList(Vector3[] list)
    {
        line.setPointList(list);
    }
}