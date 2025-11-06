using System;
using System.Collections.Generic;
using static FrameBaseUtility;

// 用于实现一个逻辑场景,一个逻辑场景会包含多个流程,进入一个场景时上一个场景会被销毁
public abstract class GameScene
{
    protected static GameScene curScene;
    protected Dictionary<Type, SceneProcedure> procedures = new(); // 场景的流程列表
    protected SceneProcedure curProcedure; // 当前流程
    protected Type startProcedure; // 起始流程类型,进入场景时会默认进入该流程
    protected Type exitProcedure; // 场景的退出流程,退出场景进入其他场景时会先进入该流程,一般用作资源卸载

    // 进入场景时初始化
    public virtual void init()
    {
        // 创建出所有的场景流程
        createSceneProcedure();
        // 设置起始流程名
        assignStartExitProcedure();
        // 开始执行起始流程
        enterStartProcedure();
    }

    public void enterStartProcedure()
    {
        changeProcedure(startProcedure);
    }

    public void destroy()
    {
        // 销毁所有流程
        procedures.Clear();
    }

    public void update(float elapsedTime)
    {
        // 更新当前流程
        curProcedure?.update(elapsedTime);
    }

    // 退出场景
    public virtual void exit()
    {
        // 首先进入退出流程,然后再退出最后的流程
        changeProcedure(exitProcedure);
        curProcedure?.exit();
        curProcedure = null;
        GC.Collect();
    }

    public virtual void willDestroy()
    {
        foreach (SceneProcedure item in procedures.Values)
        {
            item.willDestroy();
        }
    }

    public abstract void assignStartExitProcedure();

    public virtual void createSceneProcedure()
    {
    }

    public bool changeProcedure<T>() where T : SceneProcedure
    {
        return changeProcedure(typeof(T));
    }

    public bool changeProcedure(Type procedureType)
    {
        // 不能重复进入同一流程
        if (curProcedure != null && curProcedure.GetType() == procedureType)
        {
            return false;
        }

        if (!procedures.TryGetValue(procedureType, out SceneProcedure targetProcedure))
        {
            logErrorBase("can not find scene procedure : " + procedureType);
            return false;
        }

        logBase("enter procedure:" + procedureType);
        if (curProcedure == null || curProcedure.GetType() != procedureType)
        {
            // 如果当前已经在一个流程中了,则要先退出当前流程,但是不要销毁流程
            // 需要找到共同的父节点,退到该父节点时则不再退出
            curProcedure?.exit();
            curProcedure = targetProcedure;
            curProcedure.init();
        }

        return true;
    }

    public T addProcedure<T>() where T : SceneProcedure, new()
    {
        return procedures.add(typeof(T), new T()) as T;
    }
}