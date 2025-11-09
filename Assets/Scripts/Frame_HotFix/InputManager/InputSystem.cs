using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;
using static FrameUtility;

// 输入系统,用于封装Input
public class InputSystem : FrameSystem
{
    protected Dictionary<IEventListener, Dictionary<Action, KeyListenInfo>> listeners = new(); // 以监听者为索引的快捷键监听列表
    protected SafeDictionary<KeyCode, SafeList<KeyListenInfo>> keyListeners = new(); // 按键按下的监听回调列表
    protected SafeDictionary<int, TouchPoint> touchPoints = new(); // 触点信息列表,可能与GlobalTouchSystem有一定的重合
    protected HashSet<IInputField> inputFields = new(); // 输入框列表,用于判断当前是否正在输入
    protected SafeList<DeadClick> lastClicks = new(); // 已经完成单击的行为,用于实现双击的功能
    protected List<KeyCode> allKeys = new(); // 所有支持的按键列表
    protected List<KeyCode> downKeys = new(); // 这一帧按下的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
    protected List<KeyCode> upKeys = new(); // 这一帧抬起的按键列表,每帧都会清空一次,用于在其他地方获取这一帧按下了哪些按键
    protected int focusMask; // 当前的输入掩码,是输入框的输入还是快捷键输入
    protected bool enableKey = true; // 是否启用按键的响应
    protected bool activeInput = true; // 是否检测输入

    const int left = (int)MOUSE_BUTTON.LEFT;
    const int right = (int)MOUSE_BUTTON.RIGHT;
    const int mid = (int)MOUSE_BUTTON.MIDDLE;

    public override void init()
    {
        base.init();
        initKey();
        // 编辑器或者桌面端,默认会有鼠标三个键的触点
        if (isEditor() || isStandalone())
        {
            addTouch(left, true);
            addTouch(right, true);
            addTouch(mid, true);
        }
    }

