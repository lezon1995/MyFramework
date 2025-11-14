#if USE_CSHARP_10
using System.Runtime.CompilerServices;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityUtility;
using static FrameBaseHotFix;
using static StringUtility;
using static FrameBaseUtility;

// 布局脚本基类,用于执行布局相关的逻辑
public abstract class LayoutScript : DelayCmdWatcher, ILocalizationCollection, IWindowObjectOwner
{
    protected HashSet<myUGUIScrollRect> registeredScrollViews = new(); // 用于检测ScrollView合法性的列表
    protected HashSet<IInputField> registeredInputFields = new(); // 用于检测InputField合法性的列表
    protected HashSet<WindowStructPoolBase> poolList; // 布局中使用的窗口对象池列表,收集后方便统一销毁
    protected HashSet<WindowStructPoolBase> poolRootList; // mPoolList中由LayoutScript直接持有的对象池
    protected HashSet<WindowObjectBase> windowObjectList; // 布局中使用的非对象池中的窗口对象,收集后方便统一销毁
    protected HashSet<WindowObjectBase> windowObjectRootList; // mWindowObjectList中的root节点列表,也就是排除了嵌套在子界面中的对象
    protected HashSet<IDragViewLoop> dragViewLoopList; // 存储界面中的滚动列表,用于调用列表的update
    protected HashSet<IUGUIObject> localizationObjectList; // 注册的需要本地化的对象,因为每次修改文本显示都会往列表里加,所以使用HashSet
    protected GameLayout layout; // 所属布局
    protected myUGUIObject root; // 布局中的根节点
    protected bool registerChecked; // 是否已经检测过了合法性
    protected bool needUpdate = true; // 布局脚本是否需要指定update,为了提高效率,可以不执行当前脚本的update,虽然update可能是空的,但是不调用会效率更高
    protected bool escHide; // 按Esc键时是否关闭此界面

    public override void destroy()
    {
        base.destroy();
        // 避免遗漏本地化的注销,此处再次确认注销一次
        clearLocalization();

        foreach (WindowStructPoolBase item in poolList.safe())
        {
            item.destroy();
        }

        poolList?.Clear();
        poolRootList?.Clear();
        foreach (WindowObjectBase item in windowObjectRootList.safe())
        {
            item.destroy();
        }

        windowObjectRootList?.Clear();
        windowObjectList?.Clear();

        // 为了避免子类中遗漏注销监听,基类会再次注销监听一次
        mEventSystem?.unlistenEvent(this);
        mInputSystem?.unlistenKey(this);
        interruptAllCommand();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        registeredScrollViews.Clear();
        registeredInputFields.Clear();
        layout = null;
        root = null;
        registerChecked = false;
        needUpdate = true;
        escHide = false;
    }

    public virtual void setLayout(GameLayout layout)
    {
        this.layout = layout;
    }

    public virtual bool onESCDown()
    {
        if (escHide)
        {
            close();
        }

        return escHide;
    }

    public bool isNeedUpdate()
    {
        return needUpdate;
    }

    public bool isVisible()
    {
        return layout.isVisible();
    }

    public GameLayout getLayout()
    {
        return layout;
    }

    public void setRoot(myUGUIObject root)
    {
        this.root = root;
    }

    public myUGUIObject getRoot()
    {
        return root;
    }

    public void notifyUIObjectNeedUpdate(myUGUIObject uiObj, bool needUpdate)
    {
        layout.notifyUIObjectNeedUpdate(uiObj, needUpdate);
    }

    public void registerScrollRect(myUGUIScrollRect scrollRect, myUGUIObject viewport, myUGUIObject content, float verticalPivot = 1.0f, float horizontalPivot = 0.5f)
    {
        registeredScrollViews.addIf(scrollRect, isEditor());
        scrollRect.initScrollRect(viewport, content, verticalPivot, horizontalPivot);
        // 所有的可滑动列表都是不能穿透射线的
        scrollRect.registerCollider();
        bindPassOnlyParent(viewport);
    }

    public void registerInputField(IInputField inputField)
    {
        registeredInputFields.addIf(inputField, isEditor());
        mInputSystem.registeInputField(inputField);
        // 所有的输入框都是不能穿透射线的
        ((myUGUIObject)inputField).registerCollider();
    }

    public void unregisterInputField(IInputField inputField)
    {
        mInputSystem.unregisteInputField(inputField);
    }

