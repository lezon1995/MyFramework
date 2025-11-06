using System;
using System.Collections.Generic;
using static FrameUtility;
using static FrameBaseHotFix;

// 场景流程
public class SceneProcedure : DelayCmdWatcher
{
    protected Dictionary<Type, SceneProcedure> subProcedures = new(); // 子流程列表
    protected SceneProcedure parentProcedure; // 父流程
    protected SceneProcedure mPrepareNext; // 准备退出到的流程
    protected GameScene scene; // 流程所属的场景
    protected MyTimer mPrepareTimer = new(); // 准备退出的计时
    protected bool mInited; // 是否已经初始化,子节点在初始化时需要先确保父节点已经初始化

    public override void resetProperty()
    {
        base.resetProperty();
        subProcedures.Clear();
        parentProcedure = null;
        mPrepareNext = null;
        scene = null;
        mPrepareTimer.stop();
        mInited = false;
    }

    // 销毁场景时会调用流程的销毁
    public virtual void willDestroy()
    {
    }

    public override void destroy()
    {
        base.destroy();
    }

    public void setGameScene(GameScene gameScene)
    {
        scene = gameScene;
    }

    // 进入的目标流程已经准备完成(资源加载完毕等等)时的回调
    public virtual void onNextProcedurePrepared(SceneProcedure next)
    {
    }

    // 由GameScene调用
    // 进入流程
    public void init(SceneProcedure lastProcedure)
    {
        // 如果父节点还没有初始化,则先初始化父节点
        if (parentProcedure is { mInited: false })
        {
            parentProcedure.init(lastProcedure);
            // 退出父节点自身而进入子节点
            parentProcedure.onExitToChild(this);
            parentProcedure.onExitSelf();
        }

        // 再初始化自己,如果是从子节点返回到父节点,则需要调用另外一个初始化函数
        if (lastProcedure != null && lastProcedure.isThisOrParent(GetType()))
        {
            onInitFromChild(lastProcedure);
        }
        else
        {
            onInit(lastProcedure);
        }

        mInited = true;
    }

    // 更新流程
    public void update(float elapsedTime)
    {
        // 先更新父节点
        parentProcedure?.update(elapsedTime);
        // 再更新自己
        onUpdate(elapsedTime);
        // 检查准备退出流程
        if (mPrepareTimer.tickTimer(elapsedTime))
        {
            // 超过了准备时间,强制跳转流程
            changeProcedure(mPrepareNext.GetType());
        }
    }

    public void lateUpdate(float elapsedTime)
    {
        parentProcedure?.lateUpdate(elapsedTime);
        onLateUpdate(elapsedTime);
    }

    // 退出流程
    public void exit(SceneProcedure exitTo, SceneProcedure nextPro)
    {
        // 中断自己所有未执行的命令
        interruptAllCommand();
        // 当停止目标为自己时,则不再退出,此时需要判断当前将要进入的流程是否为当前流程的子流程
        // 如果是,则需要调用onExitToChild,执行退出当前并且进入子流程的操作
        // 如果不是则不需要调用,不需要执行任何退出操作
        if (this == exitTo)
        {
            if (nextPro != null && nextPro.isThisOrParent(GetType()))
            {
                onExitToChild(nextPro);
                onExitSelf();
            }

            return;
        }

        // 先退出自己
        onExit(nextPro);
        onExitSelf();
        mInited = false;
        // 再退出父节点
        parentProcedure?.exit(exitTo, nextPro);
        // 退出完毕后就修改标记
        mPrepareTimer.stop();
        mPrepareNext = null;
    }

    public void prepareExit(SceneProcedure next, float time)
    {
        mPrepareTimer.init(0.0f, time, false);
        mPrepareNext = next;
        // 通知自己准备退出
        onPrepareExit(next);
    }

    public void keyProcess(float elapsedTime)
    {
        // 先处理父节点按键响应
        parentProcedure?.keyProcess(elapsedTime);
        // 然后再处理自己的按键响应
        onKeyProcess(elapsedTime);
    }

    public void getParentList(List<SceneProcedure> parentList)
    {
        // 由于父节点列表中需要包含自己,所以先加入自己
        parentList.Add(this);
        // 再加入父节点的所有父节点
        parentProcedure?.getParentList(parentList);
    }

