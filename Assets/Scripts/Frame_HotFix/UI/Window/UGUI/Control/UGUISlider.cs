using System;
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// 自定义的滑动条
public class UGUISlider : WindowObjectUGUI, ISlider, ICommonUI
{
    protected Action onSliderStart; // 开始拖拽滑动的回调
    protected Action onSliderEnd; // 结束拖拽滑动的回调
    protected Action onSlider; // 滑动条改变的回调
    protected myUGUIImageSimple foreground; // 滑动条中显示进度的窗口
    protected myUGUIObject thumb; // 滑块窗口
    protected Vector3 originForegroundPosition; // 进度窗口初始的位置
    protected Vector2 originForegroundSize; // 进度窗口初始的大小
    protected float sliderValue; // 当前的滑动值
    protected bool dragging; // 是否正在拖拽滑动
    protected bool enableDrag; // 是否需要启用手指滑动进度条
    protected DRAG_DIRECTION direction; // 滑动方向
    protected SLIDER_MODE mode; // 滑动条显示的实现方式

    public UGUISlider(IWindowObjectOwner parent) : base(parent)
    {
        direction = DRAG_DIRECTION.HORIZONTAL;
        mode = SLIDER_MODE.FILL;
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        newObject(out foreground, "Foreground");
        newObject(out thumb, foreground, "Thumb", false);
    }

    public void initSlider(Action sliderCallback)
    {
        initSlider(true, DRAG_DIRECTION.HORIZONTAL, sliderCallback);
    }

    public void initSlider(bool enable, DRAG_DIRECTION dir, Action onSliding)
    {
        enableDrag = enable;
        direction = dir;
        if (foreground.getImage().type == Image.Type.Filled)
        {
            mode = SLIDER_MODE.FILL;
        }
        else
        {
            mode = SLIDER_MODE.SIZING;
        }

        onSlider = onSliding;
        originForegroundSize = foreground.getWindowSize();
        originForegroundPosition = foreground.getPosition();
        if (enableDrag)
        {
            root.registerCollider();
            root.setOnMouseDown(onMouseDown);
            root.setOnScreenMouseUp(onScreenMouseUp);
            root.setOnMouseMove(onMouseMove);
        }
    }

    public void setEnable(bool enable)
    {
        root.setHandleInput(enable);
    }

    public void setDirection(DRAG_DIRECTION dir)
    {
        direction = dir;
    }

    public void setStartCallback(Action callback)
    {
        onSliderStart = callback;
    }

    public void setEndCallback(Action callback)
    {
        onSliderEnd = callback;
    }

    public void setSliderCallback(Action callback)
    {
        onSlider = callback;
    }

    public void setValue(float value)
    {
        updateSlider(value);
    }

    public void setValueByListView(myUGUIObject content, myUGUIObject viewport)
    {
        if (direction == DRAG_DIRECTION.VERTICAL)
        {
            float maxY = content.getWindowSize().y * 0.5f - viewport.getWindowSize().y * 0.5f;
            setValue(inverseLerp(maxY, -maxY, content.getPosition().y));
        }
        else if (direction == DRAG_DIRECTION.HORIZONTAL)
        {
            float maxX = content.getWindowSize().x * 0.5f - viewport.getWindowSize().x * 0.5f;
            setValue(inverseLerp(-maxX, maxX, content.getPosition().x));
        }
    }

    public Vector3 generateListViewContentPosition(myUGUIObject content, myUGUIObject viewport)
    {
        if (direction == DRAG_DIRECTION.VERTICAL)
        {
            float maxY = content.getWindowSize().y * 0.5f - viewport.getWindowSize().y * 0.5f;
            if (maxY < 0.0f)
            {
                return replaceY(content.getPosition(), -maxY);
            }

            return replaceY(content.getPosition(), lerp(maxY, -maxY, sliderValue));
        }
        else if (direction == DRAG_DIRECTION.HORIZONTAL)
        {
            float maxX = content.getWindowSize().x * 0.5f - viewport.getWindowSize().x * 0.5f;
            if (maxX < 0)
            {
                return replaceY(content.getPosition(), maxX);
            }

            return replaceY(content.getPosition(), lerp(-maxX, maxX, sliderValue));
        }

        return Vector3.zero;
    }

