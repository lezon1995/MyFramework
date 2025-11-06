using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static FrameBaseHotFix;
using static CSharpUtility;
using static FrameBaseUtility;

// 用于实现一个逻辑场景,一个逻辑场景会包含多个流程,进入一个场景时上一个场景会被销毁
public abstract class GameScene : ComponentOwner
{
    protected Dictionary<Type, SceneProcedure> procedures = new(); // 场景的流程列表
    protected List<SceneProcedure> lastProcedures = new(); // 所进入过的所有流程
    protected SceneProcedure curProcedure; // 当前流程
    protected GameObject go; // 场景对应的GameObject
    protected Type startProcedure; // 起始流程类型,进入场景时会默认进入该流程
    protected Type tempStartProcedure; // 仅使用一次的起始流程类型,设置后进入场景时会默认进入该流程,生效后就清除
    protected Type exitProcedure; // 场景的退出流程,退出场景进入其他场景时会先进入该流程,一般用作资源卸载

    protected const int MAX_LAST_PROCEDURE_COUNT = 8; // lastProcedures列表的最大长度,当超过该长度时,会移除列表开始的元素

    // 进入场景时初始化
    public virtual void init()
    {
        // 创建场景对应的物体,并挂接到场景管理器下
        go = createGameObject(name, mGameSceneManager.getObject());
        if (isEditor())
        {
            go.AddComponent<GameSceneDebug>().setGameScene(this);
        }

        initComponents();
        // 创建出所有的场景流程
        createSceneProcedure();
        // 设置起始流程名
        assignStartExitProcedure();
        // 开始执行起始流程
        enterStartProcedure();
    }

    public override void resetProperty()
    {
        base.resetProperty();
        procedures.Clear();
        lastProcedures.Clear();
        curProcedure = null;
        go = null;
        startProcedure = null;
        tempStartProcedure = null;
        exitProcedure = null;
    }

    public void enterStartProcedure()
    {
        changeProcedure(tempStartProcedure ?? startProcedure);
        tempStartProcedure = null;
    }

    public virtual void willDestroy()
    {
        foreach (SceneProcedure item in procedures.Values)
        {
            item.willDestroy();
        }
    }

    public override void destroy()
    {
        base.destroy();
        // 销毁所有流程
        foreach (SceneProcedure item in procedures.Values)
        {
            item.destroy();
        }

        procedures.Clear();
        destroyUnityObject(ref go);
    }

    public override void update(float elapsedTime)
    {
        // 更新组件
        base.update(elapsedTime);
        // 更新当前流程
        keyProcess(elapsedTime);
        curProcedure?.update(elapsedTime);
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
        curProcedure?.lateUpdate(elapsedTime);
    }

    public virtual void keyProcess(float elapsedTime)
    {
        // 在准备退出当前流程时,不响应任何按键操作
        if (curProcedure != null && !curProcedure.isPreparingExit())
        {
            curProcedure.keyProcess(elapsedTime);
        }
    }

    // 退出场景
    public virtual void exit()
    {
        // 首先进入退出流程,然后再退出最后的流程
        changeProcedure(exitProcedure);
        curProcedure?.exit(null, null);
        curProcedure = null;
        GC.Collect();
    }

    public GameObject getObject()
    {
        return go;
    }

    public abstract void assignStartExitProcedure();

    public virtual void createSceneProcedure()
    {
    }

    public bool atProcedure(Type type)
    {
        return curProcedure != null && curProcedure.isThisOrParent(type);
    }

    public bool atProcedure<T>() where T : SceneProcedure
    {
        return curProcedure != null && curProcedure.isThisOrParent(typeof(T));
    }

    // 是否在指定的流程,不考虑子流程
    public bool atSelfProcedure(Type type)
    {
        return curProcedure != null && curProcedure.GetType() == type;
    }

    public void prepareChangeProcedure<T>(float time) where T : SceneProcedure
    {
        prepareChangeProcedure(typeof(T), time);
    }

