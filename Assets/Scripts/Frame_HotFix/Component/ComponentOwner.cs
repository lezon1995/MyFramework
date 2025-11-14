using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameUtility;

// 组件拥有者,只有继承了组件拥有者的类才能够添加组件
public abstract class ComponentOwner : CommandReceiver
{
    protected List<ClassObjectCallback> onDestroy; // 监听对象销毁的列表
    protected SafeDictionary<Type, GameComponent> comDict; // 组件类型列表,first是组件的类型名
    protected SafeList<GameComponent> comList; // 组件列表,保存着组件之间的更新顺序
    protected HashSet<Type> dontAutoCreateTypes; // 不需要自动添加的组件类型
    protected HashSet<Type> disableTypes; // 不需要更新的组件类型,为了不需要关心组件的添加或者销毁,只是想禁用部分组件的更新
    protected bool ignoreTimeScale; // 是否忽略时间缩放

    public override void destroy()
    {
        destroying = true;
        if (comDict != null)
        {
            using var a = new SafeDictionaryReader<Type, GameComponent>(comDict);
            UN_CLASS_LIST(a.mReadList);
            comDict.clear();
        }

        comList?.clear();
        dontAutoCreateTypes?.Clear();
        disableTypes?.Clear();
        destroying = false;
        foreach (ClassObjectCallback callback in onDestroy.safe())
        {
            callback?.Invoke(this);
        }

        onDestroy?.Clear();
        base.destroy();
    }

    public void addDestroyCallback(ClassObjectCallback callback)
    {
        onDestroy ??= new();
        onDestroy.Add(callback);
    }

    public void removeDestroyCallback(ClassObjectCallback callback)
    {
        onDestroy?.Remove(callback);
    }

    public virtual void setActive(bool active)
    {
        if (destroying)
            return;

        if (comDict != null)
        {
            using var a = new SafeDictionaryReader<Type, GameComponent>(comDict);
            foreach (GameComponent item in a.mReadList.Values)
            {
                item?.notifyOwnerActive(active);
            }
        }
    }

    // 更新正常更新的组件
    public virtual void update(float dt)
    {
        if (comList == null || comList.count() == 0)
            return;

        using var a = new SafeListReader<GameComponent>(comList);
        int rootComponentCount = a.mReadList.Count;
        for (int i = 0; i < rootComponentCount; ++i)
        {
            // 数量为0,表示在更新组件的过程中当前对象被销毁
            if (a.mReadList.Count == 0 || isDestroy())
                return;

            GameComponent com = a.mReadList[i];
            using var b = new ProfilerScope(com.GetType().Name);
            if (com.isValid() && com.isActive() && (disableTypes == null || !disableTypes.Contains(com.getType())))
            {
                com.update(com.isIgnoreTimeScale() ? Time.unscaledDeltaTime : dt);
            }
        }
    }

    // 后更新
    public virtual void lateUpdate(float dt)
    {
        if (comList == null || comList.count() == 0)
            return;

        using var a = new SafeListReader<GameComponent>(comList);
        foreach (GameComponent com in a.mReadList)
        {
            if (com != null && com.isActive() && (disableTypes == null || !disableTypes.Contains(com.getType())))
            {
                com.lateUpdate(com.isIgnoreTimeScale() ? Time.unscaledDeltaTime : dt);
            }
        }
    }

    // 物理更新
    public virtual void fixedUpdate(float dt)
    {
        if (comList == null || comList.count() == 0)
            return;

        using var a = new SafeListReader<GameComponent>(comList);
        foreach (GameComponent com in a.mReadList)
        {
            if (com != null && com.isActive() && (disableTypes == null || !disableTypes.Contains(com.getType())))
            {
                com.fixedUpdate(com.isIgnoreTimeScale() ? Time.fixedUnscaledDeltaTime : dt);
            }
        }
    }

    public virtual void notifyAddComponent(GameComponent com)
    {
    }

    public GameComponent addComponent(Type type, bool active)
    {
        // 不能创建重名的组件
        if (comDict != null && comDict.containsKey(type))
        {
            logError("已经存在相同类型的组件了:" + type);
            return null;
        }

        if (CLASS(type) is not GameComponent com)
        {
            logError("组件类型错误,不是GameComponent类型," + type);
            return null;
        }

        com.init(this);
        // 将组件加入列表
        addComponentToList(com);
        // 通知创建了组件
        notifyAddComponent(com);
        com.setActive(active);
        com.setDefaultActive(active);
        return com;
    }

    public T addComponent<T>(bool active) where T : GameComponent, new()
    {
        // 不能创建重名的组件
        if (comDict != null && comDict.containsKey(typeof(T)))
        {
            logError("已经存在相同类型的组件了:" + typeof(T));
            return null;
        }

        T com = CLASS<T>();
        if (com == null)
            return null;

        com.init(this);
        // 将组件加入列表
        addComponentToList(com);
        // 通知创建了组件
        notifyAddComponent(com);
        com.setActive(active);
        com.setDefaultActive(active);
        return com;
    }

    public void addComponent<T>(out T component, bool active) where T : GameComponent, new()
    {
        component = addComponent<T>(active);
    }

    public GameComponent addInitComponent(Type type, bool active)
    {
        if (dontAutoCreateTypes != null && dontAutoCreateTypes.Contains(type))
            return null;

        return addComponent(type, active);
    }

