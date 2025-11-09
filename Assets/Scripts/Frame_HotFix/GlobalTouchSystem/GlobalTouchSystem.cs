using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static MathUtility;
using static FrameBaseUtility;

// 用来代替UGUI的EventSystem,统一多摄像机的鼠标事件通知
public class GlobalTouchSystem : FrameSystem
{
    protected SafeDictionary<IMouseEventCollect, IMouseEventCollect> passOnlyArea = new(); // 点击穿透区域绑定列表,Key的区域中,只允许Value的区域穿透,Key无论是否设置了允许射线穿透,实际检测时都是不能够穿透的
    protected Dictionary<int, TouchInfo> touchInfos = new(); // 当前触点信息列表
    protected HashSet<IMouseEventCollect> parentPassOnlyList = new(); // 仅父节点区域可穿透的列表
    protected HashSet<IMouseEventCollect> allObjects = new(); // 所有参与鼠标或触摸事件的窗口和物体列表
    protected List<MouseCastWindowSet> mouseCastWindows = new(); // 所有窗口所对应的摄像机的列表,每个摄像机的窗口列表根据深度排序
    protected List<MouseCastObjectSet> mouseCastObjects = new(); // 所有场景中物体所对应的摄像机的列表,每个摄像机的物体列表根据深度排序
    protected SafeList<MovableObject> activeOnlyMovables = new(); // 当前只允许交互的3D物体,用于实现类似新手引导之类的功能,限定只能进行指定的操作
    protected SafeList<myUGUIObject> activeOnlyUIObjects = new(); // 当前只允许交互的UI物体,用于实现类似新手引导之类的功能,限定只能进行指定的操作,因为要对UI排序,所以只能分成两个列表
    protected bool useGlobalTouch = true; // 是否使用全局触摸检测来进行界面的输入检测
    protected bool activeOnlyUIListDirty; // UI的仅激活列表是否有修改,需要进行排序

    public GlobalTouchSystem()
    {
        Input.multiTouchEnabled = true;
    }

    public override void destroy()
    {
        allObjects.Clear();
        mouseCastWindows.Clear();
        mouseCastObjects.Clear();
        activeOnlyMovables.clear();
        activeOnlyUIObjects.clear();
        base.destroy();
    }

    public void setUseGlobalTouch(bool use)
    {
        useGlobalTouch = use;
    }

