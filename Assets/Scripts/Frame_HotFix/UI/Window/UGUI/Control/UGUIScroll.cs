using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameBaseHotFix;

// 自定义的滑动列表,基于容器(相当于状态的预设),物体(用于显示的物体),物体的各个状态在每个容器之间插值计算
// 需要在初始化时主动调用initScroll,也需要主动调用update
// 一般制作Container时需要多两个结束的Container放在两端,使Item在超出Container时不至于突然消失
public class UGUIScroll : WindowObjectUGUI, ICommonUI
{
    protected List<IScrollContainer> containers = new(); // 容器列表,用于获取位置缩放旋转等属性,每一容器的控制值就是其下标
    protected List<IScrollItem> items = new(); // 物体列表,用于显示,每一项的控制值就是其下标,所以在下面的代码中会频繁使用其下标来计算位置
    protected OnScrollItem onScrollItem; // 滚动的回调
    protected MyCurve scrollToTargetCurve; // 滚动到指定项使用的曲线
    protected DRAG_DIRECTION dragDirection; // 拖动方向,横向或纵向
    protected float focusSpeedThreshold; // 开始聚焦的速度阈值,当滑动速度正在自由降低的阶段时,速度低于该值则会以恒定速度自动聚焦到一个最近的项
    protected float maxContainerValue; // 容器中最大的控制值
    protected float maxControlValue; // 物体列表中最大的控制值,也代表一个完整周期
    protected float dragSensitive; // 拖动的灵敏度
    protected float attenuateFactor; // 移动速度衰减系数,鼠标在放开时移动速度会逐渐降低,衰减系数越大.速度降低越快
    protected float scrollToTargetStartValue; // 开始插值滚动到指定项时的控制值
    protected float scrollToTargetTimer; // 插值滚动到指定项的计时
    protected float scrollToTargetMaxTime; // 插值滚动到指定项的最大时间
    protected int mainFocus; // 容器默认聚焦的下标

    protected bool loop; // 是否循环滚动

    // 以下是用于实时计算的参数
    protected SCROLL_STATE state; // 当前状态
    protected float targetOffsetValue; // 本次移动的目标值
    protected float curOffset; // 整体的偏移值,并且会与每一项的原始偏移值叠加
    protected float scrollSpeed; // 当前滚动速度
    protected bool mouseDown; // 鼠标是否在窗口内按下,鼠标抬起或者离开窗口都会设置为false,鼠标按下时,跟随鼠标移动,鼠标放开时,按惯性移动

    public UGUIScroll(IWindowObjectOwner parent) : base(parent)
    {
        dragDirection = DRAG_DIRECTION.HORIZONTAL;
        focusSpeedThreshold = 1.5f;
        dragSensitive = 1.0f;
        attenuateFactor = 3.0f;
        scrollToTargetMaxTime = 0.2f;
    }

    public override void init()
    {
        root.setOnMouseDown(onMouseDown);
        root.setOnScreenMouseUp(onScreenMouseUp);
        root.setOnMouseMove(onMouseMove);
        root.setOnMouseStay(onMouseStay);
        root.registerCollider(true);
        // 为了能拖拽,所以根节点的深度需要在所有子节点之上
        root.setDepthOverAllChild(true);
    }

    public void initScroll<T>(List<T> containerList, int mainContainer = -1) where T : IScrollContainer
    {
        containers.setRangeDerived(containerList);
        if (containers.Count > 0)
        {
            maxContainerValue = containers.Count - 1;
            mainFocus = mainContainer;
            // 默认聚焦中间的Container
            if (mainFocus < 0)
            {
                mainFocus = containers.Count >> 1;
            }
        }

        setScrollToTargetCurve(KEY_CURVE.CUBIC_IN);
    }

    public void setScrollToTargetCurve(int curve)
    {
        scrollToTargetCurve = mKeyFrameManager.getKeyFrame(curve);
    }

    public int getNearIndex()
    {
        return getItemIndex(mainFocus - curOffset, true, loop);
    }

    public int getItemCount()
    {
        return items.Count;
    }

    public void setItemList<T>(List<T> itemList, int defaultIndex = 0) where T : class, IScrollItem
    {
        // 每一项的控制值就是其下标,所以在
        items.setRange(itemList);
        if (items.Count == 0)
        {
            maxControlValue = 0.0f;
            return;
        }

        maxControlValue = items.Count - 1;
        // 循环时首尾相接,但是首位之间间隔一个单位,所以整体长度需要加1
        if (loop)
        {
            maxControlValue += 1.0f;
        }

        if (defaultIndex >= 0)
        {
            scrollToIndex(defaultIndex);
        }
    }