    public float getValue()
    {
        return sliderValue;
    }

    public bool isDragging()
    {
        return dragging;
    }

    public void setEnableDrag(bool enable)
    {
        enableDrag = enable;
    }

    public bool isEnableDrag()
    {
        return enableDrag;
    }

    public void setSliderMode(SLIDER_MODE m)
    {
        mode = m;
    }

    public SLIDER_MODE getSliderMode()
    {
        return mode;
    }

    public void showForeground(bool show)
    {
        foreground.getImage().enabled = show;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void updateSlider(float value)
    {
        if (isVectorZero(originForegroundSize))
        {
            logError("foreground的size为0,是否忘记调用了UGUISlider的initSlider?");
            return;
        }

        sliderValue = value;
        saturate(ref sliderValue);
        switch (mode)
        {
            case SLIDER_MODE.FILL:
                foreground.setFillPercent(sliderValue);
                break;
            case SLIDER_MODE.SIZING:
                switch (direction)
                {
                    case DRAG_DIRECTION.HORIZONTAL:
                        float newWidth = sliderValue * originForegroundSize.x;
                        FT.MOVE(foreground, replaceX(originForegroundPosition, originForegroundPosition.x - originForegroundSize.x * 0.5f + newWidth * 0.5f));
                        foreground.setWindowSize(new(newWidth, originForegroundSize.y));
                        break;
                    case DRAG_DIRECTION.VERTICAL:
                        float newHeight = sliderValue * originForegroundSize.y;
                        FT.MOVE(foreground, replaceY(originForegroundPosition, originForegroundPosition.y - originForegroundSize.y * 0.5f + newHeight * 0.5f));
                        foreground.setWindowSize(new(originForegroundSize.x, newHeight));
                        break;
                }

                break;
        }

        FT.MOVE(thumb, sliderValueToThumbPos(sliderValue));
    }

    protected Vector3 sliderValueToThumbPos(float value)
    {
        var pos = Vector3.zero;
        pos = direction switch
        {
            DRAG_DIRECTION.HORIZONTAL => mode switch
            {
                SLIDER_MODE.FILL => new(value * originForegroundSize.x - originForegroundSize.x * 0.5f, 0.0f),
                SLIDER_MODE.SIZING => new(foreground.getWindowRight(), 0.0f),
                _ => pos
            },
            DRAG_DIRECTION.VERTICAL => mode switch
            {
                SLIDER_MODE.FILL => new(0.0f, value * originForegroundSize.y - originForegroundSize.y * 0.5f),
                SLIDER_MODE.SIZING => new(0.0f, foreground.getWindowTop()),
                _ => pos
            },
            _ => pos
        };

        return pos;
    }

    protected void onMouseDown(Vector3 mousePos, int touchID)
    {
        // 先调用开始回调
        onSliderStart?.Invoke();
        // 计算当前值
        updateSlider(screenPosToSliderValue(mousePos));
        onSlider?.Invoke();
        dragging = true;
    }

    protected void onScreenMouseUp(IMouseEventCollect obj, Vector3 mousePos, int touchID)
    {
        // 调用结束回调
        if (!dragging)
            return;

        dragging = false;
        onSliderEnd?.Invoke();
    }

    protected void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        if (!dragging)
            return;

        updateSlider(screenPosToSliderValue(mousePos));
        onSlider?.Invoke();
    }

    protected float screenPosToSliderValue(Vector3 screenPos)
    {
        Vector3 posInForeground = mode switch
        {
            // 只转换到进度条窗口中的坐标
            SLIDER_MODE.FILL => screenPosToWindow(screenPos, foreground),
            // 先将屏幕坐标转换到Background中的坐标,再转换到原始进度条的坐标系中
            SLIDER_MODE.SIZING => (Vector3)screenPosToWindow(screenPos, root) - originForegroundPosition,
            _ => Vector3.zero
        };

        // 将本地坐标转换为滑动条的值
        float value = direction switch
        {
            DRAG_DIRECTION.HORIZONTAL => divide(posInForeground.x + originForegroundSize.x * 0.5f, originForegroundSize.x),
            DRAG_DIRECTION.VERTICAL => divide(posInForeground.y + originForegroundSize.y * 0.5f, originForegroundSize.y),
            _ => 0.0f
        };

        return saturate(value);
    }
}