    // parent的区域中才能允许parent的子节点接收射线检测
    public void bindPassOnlyParent(myUGUIObject parent)
    {
        // 设置当前窗口需要调整深度在所有子节点之上,并计算深度调整值
        parent.setDepthOverAllChild(true);
        parent.setDepth(parent.getParent().getDepth(), parent.getDepth().getOrderInParent());
        // 刷新深度
        parent.registerCollider();
        mGlobalTouchSystem.bindPassOnlyParent(parent);
    }

    // parent的区域中只有passOnlyArea的区域可以穿透
    public void bindPassOnlyArea(myUGUIObject parent, myUGUIObject passOnlyArea)
    {
        parent.registerCollider();
        passOnlyArea.registerCollider();
        mGlobalTouchSystem.bindPassOnlyArea(parent, passOnlyArea);
    }

    public void addLocalizationObject(IUGUIObject obj)
    {
        localizationObjectList ??= new();
        localizationObjectList.Add(obj);
    }

    public void addWindowStructPool(WindowStructPoolBase pool)
    {
        poolList ??= new();
        if (!poolList.Add(pool))
        {
            logError("不能重复注册对象池");
        }

        if (pool.isRootPool())
        {
            poolRootList ??= new();
            poolRootList.Add(pool);
        }
    }

    public void addWindowObject(WindowObjectBase windowObj)
    {
        windowObjectList ??= new();
        if (!windowObjectList.Add(windowObj))
        {
            logError("不能重复注册UI对象");
        }

        if (windowObj.isRootWindowObject())
        {
            windowObjectRootList ??= new();
            windowObjectRootList.Add(windowObj);
            if (windowObj is IDragViewLoop dragViewLoop)
            {
                dragViewLoopList ??= new();
                dragViewLoopList.Add(dragViewLoop);
            }
        }
    }

    public abstract void assignWindow();

    public virtual void init()
    {
        foreach (WindowObjectBase item in windowObjectRootList.safe())
        {
            item.init();
        }
    }

    public bool hasDragViewLoopList()
    {
        return dragViewLoopList.count() > 0;
    }

    public void updateAllDragView()
    {
        // 更新UI直接创建的滚动列表
        foreach (IDragViewLoop item in dragViewLoopList.safe())
        {
            if (item.isActive())
            {
                item.updateDragView();
            }
        }

        // 更新所有一级子界面的滚动列表
        foreach (WindowObjectBase item in windowObjectRootList.safe())
        {
            if (item.isActive() && item.hasDragViewLoopList())
            {
                item.updateDragViewLoop();
            }
        }
    }

    public virtual void update(float dt)
    {
    }

    public virtual void lateUpdate(float dt)
    {
    }

    // 一般是重置布局状态,再根据当前游戏状态设置布局显示前的状态,执行一些显示时的动效
    public virtual void onGameState()
    {
        if (isEditor() && !registerChecked)
        {
            registerChecked = true;
            // 检查是否注册了所有的ScrollRect
            using var a = new ListScope<ScrollRect>(out var scrollViewList);
            root.getObject().GetComponentsInChildren(scrollViewList);
            foreach (var scrollRect in scrollViewList)
            {
                if (!registeredScrollViews.Contains(layout.getUIObject(scrollRect.gameObject) as myUGUIScrollRect))
                {
                    logError("滑动列表未注册:" + scrollRect.gameObject.name + ", layout:" + layout.getName());
                }
            }

            using var b = new ListScope<InputField>(out var inputFieldList);
            root.getObject().GetComponentsInChildren(inputFieldList);
            foreach (var inputField in inputFieldList)
            {
                if (!registeredInputFields.Contains(layout.getUIObject(inputField.gameObject) as IInputField))
                {
                    logError("输入框未注册:" + inputField.gameObject.name + ", layout:" + layout.getName());
                }
            }
        }

        // 显示界面时自动调用所有非对象池对象的reset,用于通知对象重新显示
        // 只通知没有父节点的窗口对象,其他带父节点的会由父节点窗口对象来调用
        foreach (WindowObjectBase item in windowObjectRootList.safe())
        {
            item.reset();
        }
    }

    public virtual void onDrawGizmos()
    {
    }

    public virtual void onHide()
    {
        clearLocalization();
        mEventSystem?.unlistenEvent(this);
        // 隐藏界面时调用所有非对象池中对象的onHide,用于通知自身被隐藏了
        foreach (WindowObjectBase item in windowObjectRootList.safe())
        {
            if (item.isActive())
            {
                item.onHide();
            }
        }

        // 隐藏界面时调用所有对象池的回收,将创建的所有对象都回收掉
        foreach (WindowStructPoolBase item in poolRootList.safe())
        {
            item.unuseAll();
        }

        mInputSystem?.unlistenKey(this);
    }