    public void prepareChangeProcedure(Type procedure, float time)
    {
        // 准备时间必须大于0
        if (time <= 0.0f)
        {
            logError("preapare time must be larger than 0!");
            return;
        }

        // 正在准备跳转时,不允许再次准备跳转
        if (curProcedure.isPreparingExit())
        {
            logError("procedure is preparing to exit, can not prepare again!");
            return;
        }

        curProcedure.prepareExit(procedures.get(procedure), time);
    }

    public void backToLastProcedure()
    {
        if (lastProcedures.Count == 0)
        {
            return;
        }

        // 获得上一次进入的流程
        changeProcedure(getLastProcedureType(), false);
        lastProcedures.RemoveAt(lastProcedures.Count - 1);
    }

    public bool changeProcedure<T>(bool addToLastList = true) where T : SceneProcedure
    {
        return changeProcedure(typeof(T), addToLastList);
    }

    public bool changeProcedure(Type procedureType, bool addToLastList = true)
    {
        // 当流程正在准备跳转流程时,不允许再跳转
        if (curProcedure != null && curProcedure.isPreparingExit())
        {
            logError("procedure is preparing to change, can not change again!");
            return false;
        }

        // 不能重复进入同一流程
        if (curProcedure != null && curProcedure.GetType() == procedureType)
        {
            return false;
        }

        if (!procedures.TryGetValue(procedureType, out SceneProcedure targetProcedure))
        {
            logError("can not find scene procedure : " + procedureType);
            return false;
        }

        log("enter procedure:" + procedureType);
        // 将上一个流程记录到返回列表中
        if (curProcedure != null && addToLastList)
        {
            lastProcedures.Add(curProcedure);
            if (lastProcedures.Count > MAX_LAST_PROCEDURE_COUNT)
            {
                lastProcedures.RemoveAt(0);
            }
        }

        if (curProcedure == null || curProcedure.GetType() != procedureType)
        {
            // 如果当前已经在一个流程中了,则要先退出当前流程,但是不要销毁流程
            // 需要找到共同的父节点,退到该父节点时则不再退出
            curProcedure?.exit(curProcedure.getSameParent(targetProcedure), targetProcedure);
            SceneProcedure lastProcedure = curProcedure;
            curProcedure = targetProcedure;
            curProcedure.init(lastProcedure);
        }

        return true;
    }

    // 流程调用,通知场景当前流程已经准备完毕
    public void notifyProcedurePrepared()
    {
        if (lastProcedures.Count > 0)
        {
            lastProcedures[^1].onNextProcedurePrepared(curProcedure);
        }
    }

    //  获取上一个流程
    public Type getLastProcedureType()
    {
        if (lastProcedures.Count == 0)
        {
            return null;
        }

        return lastProcedures[^1].GetType();
    }

    public SceneProcedure getProcedure(Type type)
    {
        return procedures.get(type);
    }

    public Type getCurProcedureType()
    {
        return curProcedure.GetType();
    }

    public SceneProcedure getCurProcedure()
    {
        return curProcedure;
    }

    // 获取当前场景的当前流程或父流程中指定类型的流程
    public SceneProcedure getCurOrParentProcedure(Type type)
    {
        return curProcedure.getThisOrParent(type);
    }

    public T getCurOrParentProcedure<T>() where T : SceneProcedure
    {
        return curProcedure.getThisOrParent(typeof(T)) as T;
    }

    public void setTempStartProcedure(Type procedure)
    {
        tempStartProcedure = procedure;
    }

    public T addProcedure<T>(Type parent = null) where T : SceneProcedure
    {
        return addProcedure(typeof(T), parent) as T;
    }

    public SceneProcedure addProcedure(Type type, Type parent = null)
    {
        var procedure = createInstance<SceneProcedure>(type);
        procedure.setGameScene(this);
        if (parent != null)
        {
            SceneProcedure parentProcedure = getProcedure(parent);
            if (parentProcedure == null)
            {
                logError("invalid parent procedure, procedure:" + procedure.GetType());
            }

            parentProcedure.addChildProcedure(procedure);
        }

        return procedures.add(procedure.GetType(), procedure);
    }
}