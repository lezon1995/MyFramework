using System;
using static MathUtility;

// 长按信息
public class LongPressData : ClassObject
{
    public Action onLongPressed; // 达到长按时间阈值时的回调,由GlobalTouchSystem驱动
    public FloatCallback onLongPressing; // 长按进度回调,可获取当前还有多久到达长按时间阈值
    public float longPressTime; // 长按的时间阈值,超过阈值时检测为长按
    public bool finished; // 是否已经完成了长按事件,完成后将不会再检测

    public override void resetProperty()
    {
        base.resetProperty();
        onLongPressed = null;
        onLongPressing = null;
        longPressTime = 0.0f;
        finished = false;
    }

    public void update(float pressedTime)
    {
        // 长按事件已经结束,不需要再检测
        if ((onLongPressed == null && onLongPressing == null) || finished)
            return;

        // 长按事件被中断,比如长按触点距离移动过远,触点抬起
        if (pressedTime < 0.0f)
        {
            finished = true;
            onLongPressing?.Invoke(0.0f);
            return;
        }

        // 还未到达长按时间
        onLongPressing?.Invoke(clampMax(divide(pressedTime, longPressTime), 1.0f));
        // 已到达长按时间,触发长按事件
        if (pressedTime >= longPressTime)
        {
            finished = true;
            onLongPressed?.Invoke();
        }
    }

    public void reset()
    {
        finished = false;
    }
}