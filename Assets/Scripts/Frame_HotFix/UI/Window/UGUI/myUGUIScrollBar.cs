using System;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityUtility;

// 对UGUI的ScrollBar的封装
public class myUGUIScrollBar : myUGUIObject
{
    protected Action<float, myUGUIScrollBar> onScroll; // 值改变的回调
    protected UnityAction<float> _onValueChange; // 避免GC的委托
    protected Scrollbar scrollBar; // UGUI的ScrollBar组件

    public myUGUIScrollBar()
    {
        _onValueChange = onValueChange;
    }

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out scrollBar))
        {
            if (!isNewObject)
            {
                logError("需要添加一个Scrollbar组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            scrollBar = go.AddComponent<Scrollbar>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public void setValue(float value)
    {
        scrollBar.value = value;
    }

    public float getValue()
    {
        return scrollBar.value;
    }

    public void setCallBack(Action<float, myUGUIScrollBar> callBack)
    {
        onScroll = callBack;
        scrollBar.onValueChanged.AddListener(_onValueChange);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void onValueChange(float value)
    {
        onScroll?.Invoke(value, this);
    }
}