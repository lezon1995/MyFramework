using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;

// 对UGUICanvas的封装
public class myUGUICanvas : myUGUIObject
{
    protected Canvas canvas; // UGUI的Canvas组件

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out canvas))
        {
            if (!isNewObject)
            {
                logError("需要添加一个Canvas组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            canvas = go.AddComponent<Canvas>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }

        canvas.overrideSorting = true;
        // 添加GraphicRaycaster
        getOrAddUnityComponent<GraphicRaycaster>();
    }

    public void setSortingOrder(int order)
    {
        canvas.sortingOrder = order;
    }

    public void setSortingLayer(string layerName)
    {
        canvas.sortingLayerName = layerName;
    }

    public Canvas getCanvas()
    {
        return canvas;
    }
}