    public override void update(float dt)
    {
        base.update(dt);
        if (!activeInput)
            return;

        // 点击操作会判断触点和鼠标,因为都会产生点击操作
        // 在触屏上,会将第一个触点也当作鼠标,也就是触屏上使用isMouseCurrentDown实际上是获得的第一个触点的信息
        // 所以先判断有没有触点,如果没有再判断鼠标
        if (Input.touchCount == 0)
        {
            // 左键
            if (Input.GetMouseButtonDown(left))
            {
                pointDown(left, Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(left))
            {
                pointUp(left, Input.mousePosition);
            }

            // 仅在桌面端才会有右键和中键
            if (isEditor() || isStandalone())
            {
                // 右键
                if (isMouseRightCurrentDown())
                {
                    pointDown(right, Input.mousePosition);
                }
                else if (isMouseRightCurrentUp())
                {
                    pointUp(right, Input.mousePosition);
                }

                // 中键
                if (isMouseMiddleCurrentDown())
                {
                    pointDown(mid, Input.mousePosition);
                }
                else if (isMouseMiddleCurrentUp())
                {
                    pointUp(mid, Input.mousePosition);
                }
            }
        }
        else
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        pointDown(touch.fingerId, touch.position);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        pointUp(touch.fingerId, touch.position);
                        break;
                }
            }
        }

        // 更新触点位置
        if (Input.touchCount == 0)
        {
            using var a = new SafeDictionaryReader<int, TouchPoint>(touchPoints);
            foreach (TouchPoint item in a.mReadList.Values)
            {
                // 此处只更新鼠标的位置,因为touchCount为0时,mTouchPointList种也可能存在这一帧抬起的还未来得及移除的触摸屏的触点
                // webgl上也可能在鼠标按下时读取不到触点数量,所以触点数量为0而且又检测到了鼠标按下而添加信息到mTouchPointList,也使用鼠标位置来更新触点
                if (item.isMouse() || isWebGL())
                {
                    item.update(Input.mousePosition);
                }
            }
        }
        else
        {
            // 先将触点信息放入字典中,方便查询,虽然一般情况下触点都不会超过2个
            using var a = new DicScope<int, Touch>(out var touchInfoList);
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);
                touchInfoList.Add(touch.fingerId, touch);
            }

            // 找到指定ID的触点信息,获取触点的位置
            using var b = new SafeDictionaryReader<int, TouchPoint>(touchPoints);
            foreach (var (key, touchPoint) in b.mReadList)
            {
                if (touchInfoList.TryGetValue(key, out Touch touch))
                {
                    touchPoint.update(touch.position);
                }
                // 触点已经不存在了,应该不会出现这种情况,只是为了保险
                else
                {
                    touchPoint.update(touchPoint.getCurPosition());
                }
            }
        }

        // 判断是否有触点已经过期
        var now = DateTime.Now;
        using var c = new SafeListReader<DeadClick>(lastClicks);
        int deadCount = c.mReadList.Count;
        for (int i = 0; i < deadCount; ++i)
        {
            if ((now - c.mReadList[i].clickTime).TotalSeconds > 1.0f)
            {
                lastClicks.removeAt(i);
            }
        }

        bool inputting = false;
        foreach (IInputField item in inputFields)
        {
            if (item.isVisible() && item.isFocused())
            {
                inputting = true;
                break;
            }
        }

        setMask(inputting ? FOCUS_MASK.UI : FOCUS_MASK.SCENE);

        // 检测这一帧按下过哪些按键
        downKeys.Clear();
        upKeys.Clear();
        foreach (var key in allKeys)
        {
            if (Input.GetKeyDown(key))
            {
                downKeys.Add(key);
            }
            else if (Input.GetKeyUp(key))
            {
                upKeys.Add(key);
            }
        }

        // 遍历监听列表,发送监听事件
        COMBINATION_KEY curCombination = COMBINATION_KEY.NONE;
        curCombination |= isAnyKeyDown(KeyCode.LeftControl, KeyCode.RightControl) ? COMBINATION_KEY.CTRL : COMBINATION_KEY.NONE;
        curCombination |= isAnyKeyDown(KeyCode.LeftShift, KeyCode.RightShift) ? COMBINATION_KEY.SHIFT : COMBINATION_KEY.NONE;
        curCombination |= isAnyKeyDown(KeyCode.LeftAlt, KeyCode.RightAlt) ? COMBINATION_KEY.ALT : COMBINATION_KEY.NONE;
        using var d = new SafeDictionaryReader<KeyCode, SafeList<KeyListenInfo>>(keyListeners);
        foreach (var item in d.mReadList)
        {
            if (!isKeyCurrentDown(item.Key))
                continue;

            using var e = new SafeListReader<KeyListenInfo>(item.Value);
            foreach (KeyListenInfo info in e.mReadList)
            {
                if (info.combinationKey == curCombination)
                {
                    info.callback?.Invoke();
                }
            }
        }
    }

    public override void lateUpdate(float dt)
    {
        base.lateUpdate(dt);
        // 销毁已经不存在的触点
        using var b = new SafeDictionaryReader<int, TouchPoint>(touchPoints);
        foreach (var (key, point) in b.mReadList)
        {
            TouchPoint touchPoint = point;
            if (touchPoint.isCurrentUp())
            {
                // 只放入单击的行为,双击行为不再放入,否则会导致双击检测错误
                if (touchPoint.isClick() && !touchPoint.isDoubleClick())
                {
                    lastClicks.add(new(touchPoint.getCurPosition()));
                }

                // 不会移除鼠标的触点,因为始终都会有一个鼠标触点
                if (!touchPoint.isMouse())
                {
                    touchPoints.remove(key);
                    UN_CLASS(ref touchPoint);
                    continue;
                }
            }

            touchPoint.lateUpdate();
        }
    }

    public void registeInputField(IInputField inputField)
    {
        inputFields.Add(inputField);
    }

    public void unregisteInputField(IInputField inputField)
    {
        inputFields.Remove(inputField);
    }

    public void setMask(FOCUS_MASK mask)
    {
        focusMask = (int)mask;
    }

    public bool hasMask(FOCUS_MASK mask)
    {
        return mask == FOCUS_MASK.NONE || focusMask == 0 || (focusMask & (int)mask) != 0;
    }

    // 添加对于指定按键的当前按下事件监听,一般用于一些不重要的临时逻辑,如果是游戏允许自定义的快捷键,需要使用KeyMappingSystem来映射
    public void listenKeyCurrentDown(KeyCode key, Action callback, IEventListener listener, COMBINATION_KEY combination = COMBINATION_KEY.NONE)
    {
        CLASS(out KeyListenInfo info);
        info.callback = callback;
        info.listener = listener;
        info.key = key;
        info.combinationKey = combination;
        if (!keyListeners.tryGetValue(key, out var list))
        {
            list = new();
            keyListeners.add(key, list);
        }

        list.add(info);
        listeners.getOrAddNew(listener).TryAdd(callback, info);
    }

    // 移除监听者的所有按键监听
    public void unlistenKey(IEventListener listener)
    {
        if (!listeners.Remove(listener, out var list))
            return;

        foreach (var item in list)
        {
            keyListeners.tryGetValue(item.Value.key, out var callbackList);
            if (callbackList == null)
                continue;

            var callbacks = callbackList.getMainList();
            for (int i = 0; i < callbacks.Count; ++i)
            {
                if (item.Key == callbacks[i].callback)
                {
                    UN_CLASS(callbackList.get(i));
                    callbackList.removeAt(i--);
                }
            }
        }
    }

    // 是否有任意触点在这一帧按下,如果有,则返回第一个在这一帧按下的触点
    public bool getTouchDown(out TouchPoint touchPoint)
    {
        touchPoint = null;
        foreach (TouchPoint item in touchPoints.getMainList().Values)
        {
            if (item.isCurrentDown())
            {
                touchPoint = item;
                return true;
            }
        }

        return false;
    }

    // 是否有任意触点在这一帧完成一次点击操作,如果有,则返回第一个在这一帧完成点击的触点
    public TouchPoint getTouchClick()
    {
        foreach (TouchPoint item in touchPoints.getMainList().Values)
        {
            if (item.isClick())
            {
                return item;
            }
        }

        return null;
    }

    // 是否有任意触点在这一帧完成一次双击操作,如果有,则返回第一个在这一帧完成双击的触点
    public TouchPoint isTouchDoubleClick()
    {
        foreach (TouchPoint item in touchPoints.getMainList().Values)
        {
            if (item.isDoubleClick())
            {
                return item;
            }
        }

        return null;
    }

    // 指定触点是否处于持续按下状态
    public bool isTouchKeepDown(int pointerID)
    {
        if (!touchPoints.tryGetValue(pointerID, out TouchPoint point))
        {
            return false;
        }

        // 只要不是这一帧抬起和按下的,都是处于持续按下状态
        return !point.isCurrentUp() && !point.isCurrentDown();
    }

    // 指定触点是否在这一帧抬起
    public bool isTouchUp(int pointerID)
    {
        return touchPoints.tryGetValue(pointerID, out TouchPoint point) && point.isCurrentUp();
    }

    public TouchPoint getTouchPoint(int pointerID)
    {
        return touchPoints.get(pointerID);
    }

    // 外部可以通过获取点击操作列表,获取到这一帧的所有点击操作信息,且不分平台,统一移动端触屏和桌面端鼠标操作(未考虑桌面端的触屏)
    public SafeDictionary<int, TouchPoint> getTouchPointList()
    {
        return touchPoints;
    }

    public int getTouchPointDownCount()
    {
        int count = 0;
        foreach (TouchPoint item in touchPoints.getMainList().Values)
        {
            if (item.isDown())
            {
                ++count;
            }
        }

        return count;
    }

    // 以下鼠标相关函数只能在windows或者编辑器中使用
    public void setMouseVisible(bool visible)
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return;
        }

        Cursor.visible = visible;
    }

    public float getMouseWheelDelta()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return 0.0f;
        }

        return Input.mouseScrollDelta.y;
    }

    public float getMouseMoveX()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return 0.0f;
        }

        return Input.GetAxis("Mouse X");
    }

    public float getMouseMoveY()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return 0.0f;
        }

        return Input.GetAxis("Mouse Y");
    }

    public bool isMouseLeftDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return isMouseLeftKeepDown() || isMouseLeftCurrentDown();
    }

    public bool isMouseRightDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return isMouseRightKeepDown() || isMouseRightCurrentDown();
    }

    public bool isMouseMiddleDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return isMouseMiddleKeepDown() || isMouseMiddleCurrentDown();
    }

    public bool isMouseLeftKeepDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButton((int)MOUSE_BUTTON.LEFT);
    }

    public bool isMouseRightKeepDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButton(right);
    }

    public bool isMouseMiddleKeepDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButton(mid);
    }

    public bool isMouseLeftCurrentDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonDown((int)MOUSE_BUTTON.LEFT);
    }

    public bool isMouseRightCurrentDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonDown(right);
    }

    public bool isMouseMiddleCurrentDown()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonDown(mid);
    }

    public bool isMouseLeftCurrentUp()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonUp((int)MOUSE_BUTTON.LEFT);
    }

    public bool isMouseRightCurrentUp()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonUp(right);
    }

    public bool isMouseMiddleCurrentUp()
    {
        if (!isEditor() && !isStandalone())
        {
            logError("只能在编辑器或者桌面平台调用");
            return false;
        }

        return Input.GetMouseButtonUp(mid);
    }

    // mask表示要检测的输入类型,是UI输入框输入,还是场景全局输入,或者是其他情况下的输入
    public bool isAnyKeyDown(KeyCode key0, KeyCode key1, FOCUS_MASK mask = FOCUS_MASK.SCENE)
    {
        return isKeyDown(key0, mask) || isKeyDown(key1, mask);
    }

    public bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE)
    {
        return enableKey && Input.GetKeyDown(key) && hasMask(mask);
    }

    public bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE)
    {
        return enableKey && Input.GetKeyUp(key) && hasMask(mask);
    }

    public bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE)
    {
        return enableKey && (Input.GetKeyDown(key) || Input.GetKey(key)) && hasMask(mask);
    }

    public bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE)
    {
        return enableKey && (Input.GetKeyUp(key) || !Input.GetKey(key)) && hasMask(mask);
    }

    public void setEnableKey(bool enable)
    {
        enableKey = enable;
    }

    public bool isEnableKey()
    {
        return enableKey;
    }

    public List<KeyCode> getCurKeyDownList()
    {
        return downKeys;
    }

    public List<KeyCode> getCurKeyUpList()
    {
        return upKeys;
    }

    public bool getActiveInput()
    {
        return activeInput;
    }

    public void setActiveInput(bool value)
    {
        activeInput = value;
        foreach (var each in touchPoints.getMainList().safe())
        {
            each.Value.resetState();
        }
    }

    public bool isSupportKey(KeyCode key)
    {
        return allKeys.Contains(key);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void pointDown(int pointerID, Vector3 position)
    {
        if (!touchPoints.tryGetValue(pointerID, out TouchPoint point))
        {
            point = addTouch(pointerID, false);
        }

        point.pointDown(position);
    }

    protected void pointUp(int pointerID, Vector3 position)
    {
        if (!touchPoints.tryGetValue(pointerID, out TouchPoint point))
            return;

        point.pointUp(position, lastClicks.getMainList());
    }

    protected TouchPoint addTouch(int pointerID, bool isMouse)
    {
        CLASS(out TouchPoint point);
        point.setMouse(isMouse);
        point.setTouchID(pointerID);
        touchPoints.add(point.getTouchID(), point);
        return point;
    }

    protected void initKey()
    {
        allKeys.Add(KeyCode.A);
        allKeys.Add(KeyCode.B);
        allKeys.Add(KeyCode.C);
        allKeys.Add(KeyCode.D);
        allKeys.Add(KeyCode.E);
        allKeys.Add(KeyCode.F);
        allKeys.Add(KeyCode.G);
        allKeys.Add(KeyCode.H);
        allKeys.Add(KeyCode.I);
        allKeys.Add(KeyCode.J);
        allKeys.Add(KeyCode.K);
        allKeys.Add(KeyCode.L);
        allKeys.Add(KeyCode.M);
        allKeys.Add(KeyCode.N);
        allKeys.Add(KeyCode.O);
        allKeys.Add(KeyCode.P);
        allKeys.Add(KeyCode.Q);
        allKeys.Add(KeyCode.R);
        allKeys.Add(KeyCode.S);
        allKeys.Add(KeyCode.T);
        allKeys.Add(KeyCode.U);
        allKeys.Add(KeyCode.V);
        allKeys.Add(KeyCode.W);
        allKeys.Add(KeyCode.X);
        allKeys.Add(KeyCode.Y);
        allKeys.Add(KeyCode.Z);
        allKeys.Add(KeyCode.Keypad0);
        allKeys.Add(KeyCode.Keypad1);
        allKeys.Add(KeyCode.Keypad2);
        allKeys.Add(KeyCode.Keypad3);
        allKeys.Add(KeyCode.Keypad4);
        allKeys.Add(KeyCode.Keypad5);
        allKeys.Add(KeyCode.Keypad6);
        allKeys.Add(KeyCode.Keypad7);
        allKeys.Add(KeyCode.Keypad8);
        allKeys.Add(KeyCode.Keypad9);
        allKeys.Add(KeyCode.KeypadPeriod);
        allKeys.Add(KeyCode.KeypadDivide);
        allKeys.Add(KeyCode.KeypadMultiply);
        allKeys.Add(KeyCode.KeypadMinus);
        allKeys.Add(KeyCode.KeypadPlus);
        allKeys.Add(KeyCode.Alpha0);
        allKeys.Add(KeyCode.Alpha1);
        allKeys.Add(KeyCode.Alpha2);
        allKeys.Add(KeyCode.Alpha3);
        allKeys.Add(KeyCode.Alpha4);
        allKeys.Add(KeyCode.Alpha5);
        allKeys.Add(KeyCode.Alpha6);
        allKeys.Add(KeyCode.Alpha7);
        allKeys.Add(KeyCode.Alpha8);
        allKeys.Add(KeyCode.Alpha9);
        allKeys.Add(KeyCode.F1);
        allKeys.Add(KeyCode.F2);
        allKeys.Add(KeyCode.F3);
        allKeys.Add(KeyCode.F4);
        allKeys.Add(KeyCode.F5);
        allKeys.Add(KeyCode.F6);
        allKeys.Add(KeyCode.F7);
        allKeys.Add(KeyCode.F8);
        allKeys.Add(KeyCode.F9);
        allKeys.Add(KeyCode.F10);
        allKeys.Add(KeyCode.F11);
        allKeys.Add(KeyCode.F12);
        allKeys.Add(KeyCode.Equals);
        allKeys.Add(KeyCode.Minus);
        allKeys.Add(KeyCode.LeftBracket);
        allKeys.Add(KeyCode.RightBracket);
        allKeys.Add(KeyCode.Backslash);
        allKeys.Add(KeyCode.Semicolon);
        allKeys.Add(KeyCode.Quote);
        allKeys.Add(KeyCode.Comma);
        allKeys.Add(KeyCode.Period);
        allKeys.Add(KeyCode.Slash);
        allKeys.Add(KeyCode.BackQuote);
        allKeys.Add(KeyCode.Backspace);
        allKeys.Add(KeyCode.Insert);
        allKeys.Add(KeyCode.Delete);
        allKeys.Add(KeyCode.Home);
        allKeys.Add(KeyCode.End);
        allKeys.Add(KeyCode.PageUp);
        allKeys.Add(KeyCode.PageDown);
        allKeys.Add(KeyCode.Tab);
        allKeys.Add(KeyCode.UpArrow);
        allKeys.Add(KeyCode.DownArrow);
        allKeys.Add(KeyCode.LeftArrow);
        allKeys.Add(KeyCode.RightArrow);
        allKeys.Add(KeyCode.Space);
        allKeys.Add(KeyCode.LeftShift);
        allKeys.Add(KeyCode.RightShift);
        allKeys.Add(KeyCode.LeftControl);
        allKeys.Add(KeyCode.RightControl);
    }
}