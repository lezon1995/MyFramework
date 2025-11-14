using UnityEngine.Events;
using UnityEngine.UI;
using static UnityUtility;

// 对UGUI的Slider组件的封装
public class myUGUISlider : myUGUIObject, ISlider
{
    protected Slider slider; // UGUI的Slider组件

    public override void init()
    {
        base.init();
        if (!go.TryGetComponent(out slider))
        {
            if (!isNewObject)
            {
                logError("需要添加一个Slider组件,name:" + getName() + ", layout:" + getLayout().getName());
            }

            slider = go.AddComponent<Slider>();
            // 添加UGUI组件后需要重新获取RectTransform
            go.TryGetComponent(out rectT);
            t = rectT;
        }
    }

    public Slider getSlider()
    {
        return slider;
    }

    public void setRange(int min, int max)
    {
        if (min > max)
        {
            logError("滑动条的范围下限不能高于范围上限");
        }

        slider.minValue = min;
        slider.maxValue = max;
    }

    public void setSliderCallback(UnityAction<float> callback)
    {
        slider.onValueChanged.AddListener(callback);
    }

    public void setValue(float value)
    {
        slider.value = value;
    }

    public float getValue()
    {
        return slider.value;
    }

    public void setFillRect(myUGUIObject obj)
    {
        slider.fillRect = obj.getRectTransform();
    }

    public void setHandleRect(myUGUIObject obj)
    {
        slider.handleRect = obj.getRectTransform();
    }
}