    public void update(float elapsedTime)
    {
        switch (state)
        {
            // 自动匀速滚动到目标点
            case SCROLL_STATE.SCROLL_TO_TARGET:
            {
                float curOffset = this.curOffset;
                // 速度逐渐降低到速度阈值的一半,这里会将速度转化为绝对值再计算,但是为了避免可能对其他逻辑产生的影响,计算后会恢复其符号
                float speedSign = sign(scrollSpeed);
                scrollSpeed = abs(scrollSpeed) - elapsedTime * 1.0f;
                clampMin(ref scrollSpeed, focusSpeedThreshold * 0.5f);
                checkReachTarget(ref curOffset, elapsedTime * sign(targetOffsetValue - curOffset) * scrollSpeed, targetOffsetValue);
                scrollSpeed *= speedSign;
                updateItem(curOffset);
                if (isFloatEqual(this.curOffset, targetOffsetValue))
                {
                    stop();
                }

                break;
            }
            case SCROLL_STATE.LERP_SCROLL_TO_TARGET:
            {
                scrollToTargetTimer += elapsedTime;
                float percent = scrollToTargetCurve.evaluate(divide(scrollToTargetTimer, scrollToTargetMaxTime));
                updateItem(lerp(scrollToTargetStartValue, targetOffsetValue, percent));
                if (isFloatEqual(curOffset, targetOffsetValue))
                {
                    stop();
                }

                break;
            }
            // 鼠标拖动
            case SCROLL_STATE.DRAGING:
                scroll(mainFocus - curOffset + elapsedTime * scrollSpeed, false);
                break;
            // 鼠标抬起后自动减速到停止,或者减速到一定阈值,再自动滚动到某个项
            case SCROLL_STATE.SCROLL_TO_STOP:
            {
                float curControlValue = mainFocus - curOffset;
                bool needClamp = !loop && !inRangeFixed(curControlValue, 0.0f, maxControlValue);
                // 非循环模式下,当前偏移值小于0或者大于最大值时,需要回到正常的范围,偏移值越小,减速越快
                if (!isFloatZero(scrollSpeed))
                {
                    float t;
                    // 超出范围后快速减速至0,然后回弹
                    if (needClamp)
                    {
                        float delta = 0.0f;
                        if (curControlValue < 0.0f)
                        {
                            delta = 1.0f - curControlValue * 10.0f;
                        }
                        else if (curControlValue > maxControlValue)
                        {
                            delta = 1.0f + (curControlValue - maxControlValue) * 10.0f;
                        }

                        t = elapsedTime * attenuateFactor * delta * delta * 200.0f;
                    }
                    else
                    {
                        t = elapsedTime * attenuateFactor;
                    }

                    scrollSpeed = lerp(scrollSpeed, 0.0f, t, 0.1f);
                    curControlValue += elapsedTime * scrollSpeed;
                    scroll(curControlValue, false);
                    int willFocusIndex = getNearIndex();
                    if (needClamp)
                    {
                        // 超出范围在移动停止后回弹
                        if (isFloatZero(scrollSpeed))
                        {
                            lerpToTarget(willFocusIndex);
                        }
                    }
                    else
                    {
                        // 当速度小于一定值时才开始选择聚焦到某一项
                        if (abs(scrollSpeed) < focusSpeedThreshold)
                        {
                            scrollToTargetWithSpeed(willFocusIndex);
                        }
                    }
                }
                else
                {
                    int willFocusIndex = getNearIndex();
                    if (needClamp)
                    {
                        lerpToTarget(willFocusIndex);
                    }
                    else
                    {
                        scrollToTargetWithSpeed(willFocusIndex);
                    }
                }

                break;
            }
        }
    }

    public void stop()
    {
        state = SCROLL_STATE.NONE;
        scrollSpeed = 0.0f;
    }

    public SCROLL_STATE getState()
    {
        return state;
    }

    // 直接设置到指定位置
    public void scroll(float controlValue, bool checkValueRange = true)
    {
        float offset = mainFocus - controlValue;
        if (checkValueRange && !loop)
        {
            clamp(ref offset, mainFocus - maxControlValue, mainFocus);
        }

        updateItem(offset);
    }

    // 立即设置到指定下标
    public void scrollToIndex(int index)
    {
        if (items.Count == 0)
            return;

        clamp(ref index, 0, items.Count - 1);
        scroll(index);
        onScrollItem?.Invoke(items[index], index);
    }

