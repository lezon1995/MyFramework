using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static MathUtility;

// 自定义的进度条,跟滑动条的区别就是不能拖拽,实现更加简单,适用于加载进度条等等的功能
public class UGUIProgress : WindowObjectUGUI, ISlider, ICommonUI
{
    protected myUGUIImageSimple progressBar; // 进度条中显示进度的窗口
    protected myUGUIObject thumb; // 显示当前进度的点
    protected Vector3 originProgressPosition; // 进度窗口初始的位置
    protected Vector2 originProgressSize; // 进度窗口初始的大小
    protected float progressValue; // 当前的进度值
    protected SLIDER_MODE mode; // 进度条显示的实现方式

    public UGUIProgress(IWindowObjectOwner parent) : base(parent)
    {
        mode = SLIDER_MODE.FILL;
    }

    protected override void assignWindowInternal()
    {
        base.assignWindowInternal();
        newObject(out progressBar, "ProgressBar");
        newObject(out thumb, "Thumb", false);
    }

    public void initProgress()
    {
        if (progressBar.getImage().type == Image.Type.Filled)
        {
            mode = SLIDER_MODE.FILL;
        }
        else
        {
            mode = SLIDER_MODE.SIZING;
        }

        originProgressSize = progressBar.getWindowSize();
        originProgressPosition = progressBar.getPosition();
    }

    public void setValue(float value)
    {
        if (isVectorZero(originProgressSize))
        {
            logError("ProgressBar的size为0,是否忘记调用了UGUIProgress的initProgress?");
            return;
        }

        progressValue = value;
        saturate(ref progressValue);
        switch (mode)
        {
            case SLIDER_MODE.FILL:
                progressBar.setFillPercent(progressValue);
                break;
            case SLIDER_MODE.SIZING:
                float newWidth = progressValue * originProgressSize.x;
                FT.MOVE(progressBar, replaceX(originProgressPosition, originProgressPosition.x - originProgressSize.x * 0.5f + newWidth * 0.5f));
                progressBar.setWindowSize(new(newWidth, originProgressSize.y));
                break;
        }

        thumb?.setPositionX((progressValue - 0.5f) * originProgressSize.x);
    }

    public float getValue()
    {
        return progressValue;
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
        progressBar.getImage().enabled = show;
    }

    public Vector2 getOriginProgressSize()
    {
        return originProgressSize;
    }

    public Vector3 getOriginProgressPosition()
    {
        return originProgressPosition;
    }
}