    // 获得自己和otherProcedure的共同的父节点
    public SceneProcedure getSameParent(SceneProcedure otherProcedure)
    {
        // 获得两个流程的父节点列表
        SceneProcedure sameParent = null;
        using var a = new ListScope2<SceneProcedure>(out var tempList0, out var tempList1);
        getParentList(tempList0);
        otherProcedure.getParentList(tempList1);
        // 从前往后判断,找到第一个相同的父节点
        int count0 = tempList0.Count;
        for (int i = 0; i < count0; ++i)
        {
            SceneProcedure thisParent = tempList0[i];
            foreach (SceneProcedure item in tempList1)
            {
                if (thisParent == item)
                {
                    sameParent = thisParent;
                    i = count0;
                    break;
                }
            }
        }

        return sameParent;
    }

    public bool isThisOrParent(Type type)
    {
        // 判断是否是自己的类型
        if (GetType() == type)
        {
            return true;
        }

        // 判断是否为父节点的类型
        if (parentProcedure != null)
        {
            return parentProcedure.isThisOrParent(type);
        }

        // 没有父节点,返回false
        return false;
    }

    public GameScene getGameScene()
    {
        return scene;
    }

    public SceneProcedure getParent()
    {
        return parentProcedure;
    }

    public SceneProcedure getPrepareNext()
    {
        return mPrepareNext;
    }

    // 是否正在准备退出流程
    public bool isPreparingExit()
    {
        return mPrepareTimer.isCounting();
    }

    public SceneProcedure getParent(Type type)
    {
        // 没有父节点,返回null
        if (parentProcedure == null)
        {
            return null;
        }

        // 有父节点,则判断类型是否匹配,匹配则返回父节点
        if (parentProcedure.GetType() == type)
        {
            return parentProcedure;
        }

        // 不匹配,则继续向上查找
        return parentProcedure.getParent(type);
    }

    public SceneProcedure getThisOrParent(Type type)
    {
        if (GetType() == type)
        {
            return this;
        }

        return getParent(type);
    }

    public SceneProcedure getChildProcedure(Type type)
    {
        return subProcedures.get(type);
    }

    public bool addChildProcedure(SceneProcedure child)
    {
        if (child == null || !subProcedures.TryAdd(child.GetType(), child))
        {
            return false;
        }

        child.setParent(this);
        return true;
    }

    //------------------------------------------------------------------------------------------------------------------------------
    // 从自己的子流程进入当前流程
    protected virtual void onInitFromChild(SceneProcedure lastProcedure)
    {
    }

    // 在进入流程时调用
    // 在onInit中如果要跳转流程,必须使用延迟命令进行跳转
    protected virtual void onInit(SceneProcedure lastProcedure)
    {
    }

    // 更新流程时调用
    protected virtual void onUpdate(float elapsedTime)
    {
    }

    protected virtual void onLateUpdate(float elapsedTime)
    {
    }

    // 更新流程时调用
    protected virtual void onKeyProcess(float elapsedTime)
    {
    }

    // 退出当前流程,进入的不是自己的子流程时调用
    protected virtual void onExit(SceneProcedure next)
    {
    }

    // 退出当前流程,进入自己的子流程时调用
    protected virtual void onExitToChild(SceneProcedure next)
    {
    }

    // 退出当前流程进入其他任何流程时调用
    protected virtual void onExitSelf()
    {
    }

    protected virtual void onPrepareExit(SceneProcedure next)
    {
    }

    protected bool setParent(SceneProcedure parent)
    {
        if (parentProcedure != null)
        {
            return false;
        }

        parentProcedure = parent;
        return true;
    }

    // 专为exit流程而封装的一些通用卸载逻辑
    protected void genericExit(int tag = 0)
    {
        // 一般在场景的Exit流程中,卸载该场景的所有布局,确保没有资源遗留
        mLayoutManager.unloadAllPartLayout();
        // 0通常属于无效tag,直接卸载tag为0的资源可能会有很多意料之外的问题
        if (tag > 0)
        {
            // 先销毁所有指定tag的特效
            mEffectManager.destroyAllEffectWithTag(tag);
            // 销毁所有指定tag的资源
            mPrefabPoolManager.destroyAllWithTag(tag);
            // 如果上一步中有销毁特效,则需要清除无效的特效对象
            mEffectManager.clearInvalidEffect();
        }
    }
}