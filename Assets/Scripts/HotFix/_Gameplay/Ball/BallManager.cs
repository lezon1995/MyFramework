using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains;

// 角色管理器
public class BallManager : FrameSystem
    , IEvent<OnBallDeath>
    , IEvent<OnBallDeathTotally>
{
    //key: gameObject.GetInstanceID()
    public Dictionary<int, Ball> activeBalls = new(); //发射后运动中的Ball
    protected Dictionary<int, Ball> inactiveBalls = new(); //回到底板后待发射的Ball
    protected Dictionary<Type, Dictionary<long, Ball>> ballTypeList = new(); // 角色分类列表
    protected Dictionary<long, Ball> ballGUIDList = new(); // 角色ID索引表

    protected Dictionary<Type, ObjectPool<Ball>> ballPools = new();

    public BallManager()
    {
        mCreateObject = true;
    }

    public override void init()
    {
        base.init();
    }

    public override void destroy()
    {
        base.destroy();
        destroyAllBall();
        ballTypeList = null;
        ballGUIDList = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
    }

    public Ball getBall(long id)
    {
        return ballGUIDList.get(id);
    }

    public Ball getActiveBall(int instanceID)
    {
        return activeBalls.get(instanceID);
    }

    public bool getActiveBall(int instanceID, out Ball ball)
    {
        ball = activeBalls.get(instanceID);
        return ball != null;
    }

    public Dictionary<long, Ball> getBallList()
    {
        return ballGUIDList;
    }

    public Dictionary<long, Ball> getBallListByType<T>() where T : Ball
    {
        return ballTypeList.get(typeof(T));
    }

    public Dictionary<long, Ball> getBallListByType(Type type)
    {
        return ballTypeList.get(type);
    }
    
    public Ball acquireBall()
    {
        return acquireBall(typeof(Ball), Vector2.zero, Vector2.up);
    }
    
    public Ball acquireBall(Vector2 pos)
    {
        return acquireBall(typeof(Ball), pos, Vector2.up);
    }

    public Ball acquireBall(Vector2 pos, Vector2 direction)
    {
        return acquireBall(typeof(Ball), pos, direction);
    }

    public Ball acquireBall(Type type, Vector2 pos, Vector2 direction)
    {
        if (!ballPools.TryGetValue(type, out var pool))
        {
            pool = new(
                createFunc: () => createBall(type),
                actionOnGet: ball =>
                {
                    ball.setActive(true);
                    ball.setEnabled(true);

                    activeBalls[ball.instanceID] = ball;
                    inactiveBalls.Remove(ball.instanceID);
                },
                actionOnRelease: ball =>
                {
                    ball.setActive(false);
                    ball.setEnabled(false);

                    activeBalls.Remove(ball.instanceID);
                    inactiveBalls[ball.instanceID] = ball;
                },
                actionOnDestroy: destroyBall,
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 100);

            ballPools.add(type, pool);
        }

        var ball = pool.Get();
        ball.setTeleportPosition(pos, BORDER_BOT_LAYER_MASK);
        ball.setShootDirection(direction);
        ball.setInitialHealth(int.MaxValue);
        ball.setRendererActive(true);
        ball.onAcquire();
        
        ball.Event.addListener<OnBallDeath>(this);
        ball.Event.addListener<OnBallDeathTotally>(this);
        return ball;
    }

    Ball createBall() => createBall(typeof(Ball));

    T createBall<T>() where T : Ball => createBall(typeof(T)) as T;

    Ball createBall(Type type)
    {
        var id = generateGUID();

        if (ballGUIDList.ContainsKey(id))
        {
            logError("there is a ball id : " + id + "! can not create again!");
            return null;
        }

        var path = $"{GAMEPLAY_PATH}/Balls/Ball_0.prefab";
        var o = prefabPool.createObject(path);
        o.TryGetComponent(out Ball ball);
        ball.setName($"Ball_{activeBalls.Count + 1}");
        ball.setBallType(type);
        // 将角色挂接到管理器下
        ball.setID(id);
        ball.setPlayer(player);
        ball.setEnabled(true);
        addBallToList(ball);
        return ball;
    }

        public void releaseBall(Ball ball)
        {
            if (ballPools.TryGetValue(ball.getType(), out var pool))
            {
                ball.onRelease();
                pool.Release(ball);
            }
        }

    public void destroyAllBall()
    {
        ballTypeList.Clear();
    }

    void destroyBall(Ball ball)
    {
        if (ball == null)
            return;

        prefabPool.destroyObject(ball.gameObject, false);

        long guid = ball.getGUID();
        // 从角色分类列表中移除
        ballTypeList.get(ball.getType())?.Remove(guid);
        // 从ID索引表中移除
        ballGUIDList.Remove(guid);

        ball.Event.removeListener<OnBallDeath>(this);
        ball.Event.removeListener<OnBallDeathTotally>(this);
    }

    public void destroyBallList<T>(List<T> characterList) where T : Ball
    {
        foreach (T ball in characterList.safe())
        {
            long guid = ball.getGUID();
            // 从角色分类列表中移除
            ballTypeList.get(ball.getType())?.Remove(guid);
            // 从ID索引表中移除
            ballGUIDList.Remove(guid);
        }

    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void addBallToList(Ball ball)
    {
        if (ball == null)
            return;

        long guid = ball.getGUID();
        // 加入到角色分类列表
        ballTypeList.getOrAddNew(ball.getType()).Add(guid, ball);
        // 加入ID索引表
        if (!ballGUIDList.TryAdd(guid, ball))
        {
            logError("there is a ball id : " + guid + ", can not add again!");
        }
    }

    public bool anyActiveBall()
    {
        return activeBalls.Count > 0;
    }

    public void onEvent(OnBallDeath e)
    {
        activeBalls.Remove(e.ball.instanceID);
    }

    public void onEvent(OnBallDeathTotally e)
    {
        releaseBall(e.ball);
    }
}