using System;
using UnityEngine;
using static MathUtility;

// 使用LineRenderer的方式进行画线,是对LineRenderer的封装,用于在3D场景中画线,不局限于界面中
public class myLineRenderer
{
    protected LineRenderer renderer; // Unity的LineRenderer组件

    public void setLineRenderer(LineRenderer r)
    {
        renderer = r;
    }

    public void setPointList(Span<Vector3> pointList)
    {
        var count = pointList.Length;
        var list = new Vector3[count];
        for (int i = 0; i < count; ++i)
        {
            list[i] = pointList[i];
        }

        setPointList(list);
    }

    public void setPointList(Vector3[] pointList)
    {
        if (pointList == null)
        {
            renderer.SetPositions(null);
            return;
        }

        if (pointList.Length > renderer.positionCount)
        {
            renderer.positionCount = pointList.Length;
        }

        renderer.SetPositions(pointList);
        if (pointList.Length > 0 && pointList.Length < renderer.positionCount)
        {
            // 将未使用的点坐标设置为最后一个点
            int unuseCount = renderer.positionCount - pointList.Length;
            for (int i = 0; i < unuseCount; ++i)
            {
                renderer.SetPosition(i + pointList.Length, pointList[^1]);
            }
        }
    }

    public void setPointListBezier(Vector3[] pointList, int bezierDetail = 10)
    {
        if (pointList == null)
            return;

        Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
        getBezierPoints(pointList, curveList, renderer.loop, bezierDetail);
        setPointList(curveList);
    }

    public void setPointListSmooth(Vector3[] pointList, int bezierDetail = 10)
    {
        if (pointList == null)
            return;

        Span<Vector3> curveList = stackalloc Vector3[bezierDetail];
        getCurvePoints(pointList, curveList, renderer.loop, bezierDetail);
        setPointList(curveList);
    }

    public LineRenderer getRenderer()
    {
        return renderer;
    }

    public void setActive(bool active)
    {
        renderer.gameObject.SetActive(active);
    }

    public GameObject getGameObject()
    {
        return renderer.gameObject;
    }
}