    // 通知脚本开始显示或隐藏,中断全部命令
    public void notifyStartShowOrHide()
    {
        interruptAllCommand();
    }

    public bool hasObject(string name)
    {
        return hasObject(root, name);
    }

    public bool hasObject(myUGUIObject parent, string name)
    {
        parent ??= root;
        return getGameObject(name, parent.getObject()) != null;
    }

    public T cloneObject<T>(myUGUIObject parent, myUGUIObject oriObj, string name) where T : myUGUIObject, new()
    {
        cloneObject(out T target, parent, oriObj, name, true);
        return target;
    }

    public T cloneObject<T>(myUGUIObject parent, myUGUIObject oriObj, string name, bool active) where T : myUGUIObject, new()
    {
        cloneObject(out T target, parent, oriObj, name, active);
        return target;
    }

    // 各种形式的创建窗口操作一律不会排序子节点,不会刷新布局中的窗口深度,因为一般都是在assignWindow中调用
    // 而assignWindow后会刷新当前布局的窗口深度,而子节点排序只有在部分情况下才会使用,大部分情况不会用到
    public void cloneObject<T>(out T target, myUGUIObject parent, myUGUIObject oriObj, string name) where T : myUGUIObject, new()
    {
        cloneObject(out target, parent, oriObj, name, true);
    }

    public void cloneObject<T>(out T target, myUGUIObject parent, myUGUIObject oriObj, string name, bool active) where T : myUGUIObject, new()
    {
        parent ??= root;
        var obj = UnityUtility.cloneObject(oriObj.getObject(), name);
        target = newUIObject<T>(parent, layout, obj);
        target.setActive(active);
        target.cloneFrom(oriObj);
    }

    // 创建myUGUIObject,并且新建GameObject,分配到myUGUIObject中
    // refreshUIDepth表示创建后是否需要刷新所属父节点下所有子节点的深度信息
    // sortChild表示创建后是否需要对myUGUIObject中的子节点列表进行排序,使列表的顺序与面板的顺序相同,对需要射线检测的窗口有影响
    public T createUGUIObject<T>(myUGUIObject parent, string name, bool active) where T : myUGUIObject, new()
    {
        var go = createGameObject(name);
        parent ??= root;
        // UGUI需要添加RectTransform
        getOrAddComponent<RectTransform>(go);
        go.layer = parent.getObject().layer;
        T obj = newUIObject<T>(parent, layout, go);
        obj.setActive(active);
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = Vector3.zero;
        return obj;
    }

    public T createUIObject<T>(myUGUIObject parent, string name, bool active) where T : myUGUIObject, new()
    {
        var go = createGameObject(name);
        parent ??= root;
        go.layer = parent.getObject().layer;
        T obj = newUIObject<T>(parent, layout, go);
        obj.setActive(active);
        go.transform.localScale = Vector3.one;
        go.transform.localEulerAngles = Vector3.zero;
        go.transform.localPosition = Vector3.zero;
        return obj;
    }

    public void createUIObject<T>(out T obj, myUGUIObject parent, string name) where T : myUGUIObject, new()
    {
        obj = createUIObject<T>(parent, name, true);
    }

    public T createUGUIObject<T>(myUGUIObject parent, string name) where T : myUGUIObject, new()
    {
        return createUGUIObject<T>(parent, name, true);
    }

    public void createUGUIObject<T>(out T obj, myUGUIObject parent, string name) where T : myUGUIObject, new()
    {
        obj = createUGUIObject<T>(parent, name, true);
    }

    public T createUGUIObject<T>(string name, bool active) where T : myUGUIObject, new()
    {
        return createUGUIObject<T>(null, name, active);
    }
    // 仅支持C#10
#if USE_CSHARP_10
	public T newObject<T>(out T obj, [CallerArgumentExpression("obj")] string name = "") where T : myUGUIObject, new()
	{
		return newObject(out obj, mRoot, name.rangeToEnd(1), true);
	}
	public T newObject<T>(out T obj, [CallerArgumentExpression("obj")] string name = "") where T : myUGUIObject, new()
	{
		return newObject(out obj, mRoot, name.rangeToEnd(1), true);
	}
	public T newObject<T>(out T obj, myUGUIObject parent, [CallerArgumentExpression("obj")] string name = "") where T : myUGUIObject, new()
	{
		return newObject(out obj, parent, name.rangeToEnd(1), true);
	}
	public T newObject<T>(out T obj, myUGUIObject parent, [CallerArgumentExpression("obj")] string name = "") where T : myUGUIObject, new()
	{
		return newObject(out obj, parent, name.rangeToEnd(1), true);
	}
#endif
    public T newObject<T>(out T obj, string name) where T : myUGUIObject, new()
    {
        return newObject(out obj, root, name, true);
    }