    public IMouseEventCollect getHoverObject(Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
    {
        // 返回最上层的物体
        IMouseEventCollect forwardButton = null;
        using var a = new ListScope<IMouseEventCollect>(out var resultList);
        globalRaycast(resultList, pos, ignorePassRay);
        foreach (var window in resultList)
        {
            if (ignoreWindow != window)
            {
                forwardButton = window;
                break;
            }
        }

        return forwardButton;
    }

    // 越顶层的物体越靠近列表前面
    public void getAllHoverObject(HashSet<IMouseEventCollect> hoverList, Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
    {
        hoverList.Clear();
        using var a = new ListScope<IMouseEventCollect>(out var resultList);
        globalRaycast(resultList, pos, ignorePassRay);
        foreach (var window in resultList)
        {
            hoverList.addIf(window, ignoreWindow != window);
        }
    }

    public void getAllHoverObject(List<IMouseEventCollect> hoverList, Vector3 pos, IMouseEventCollect ignoreWindow = null, bool ignorePassRay = false)
    {
        hoverList.Clear();
        using var a = new ListScope<IMouseEventCollect>(out var resultList);
        globalRaycast(resultList, pos, ignorePassRay);
        foreach (var window in resultList)
        {
            hoverList.addIf(window, ignoreWindow != window);
        }
    }

    public override void update(float dt)
    {
        if (!useGlobalTouch)
        {
            return;
        }

        foreach (var item in mouseCastWindows)
        {
            item.update();
        }

        using var a = new SafeDictionaryReader<int, TouchPoint>(mInputSystem.getTouchPointList());
        foreach (var (key, touchPoint) in a.mReadList)
        {
            if (touchPoint.isCurrentDown())
            {
                notifyTouchPress(touchPoint);
            }
            else if (touchPoint.isCurrentUp())
            {
                notifyTouchRelease(key);
            }
        }

        // 更新触点逻辑
        foreach (var touchInfo in touchInfos.Values)
        {
            touchInfo.update(dt);
        }

        // 检查摄像机是否被销毁
        if (isEditor())
        {
            foreach (var item in mouseCastWindows)
            {
                if (item.getCamera() != null && item.getCamera().isDestroy())
                {
                    logError("摄像机已销毁:" + item.getCamera().getName());
                }
            }

            foreach (var item in mouseCastObjects)
            {
                if (item.mCamera != null && item.mCamera.isDestroy())
                {
                    logError("摄像机已销毁:" + item.mCamera.getName());
                }
            }
        }
    }

    public bool isColliderRegistered(IMouseEventCollect obj)
    {
        return allObjects.Contains(obj);
    }

    // 注册碰撞器,只有注册了的碰撞器才会进行检测
    public void registerCollider(IMouseEventCollect obj, GameCamera camera = null)
    {
        // 允许自动添加碰撞盒
        if (obj.getCollider(true) == null)
        {
            logError("注册碰撞体的物体上找不到碰撞体组件! name:" + obj.getName() + ", " + obj.getDescription());
            return;
        }

        if (allObjects.Contains(obj))
        {
            logError("不能重复注册碰撞体: " + obj.getName() + ", " + obj.getDescription());
            return;
        }

        if (obj is myUGUIObject uiObj)
        {
            // 寻找窗口对应的摄像机
            camera ??= mCameraManager.getUICamera();
            if (camera == null)
            {
                logError("can not find ui camera for raycast!");
            }

            // 将窗口加入到鼠标射线检测列表中
            MouseCastWindowSet mouseCastSet = null;
            foreach (var item in mouseCastWindows)
            {
                if (item.getCamera() == camera)
                {
                    mouseCastSet = item;
                    break;
                }
            }

            if (mouseCastSet == null)
            {
                mouseCastSet = new();
                mouseCastSet.setCamera(camera);
                mouseCastWindows.Add(mouseCastSet);
            }

            mouseCastSet.addWindow(uiObj);
        }
        else if (obj is MovableObject)
        {
            MouseCastObjectSet mouseCastSet = null;
            foreach (var item in mouseCastObjects)
            {
                if (item.mCamera == camera)
                {
                    mouseCastSet = item;
                    break;
                }
            }

            if (mouseCastSet == null)
            {
                mouseCastSet = new();
                mouseCastSet.setCamera(camera);
                mouseCastObjects.Add(mouseCastSet);
            }

            mouseCastSet.addObject(obj);
        }
        else
        {
            logError("不支持的注册类型:" + obj.GetType());
        }

        allObjects.Add(obj);
    }

    // parent的区域中只有passOnlyArea的区域可以穿透
    public void bindPassOnlyArea(IMouseEventCollect parent, IMouseEventCollect passOnlyArea)
    {
        if (!allObjects.Contains(parent) || !allObjects.Contains(passOnlyArea))
        {
            logError("需要先注册碰撞体,才能绑定穿透区域, name" + passOnlyArea.getName() + ", " + passOnlyArea.getDescription());
            return;
        }

        this.passOnlyArea.add(parent, passOnlyArea);
    }

    // parent的区域中才能允许parent的子节点接收射线检测
    public void bindPassOnlyParent(IMouseEventCollect parent)
    {
        if (!allObjects.Contains(parent))
        {
            logError("需要先注册碰撞体,才能绑定父节点穿透区域, name:" + parent.getName() + ", " + parent.getDescription());
            return;
        }

        parentPassOnlyList.Add(parent);
    }

    // 注销碰撞器
    public void unregisterCollider(IMouseEventCollect obj)
    {
        if (!allObjects.Remove(obj))
            return;

        foreach (var item in touchInfos.Values)
        {
            item.removeObject(obj);
        }

        if (obj is myUGUIObject window)
        {
            activeOnlyUIObjects.remove(window);
            int count = mouseCastWindows.Count;
            for (int i = 0; i < count; ++i)
            {
                var item = mouseCastWindows[i];
                if (item.removeWindow(window))
                {
                    if (item.isEmpty())
                    {
                        mouseCastWindows.RemoveAt(i);
                    }

                    break;
                }
            }
        }
        else if (obj is MovableObject movable)
        {
            activeOnlyMovables.remove(movable);
            int count = mouseCastObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                var item = mouseCastObjects[i];
                if (item.removeObject(obj))
                {
                    if (item.isEmpty())
                    {
                        mouseCastObjects.RemoveAt(i);
                    }

                    break;
                }
            }
        }
        else
        {
            logError("此对象无法注销:" + obj);
        }

        parentPassOnlyList.Remove(obj);
        // key或者value中任意一个注销了,都要从列表中移除
        if (!passOnlyArea.remove(obj))
        {
            using var a = new SafeDictionaryReader<IMouseEventCollect, IMouseEventCollect>(passOnlyArea);
            foreach (var item in a.mReadList)
            {
                if (item.Value == obj)
                {
                    passOnlyArea.remove(item.Key);
                }
            }
        }
    }

    public void notifyWindowActiveChanged()
    {
        foreach (var item in mouseCastWindows)
        {
            item.notifyWindowActiveChanged();
        }
    }

    public void setActiveOnlyObject(IMouseEventCollect obj)
    {
        activeOnlyUIObjects.clear();
        activeOnlyMovables.clear();
        switch (obj)
        {
            case myUGUIObject window:
                activeOnlyUIObjects.addNotNull(window);
                activeOnlyUIListDirty = true;
                break;
            case MovableObject movable:
                activeOnlyMovables.addNotNull(movable);
                break;
        }
    }

    public void addActiveOnlyObject(IMouseEventCollect obj)
    {
        switch (obj)
        {
            case myUGUIObject window:
                activeOnlyUIObjects.addNotNull(window);
                activeOnlyUIListDirty = true;
                break;
            case MovableObject movable:
                activeOnlyMovables.addNotNull(movable);
                break;
        }
    }

    public bool hasActiveOnlyObject()
    {
        return activeOnlyMovables.count() > 0 || activeOnlyUIObjects.count() > 0;
    }

    // 将obj以及obj的所有父节点都放入列表,适用于滑动列表中的节点响应.因为需要依赖于父节点先接收事件,子节点才能正常接收事件
    public void setActiveOnlyObjectWithAllParent(myUGUIObject obj)
    {
        using var a = new ListScope<myUGUIObject>(out var list);
        while (obj != null)
        {
            list.addIf(obj, allObjects.Contains(obj));
            obj = obj.getParent();
        }

        activeOnlyMovables.clear();
        activeOnlyUIObjects.setRange(list);
        activeOnlyUIListDirty = true;
    }

    public void addActiveOnlyObjectWithAllParent(myUGUIObject obj)
    {
        using var a = new ListScope<myUGUIObject>(out var list);
        while (obj != null)
        {
            list.addIf(obj, allObjects.Contains(obj));
            obj = obj.getParent();
        }

        if (list.Count == 0)
        {
            return;
        }

        foreach (var item in list)
        {
            activeOnlyUIObjects.addUnique(item);
        }

        activeOnlyUIListDirty = true;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void notifyTouchPress(TouchPoint touch)
    {
        var touchID = touch.getTouchID();
        var pos = touch.getCurPosition();
        // 触点按下时记录触点的初始位置
        var touchInfo = touchInfos.getOrAddClass(touchID);
        touchInfo.init(touch);
        touchInfo.touchPress();

        // 通知全局屏幕触点事件
        if (activeOnlyUIObjects.count() == 0 && activeOnlyMovables.count() == 0)
        {
            foreach (var item in allObjects)
            {
                if (item.isReceiveScreenMouse())
                {
                    item.onScreenMouseDown(pos, touchID);
                }
            }

            using var a = new SafeListReader<IMouseEventCollect>(touchInfo.getPressList());
            foreach (var obj in a.mReadList)
            {
                // 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseDown中销毁了
                if (allObjects.Contains(obj))
                {
                    obj.onMouseDown(pos, touchID);
                }
            }
        }
        // 只允许指定的物体接收事件时
        else
        {
            using (var a = new SafeListReader<myUGUIObject>(activeOnlyUIObjects))
            {
                foreach (var item in a.mReadList)
                {
                    if (allObjects.Contains(item) && item.isReceiveScreenMouse())
                    {
                        item.onScreenMouseDown(pos, touchID);
                    }
                }
            }

            using (var b = new SafeListReader<MovableObject>(activeOnlyMovables))
            {
                foreach (var item in b.mReadList)
                {
                    if (allObjects.Contains(item) && item.isReceiveScreenMouse())
                    {
                        item.onScreenMouseDown(pos, touchID);
                    }
                }
            }

            // 因为onScreenMouseDown里可能会移除物体,所以这里还要再判断一次mAllObjectSet.Contains
            using (var c = new SafeListReader<myUGUIObject>(activeOnlyUIObjects))
            {
                foreach (var item in c.mReadList)
                {
                    if (allObjects.Contains(item) && touchInfo.getPressList().contains(item))
                    {
                        item.onMouseDown(pos, touchID);
                    }
                }
            }

            using (var d = new SafeListReader<MovableObject>(activeOnlyMovables))
            {
                foreach (var item in d.mReadList)
                {
                    if (allObjects.Contains(item) && touchInfo.getPressList().contains(item))
                    {
                        item.onMouseDown(pos, touchID);
                    }
                }
            }
        }
    }

    protected void notifyTouchRelease(int touchID)
    {
        // 触点抬起时移除记录的触点位置
        if (!touchInfos.TryGetValue(touchID, out TouchInfo touchInfo))
            return;

        var pos = touchInfo.getTouch().getCurPosition();
        // 通知全局屏幕触点事件
        if (activeOnlyUIObjects.count() == 0 && activeOnlyMovables.count() == 0)
        {
            foreach (IMouseEventCollect item in allObjects)
            {
                if (item.isReceiveScreenMouse())
                {
                    item.onScreenMouseUp(pos, touchID);
                }
            }

            using var a = new SafeListReader<IMouseEventCollect>(touchInfo.getPressList());
            foreach (var obj in a.mReadList)
            {
                // 如果此时窗口已经被销毁了,则不再通知,因为可能在onScreenMouseUp中销毁了
                if (allObjects.Contains(obj))
                {
                    obj.onMouseUp(pos, touchID);
                }
            }
        }
        // 只允许指定的物体接收事件时
        else
        {
            // 为了保险起见,在每次遍历时都会判断mAllObjectSet.Contains(item)
            using (var a = new SafeListReader<myUGUIObject>(activeOnlyUIObjects))
            {
                foreach (var item in a.mReadList)
                {
                    if (allObjects.Contains(item) && item.isReceiveScreenMouse())
                    {
                        item.onScreenMouseUp(pos, touchID);
                    }
                }
            }

            using (var b = new SafeListReader<MovableObject>(activeOnlyMovables))
            {
                foreach (var item in b.mReadList)
                {
                    if (allObjects.Contains(item) && item.isReceiveScreenMouse())
                    {
                        item.onScreenMouseUp(pos, touchID);
                    }
                }
            }

            // 因为onScreenMouseUp里可能会移除物体,所以这里还要再判断一次mAllObjectSet.Contains
            using (var c = new SafeListReader<myUGUIObject>(activeOnlyUIObjects))
            {
                foreach (var item in c.mReadList)
                {
                    if (allObjects.Contains(item) && touchInfo.getPressList().contains(item))
                    {
                        item.onMouseUp(pos, touchID);
                    }
                }
            }

            using (var d = new SafeListReader<MovableObject>(activeOnlyMovables))
            {
                foreach (var item in d.mReadList)
                {
                    if (allObjects.Contains(item) && touchInfo.getPressList().contains(item))
                    {
                        item.onMouseUp(pos, touchID);
                    }
                }
            }
        }

        if (touchInfo.getTouch().isMouse())
        {
            touchInfo.clearPressList();
        }
        else
        {
            touchInfos.Remove(touchID);
            UN_CLASS(ref touchInfo);
        }
    }

    // 全局射线检测
    protected void globalRaycast(List<IMouseEventCollect> resultList, Vector3 mousePos, bool ignorePassRay = false)
    {
        bool continueRay = true;
        // 每次检测UI时都需要对列表按摄像机深度进行降序排序
        quickSort(mouseCastWindows, MouseCastWindowSet.mComparisonDescend);
        foreach (var item in mouseCastWindows)
        {
            if (!continueRay)
                break;

            // 检查摄像机是否被销毁
            GameCamera camera = item.getCamera();
            if (!camera.isValid())
            {
                logError("摄像机已销毁:" + camera.getName());
                continue;
            }

            Ray ray = getCameraRay(mousePos, camera.getCamera());
            // 没有指定的交互物体
            if (activeOnlyUIObjects.count() == 0 && activeOnlyMovables.count() == 0)
            {
                raycastLayout(ray, item.getWindowOrderList(), resultList, ref continueRay, false, ignorePassRay);
            }
            else if (activeOnlyUIObjects.count() > 0)
            {
                checkActiveOnlyOrder();
                using var a = new ListScope<myUGUIObject>(out var list);
                foreach (IMouseEventCollect obj in activeOnlyUIObjects.getMainList())
                {
                    if (obj is myUGUIObject uiObj && item.getWindowOrderList().Contains(uiObj))
                    {
                        list.Add(uiObj);
                    }
                }

                if (list.Count > 0)
                {
                    raycastLayout(ray, list, resultList, ref continueRay, false, ignorePassRay);
                }
            }
        }

        // UI层允许当前鼠标射线穿过时才检测场景物体
        if (continueRay)
        {
            quickSort(mouseCastObjects, MouseCastObjectSet.mCompareDescend);
            foreach (var item in mouseCastObjects)
            {
                if (!continueRay)
                    break;

                // 检查摄像机是否被销毁
                if (item.mCamera != null && item.mCamera.isDestroy())
                {
                    logError("摄像机已销毁:" + item.mCamera.getName());
                    continue;
                }

                Camera camera;
                if (item.mCamera == null)
                {
                    GameCamera mainCamera = getMainCamera();
                    if (mainCamera == null)
                    {
                        logError("找不到主摄像机,无法检测摄像机射线碰撞");
                        continue;
                    }

                    camera = mainCamera.getCamera();
                }
                else
                {
                    camera = item.mCamera.getCamera();
                }

                if (activeOnlyUIObjects.count() == 0 && activeOnlyMovables.count() == 0)
                {
                    raycastMovableObject(getCameraRay(mousePos, camera), item.mObjectOrderList, resultList, ref continueRay, false);
                }
                else if (activeOnlyMovables.count() > 0)
                {
                    using var a = new ListScope<IMouseEventCollect>(out var list);
                    foreach (IMouseEventCollect obj in activeOnlyMovables.getMainList())
                    {
                        if (obj is MovableObject movable && item.mObjectOrderList.Contains(movable))
                        {
                            list.Add(movable);
                        }
                    }

                    if (list.Count > 0)
                    {
                        raycastMovableObject(getCameraRay(mousePos, camera), list, resultList, ref continueRay, false);
                    }
                }
            }
        }
    }

    protected void raycastMovableObject(Ray ray, List<IMouseEventCollect> moveObjectList, List<IMouseEventCollect> retList, ref bool continueRay, bool clearList = true)
    {
        if (clearList)
            retList.Clear();

        continueRay = true;
        using var a = new ListScope<DistanceSortHelper>(out var sortList);
        foreach (var box in moveObjectList)
        {
            // 将所有射线碰到的物体都放到列表中
            if (box.isActiveInHierarchy() &&
                box.isHandleInput() &&
                box.getCollider() != null &&
                box.getCollider().Raycast(ray, out RaycastHit hit, 10000.0f))
            {
                sortList.Add(new(getSquaredLength(hit.point - ray.origin), box));
            }
        }

        // 根据相交点由近到远的顺序排序
        quickSort(sortList, DistanceSortHelper.mCompareAscend);
        foreach (var item in sortList)
        {
            retList.Add(item.mObject);
            if (!item.mObject.isPassRay())
            {
                continueRay = false;
                break;
            }
        }
    }

    // ignorePassRay表示是否忽略窗口的isPassRay属性,true表示认为所有的都允许射线穿透
    // 但是ignorePassRay不会影响到PassOnlyArea和ParentPassOnly
    protected void raycastLayout<T>(Ray ray,
        List<T> windowOrderList,
        List<IMouseEventCollect> retList,
        ref bool continueRay,
        bool clearList = true,
        bool ignorePassRay = false) where T : IMouseEventCollect
    {
        if (clearList)
            retList.Clear();

        // mParentPassOnlyList需要重新整理,排除未启用的布局的窗口
        // passParent,在只允许父节点穿透的列表中已成功穿透的父节点列表
        using var a = new HashSetScope2<IMouseEventCollect>(out var activeParentList, out var passParent);
        // 筛选出已激活的父节点穿透窗口
        foreach (var item in parentPassOnlyList)
        {
            if (item.isDestroy())
            {
                logError("窗口已经被销毁,无法访问:" + item.getName());
                continue;
            }

            activeParentList.addIf(item, item.isActiveInHierarchy());
        }

        // 射线检测
        continueRay = true;
        foreach (T window in windowOrderList)
        {
            if (window.isDestroy())
            {
                logError("窗口已经被销毁,无法访问:" + window.getName());
                continue;
            }

            if (window.isActiveInHierarchy() &&
                window.isHandleInput() &&
                window.getCollider().Raycast(ray, out _, 10000.0f))
            {
                // 点击到了只允许父节点穿透的窗口,记录到列表中
                // 但是因为父节点一定是在子节点之后判断的,子节点可能已经拦截了射线,从而导致无法检测到父节点
                if (passParent.addIf(window, activeParentList.Contains(window)))
                {
                    // 特殊窗口暂时不能接收输入事件,所以不放入相交窗口列表中
                    continue;
                }

                // 点击了只允许部分穿透的背景
                if (this.passOnlyArea.tryGetValue(window, out IMouseEventCollect passOnlyArea))
                {
                    // 判断是否点到了背景中允许穿透的部分,如果是允许穿透的部分,则射线可以继续判断下层的窗口，否则不允许再继续穿透
                    continueRay = passOnlyArea.isActiveInHierarchy() && passOnlyArea.isHandleInput() && passOnlyArea.getCollider().Raycast(ray, out _, 10000.0f);
                    if (!continueRay)
                        break;

                    // 特殊窗口暂时不能接收输入事件,所以不放入相交窗口列表中
                    continue;
                }

                // 如果父节点不允许穿透
                if (!isParentPassed(window, activeParentList, passParent))
                    continue;

                // 射线成功与窗口相交,放入列表
                retList.Add(window);
                // 如果射线不能穿透当前按钮,则不再继续
                continueRay = ignorePassRay || window.isPassRay();
            }

            if (!continueRay)
                break;
        }
    }

    // obj的所有父节点中是否允许射线选中obj
    // bindParentList是当前激活的已绑定的仅父节点区域穿透的列表
    // passedParentList是bindParentList中射线已经穿透的父节点
    protected bool isParentPassed(IMouseEventCollect obj, HashSet<IMouseEventCollect> bindParentList, HashSet<IMouseEventCollect> passedParentList)
    {
        foreach (var item in bindParentList)
        {
            // 有父节点,并且父节点未成功穿透时,则认为当前窗口未相交
            if (obj.isChildOf(item) && !passedParentList.Contains(item))
            {
                return false;
            }
        }

        return true;
    }

    protected void checkActiveOnlyOrder()
    {
        if (activeOnlyUIListDirty)
        {
            activeOnlyUIListDirty = false;
            using var a = new ListScope<myUGUIObject>(out var list, activeOnlyUIObjects.getMainList());
            quickSort(list, MouseCastWindowSet.mUIDepthDescend);
            activeOnlyUIObjects.setRange(list);
        }
    }
}