    // 一定时间滚动到指定下标
    public void scrollToIndexWithTime(int index, float time)
    {
        if (items.Count == 0)
            return;

        clampMin(ref time, 0.03f);
        clamp(ref index, 0, items.Count - 1);
        // 设置目标值
        targetOffsetValue = mainFocus - index;
        if (loop)
        {
            clampCycle(ref curOffset, 0, maxControlValue, maxControlValue, false);
            clampCycle(ref targetOffsetValue, 0, maxControlValue, maxControlValue, false);
            // 当起始值与目标值差值超过了最大值的一半时,则以当前值为基准,调整目标值的范围
            float halfMax = maxControlValue * 0.5f;
            if (abs(targetOffsetValue - curOffset) > halfMax)
            {
                clampCycle(ref curOffset, -halfMax, halfMax, maxControlValue, false);
                clampCycle(ref targetOffsetValue, -halfMax, halfMax, maxControlValue, false);
            }
        }

        scrollSpeed = divide(targetOffsetValue - curOffset, time);
        state = isFloatZero(scrollSpeed) ? SCROLL_STATE.NONE : SCROLL_STATE.SCROLL_TO_TARGET;
        onScrollItem?.Invoke(items[index], index);
    }

    // 根据当前速度计算出滚动时间,匀减速滚动到指定下标
    public void scrollToTargetWithSpeed(int focusIndex)
    {
        float focusTime = clamp(abs(divide(0.4f, scrollSpeed)), 0.1f, 0.3f);
        scrollToIndexWithTime(focusIndex, focusTime);
    }

    // 按照指定曲线插值聚焦到指定下标
    public void lerpToTarget(int index)
    {
        state = SCROLL_STATE.LERP_SCROLL_TO_TARGET;
        scrollToTargetStartValue = curOffset;
        scrollToTargetTimer = 0.0f;
        clamp(ref index, 0, items.Count - 1);
        // 设置目标值
        targetOffsetValue = mainFocus - index;
        if (loop)
        {
            clampCycle(ref curOffset, 0, maxControlValue, maxControlValue, false);
            clampCycle(ref targetOffsetValue, 0, maxControlValue, maxControlValue, false);
            // 当起始值与目标值差值超过了最大值的一半时,则以当前值为基准,调整目标值的范围
            float halfMax = maxControlValue * 0.5f;
            if (abs(targetOffsetValue - curOffset) > halfMax)
            {
                clampCycle(ref curOffset, -halfMax, halfMax, maxControlValue, false);
                clampCycle(ref targetOffsetValue, -halfMax, halfMax, maxControlValue, false);
            }
        }

        onScrollItem?.Invoke(items[index], index);
    }

    public void setDragDirection(DRAG_DIRECTION direction)
    {
        dragDirection = direction;
    }

    public void setLoop(bool loop)
    {
        this.loop = loop;
    }

    public float getCurOffsetValue()
    {
        return curOffset;
    }

    public void setDragSensitive(float sensitive)
    {
        dragSensitive = sensitive;
    }

    public void setFocusSpeedThreshold(float threshold)
    {
        focusSpeedThreshold = threshold;
    }

    public void setAttenuateFactor(float factor)
    {
        attenuateFactor = factor;
    }