    public T newObject<T>(out T obj, string name, bool showError) where T : myUGUIObject, new()
    {
        return newObject(out obj, root, name, showError);
    }

    public T newObject<T>(out T obj, myUGUIObject parent, string name) where T : myUGUIObject, new()
    {
        return newObject(out obj, parent, name, true);
    }

    // 创建myUGUIObject,并且在布局中查找GameObject分配到myUGUIObject
    public T newObject<T>(out T obj, myUGUIObject parent, string name, bool showError) where T : myUGUIObject, new()
    {
        obj = null;
        var parentObj = parent?.getObject();
        GameObject gameObject;
        if (parentObj == null)
            gameObject = getRootGameObject(name, showError);
        else
            gameObject = getGameObject(name, parentObj, showError, false);

        if (gameObject == null)
            return obj;

        myUGUIObject existUIObj = layout.getUIObject(gameObject);
        if (existUIObj)
        {
            if (showError)
            {
                logError("已经创建了相同GameObject的UI对象:" + name);
                return null;
            }

            obj = existUIObj as T;
            if (obj == null)
            {
                logError("已经创建了相同GameObject的UI对象,但是两次创建的类型不一致,第一次创建的类型:" + existUIObj.GetType() + ", 第二次创建的类型:" + typeof(T) + ", name:" + name);
            }

            return obj;
        }

        obj = newUIObject<T>(parent, layout, gameObject);
        return obj;
    }

    public T newObject<T>(out T obj, myUGUIObject parent, GameObject go) where T : myUGUIObject, new()
    {
        obj = newUIObject<T>(parent, layout, go);
        return obj;
    }

    public static T newUIObject<T>(myUGUIObject parent, GameLayout layout, GameObject go) where T : myUGUIObject, new()
    {
        var obj = new T();
        obj.setLayout(layout);
        obj.setObject(go);
        obj.setParent(parent, false);
        obj.init();
        // 如果在创建窗口对象时,布局已经完成了自适应,则通知窗口
        if (layout && layout.isAnchorApplied())
        {
            obj.notifyAnchorApply();
        }

        return obj;
    }

    public static GameObject instantiate(myUGUIObject parent, string prefabPath, string name, int tag = 0)
    {
        var go = mPrefabPoolManager.createObject(prefabPath, tag, false, false, parent.getObject());
        if (go)
            go.name = name;

        return go;
    }

    public static CustomAsyncOperation instantiateAsync(string prefabPath, string name, int tag, GameObjectCallback callback)
    {
        return mPrefabPoolManager.createObjectAsync(prefabPath, tag, false, false, go =>
        {
            if (go)
                go.name = name;

            callback?.Invoke(go);
        });
    }

    public static void instantiate(myUGUIObject parent, string prefabName)
    {
        instantiate(parent, prefabName, getFileNameNoSuffixNoDir(prefabName));
    }

    public static void destroyInstantiate(myUGUIObject window, bool destroyReally)
    {
        if (window == null)
            return;

        var go = window.getObject();
        myUGUIObject.destroyWindow(window, false);
        mPrefabPoolManager.destroyObject(ref go, destroyReally);
        // 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
    }

    // 虽然执行内容与类似,但是为了外部使用方便,所以添加了对于不同方式创建出来的窗口的销毁方法
    public static void destroyCloned(myUGUIObject obj, bool immediately = false)
    {
        destroyObject(obj, immediately);
    }

    public static void destroyObject(ref myUGUIObject obj, bool immediately = false)
    {
        destroyObject(obj, immediately);
        obj = null;
    }

    public static void destroyObject(myUGUIObject obj, bool immediately = false)
    {
        obj.setDestroyImmediately(immediately);
        myUGUIObject.destroyWindow(obj, true);
        // 窗口销毁时不会通知布局刷新深度,因为移除对于深度不会产生影响
    }

    public void close()
    {
        CmdLayoutManagerVisible.execute(GetType(), false, false);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void clearLocalization()
    {
        foreach (var o in localizationObjectList.safe())
        {
            var item = o as myUGUIObject;
            mLocalizationManager.unregisterLocalization(item);
        }

        localizationObjectList?.Clear();
    }
}