    public T addInitComponent<T>(bool active) where T : GameComponent, new()
    {
        if (dontAutoCreateTypes != null && dontAutoCreateTypes.Contains(typeof(T)))
            return null;

        return addComponent<T>(active);
    }

    public void addInitComponent<T>(out T component, bool active) where T : GameComponent, new()
    {
        if (dontAutoCreateTypes != null && dontAutoCreateTypes.Contains(typeof(T)))
        {
            component = null;
            return;
        }

        component = addComponent<T>(active);
    }

    public SafeDictionary<Type, GameComponent> getAllComponent()
    {
        return comDict;
    }

    // 获取组件,如果没有此组件,则会添加组件
    public T getOrAddComponent<T>() where T : GameComponent
    {
        return getOrAddComponent(typeof(T)) as T;
    }

    public T getOrAddComponent<T>(out T com) where T : GameComponent
    {
        return com = getOrAddComponent(typeof(T)) as T;
    }

    public GameComponent getOrAddComponent(Type type)
    {
        if (comDict != null && comDict.tryGetValue(type, out GameComponent com))
        {
            return com;
        }

        return addComponent(type, true);
    }

    // 只获取激活的组件
    public T getActiveComponent<T>() where T : GameComponent
    {
        return getActiveComponent(typeof(T)) as T;
    }

    public T getActiveComponent<T>(out T com) where T : GameComponent
    {
        return com = getActiveComponent(typeof(T)) as T;
    }

    public GameComponent getActiveComponent(Type type)
    {
        if (comDict != null && comDict.tryGetValue(type, out GameComponent com) && com.isActive())
            return com;

        return null;
    }

    // 获取指定类型的组件
    public T getComponent<T>() where T : GameComponent
    {
        return getComponent(typeof(T)) as T;
    }

    public T getComponent<T>(out T com) where T : GameComponent
    {
        return com = getComponent(typeof(T)) as T;
    }

    public GameComponent getComponent(Type type)
    {
        if (comDict != null && comDict.tryGetValue(type, out GameComponent com))
            return com;

        return null;
    }

    public void removeDisableComponent<T>() where T : GameComponent
    {
        disableTypes?.Remove(typeof(T));
    }

    public void removeDisableComponent(Type type)
    {
        disableTypes?.Remove(type);
    }

    public void addDisableComponent<T>() where T : GameComponent
    {
        disableTypes ??= new();
        disableTypes.Add(typeof(T));
    }

    public void addDisableComponent(Type type)
    {
        disableTypes ??= new();
        disableTypes.Add(type);
    }

    public void activeComponent<T>(bool active = true) where T : GameComponent
    {
        getComponent(typeof(T))?.setActive(active);
    }

    public void activeComponent(Type type, bool active = true)
    {
        getComponent(type)?.setActive(active);
    }

    public void notifyComponentStart(GameComponent com)
    {
        if (com is IComponentModifyAlpha)
        {
            breakComponent<IComponentModifyAlpha>(com);
        }

        if (com is IComponentModifyColor)
        {
            breakComponent<IComponentModifyColor>(com);
        }

        if (com is IComponentModifyPosition)
        {
            breakComponent<IComponentModifyPosition>(com);
        }

        if (com is IComponentModifyRotation)
        {
            breakComponent<IComponentModifyRotation>(com);
        }

        if (com is IComponentModifyScale)
        {
            breakComponent<IComponentModifyScale>(com);
        }
    }

    public SafeList<GameComponent> getComponentList()
    {
        return comList;
    }

    public bool isIgnoreTimeScale()
    {
        return ignoreTimeScale;
    }

    // componentOnly表示是否只设置组件不受时间缩放影响,如果只有组件受影响,则Owner本身还是受到时间缩放影响的
    public virtual void setIgnoreTimeScale(bool ignore, bool componentOnly = false)
    {
        if (!componentOnly)
        {
            ignoreTimeScale = ignore;
        }

        foreach (GameComponent item in (comList?.getMainList()).safe())
        {
            item.setIgnoreTimeScale(ignore);
        }
    }

    public void addDontAutoCreate<T>() where T : GameComponent
    {
        dontAutoCreateTypes ??= new();
        dontAutoCreateTypes.Add(typeof(T));
    }

    public void addDontAutoCreate(Type type)
    {
        dontAutoCreateTypes ??= new();
        dontAutoCreateTypes.Add(type);
    }

    public override void resetProperty()
    {
        base.resetProperty();
        onDestroy?.Clear();
        comDict?.clear();
        comList?.clear();
        dontAutoCreateTypes?.Clear();
        disableTypes?.Clear();
        ignoreTimeScale = false;
        destroying = false;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 此函数由子类调用
    protected virtual void initComponents()
    {
    }

    protected void addComponentToList(GameComponent com)
    {
        comList ??= new();
        comList.add(com);

        comDict ??= new();
        comDict.add(com.GetType(), com);
    }

    protected void breakComponent<T>(GameComponent exceptComponent)
    {
        if (destroying)
            return;

        // 中断所有可中断的组件
        using var a = new SafeDictionaryReader<Type, GameComponent>(comDict);
        foreach (GameComponent com in a.mReadList.Values)
        {
            if (com.isActive() && com is T && com is IComponentBreakable comBreakable && com != exceptComponent)
            {
                comBreakable.notifyBreak();
                com.setActive(false);
            }
        }
    }
}