    public void setOnScrollItem(OnScrollItem callback)
    {
        onScrollItem = callback;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void updateItem(float controlValue)
    {
        if (containers.Count == 0)
            return;

        // 变化时需要随时更新当前值
        curOffset = controlValue;
        if (loop)
        {
            clampCycle(ref curOffset, -maxControlValue, maxControlValue, maxControlValue, false);
        }

        int itemCount = items.Count;
        for (int i = 0; i < itemCount; ++i)
        {
            IScrollItem item = items[i];
            myUGUIObject itemRoot = item.getItemRoot();
            float newControlValue = i + curOffset;
            itemRoot.setActive(true);
            if (loop)
            {
                clampCycle(ref newControlValue, -maxControlValue, maxControlValue, maxControlValue, false);
            }
            else
            {
                // 非循环模式下,新的控制值不在容器控制值范围内时,表示已经不在容器范围内了
                if (!inRangeFixed(newControlValue, 0.0f, maxContainerValue))
                {
                    itemRoot.setActive(false);
                }
            }

            if (!itemRoot.isActiveInHierarchy())
            {
                continue;
            }

            // 找到当前项位于哪两个容器之间,并且计算插值系数
            int containerIndex = getContainerIndex(newControlValue, false);
            itemRoot.setActive(inRangeFixed(containerIndex, 0, containers.Count - 1));
            if (!itemRoot.isActiveInHierarchy())
            {
                continue;
            }

            int nextContainerIndex = containerIndex + 1;
            if (loop)
            {
                nextContainerIndex %= containers.Count;
            }

            if (inRangeFixed(nextContainerIndex, 0, containers.Count - 1))
            {
                float curItemOffsetValue = containerIndex;
                float nextItemOffsetValue = nextContainerIndex;
                // 下一个下标比当前下标还小时,说明下一个下标已经从头开始了,需要重新调整下标
                if (nextContainerIndex < containerIndex && loop)
                {
                    nextItemOffsetValue = curItemOffsetValue + 1;
                }

                clampCycle(ref newControlValue, curItemOffsetValue, nextItemOffsetValue, maxControlValue);
                float percent = inverseLerp(curItemOffsetValue, nextItemOffsetValue, newControlValue);
                checkInt(ref percent);
                saturate(ref percent);
                item.lerpItem(containers[containerIndex], containers[nextContainerIndex], percent);
            }
            else
            {
                item.lerpItem(containers[containerIndex], containers[containerIndex], 1.0f);
            }
        }
    }

    // 根据controlValue查找在ItemList中的对应下标
    protected int getItemIndex(float controlValue, bool nearest, bool loop)
    {
        int itemCount = items.Count;
        if (itemCount == 0)
        {
            return -1;
        }

        if (loop)
        {
            clampCycle(ref controlValue, 0.0f, maxControlValue, maxControlValue, false);
        }

        int index = -1;
        for (int i = 0; i < itemCount; ++i)
        {
            float thisControlValue = i;
            if (isFloatEqual(thisControlValue, controlValue))
            {
                index = i;
                break;
            }

            // 找到第一个比controlValue大的项
            if (thisControlValue >= controlValue)
            {
                if (nearest)
                {
                    if (i > 0 && abs(thisControlValue - controlValue) >= abs(i - 1 - controlValue))
                    {
                        index = i - 1;
                    }
                    else
                    {
                        index = i;
                    }
                }
                else
                {
                    index = clampMin(i - 1);
                }

                break;
            }
        }

        // 如果找不到比当前ControlValue大的项
        if (index < 0)
        {
            index = itemCount - 1;
            // 非循环模式下,则固定范围最后一个,循环模式就找最后一个或者第一个中最近的一个
            if (loop && nearest)
            {
                if (abs(0 - (maxControlValue - controlValue)) < abs(itemCount - 1 - controlValue))
                {
                    index = 0;
                }
            }
        }

        return index;
    }

    // 根据controlValue查找在ContainerList中的对应下标,nearest为true则表示查找离该controlValue最近的下标
    protected int getContainerIndex(float controlValue, bool nearest)
    {
        if (loop)
        {
            clampCycle(ref controlValue, 0.0f, maxControlValue, maxControlValue, false);
        }

        if (controlValue > maxContainerValue)
        {
            return containers.Count - 1;
        }

        if (controlValue < 0.0f)
        {
            return -1;
        }

        int index = -1;
        int containerCount = containers.Count;
        for (int i = 0; i < containerCount; ++i)
        {
            float curControlValue = i;
            if (isFloatEqual(curControlValue, controlValue))
            {
                index = i;
                break;
            }

            // 找到第一个比controlValue大的项
            if (curControlValue >= controlValue)
            {
                if (nearest)
                {
                    if (i > 0 && abs(curControlValue - controlValue) <= abs(i - 1 - controlValue))
                    {
                        index = i - 1;
                    }
                    else
                    {
                        index = i;
                    }
                }
                else
                {
                    index = i - 1;
                }

                break;
            }
        }

        return index;
    }

    protected void onMouseDown(Vector3 mousePos, int touchID)
    {
        mouseDown = true;
        state = SCROLL_STATE.DRAGING;
        scrollSpeed = 0.0f;
    }

    // 鼠标在屏幕上抬起
    protected void onScreenMouseUp(IMouseEventCollect obj, Vector3 mousePos, int touchID)
    {
        mouseDown = false;
        // 正在拖动时鼠标抬起,则开始逐渐减速到0
        if (state == SCROLL_STATE.DRAGING)
        {
            state = SCROLL_STATE.SCROLL_TO_STOP;
        }
    }

    protected void onMouseMove(Vector3 mousePos, Vector3 moveDelta, float moveTime, int touchID)
    {
        // 鼠标未按下时不允许改变移动速度
        if (!mouseDown)
            return;

        scrollSpeed = dragDirection switch
        {
            DRAG_DIRECTION.HORIZONTAL => sign(-moveDelta.x) * abs(divide(moveDelta.x, moveTime)) * dragSensitive * 0.01f,
            DRAG_DIRECTION.VERTICAL => sign(moveDelta.y) * abs(divide(moveDelta.y, moveTime)) * dragSensitive * 0.01f,
            _ => scrollSpeed
        };
    }

    protected void onMouseStay(Vector3 mousePos, int touchID)
    {
        if (!mouseDown)
            return;

        scrollSpeed = 0.0f;
    }
}