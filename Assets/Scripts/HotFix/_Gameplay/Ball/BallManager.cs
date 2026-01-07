using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MarbleHero;

// 角色管理器
public class BallManager : FrameSystem
{
    //key: gameObject.GetInstanceID()
    protected Dictionary<int, Ball> activeBalls = new();//发射后运动中的Ball
    protected Dictionary<int, Ball> inactiveBalls = new();//回到底板后待发射的Ball
    protected Dictionary<Type, Dictionary<long, Ball>> ballTypeList = new(); // 角色分类列表
    protected Dictionary<long, Ball> ballGUIDList = new(); // 角色ID索引表
    protected SafeList<Ball> ballUpdateList = new(); // 用于更新角色的列表
    protected SafeList<Ball> ballFixedUpdateList = new(); // 需要在FixedUpdate中更新的列表,如果直接使用mBallGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储

    protected Dictionary<Type, ObjectPool<Ball>> ballPools = new();

    Action<Ball> ballDead;

    public BallManager()
    {
        mCreateObject = true;
        ballDead = onBallDead;
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
        ballFixedUpdateList = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        using var a = new SafeListReader<Ball>(ballUpdateList);
        foreach (var ball in a.mReadList)
        {
            if (ball && ball.isActiveInHierarchy())
            {
                var dt = !ball.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                ball.update(dt);
            }
        }
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
        using var a = new SafeListReader<Ball>(ballUpdateList);
        foreach (var ball in a.mReadList)
        {
            if (ball && ball.isActiveInHierarchy())
            {
                var dt = !ball.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                ball.lateUpdate(dt);
            }
        }
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
        using var a = new SafeListReader<Ball>(ballFixedUpdateList);
        foreach (var ball in a.mReadList)
        {
            if (ball && ball.isActiveInHierarchy())
            {
                var dt = !ball.isIgnoreTimeScale() ? elapsedTime : Time.fixedUnscaledDeltaTime;
                ball.fixedUpdate(dt);
            }
        }
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

    public Ball acquireBall(Vector2 pos, float radius, Vector2 direction, float speed)
    {
        return acquireBall(typeof(Ball), pos, radius, direction, speed);
    }

    public Ball acquireBall(Type type, Vector2 pos, float radius, Vector2 direction, float speed)
    {
        if (!ballPools.TryGetValue(type, out var pool))
        {
            pool = new(
                createFunc: () =>
                {
                    return createBall(type, pos, radius, direction, speed);
                },
                actionOnGet: ball =>
                {
                    ball.setActive(true);
                    ball.setEnabled(true);

                    ballUpdateList.add(ball);
                    ballFixedUpdateList.add(ball);
                    activeBalls[ball.instanceID] = ball;
                    inactiveBalls.Remove(ball.instanceID);
                },
                actionOnRelease: ball =>
                {
                    ball.setActive(false);
                    ball.setEnabled(false);

                    ballUpdateList.remove(ball);
                    ballFixedUpdateList.remove(ball);
                    activeBalls.Remove(ball.instanceID);
                    inactiveBalls[ball.instanceID] = ball;
                },
                actionOnDestroy: ball =>
                {
                    destroyBall(ball);
                },
                collectionCheck: true,
                defaultCapacity: 100,
                maxSize: 100);

            ballPools.add(type, pool);
        }

        var ball = pool.Get();
        ball.setTeleportPosition(pos);
        ball.setRadius(radius);
        ball.setShootDirection(direction);
        ball.setSpeed(speed);
        ball.setPhysicDamage(1, 1);
        ball.setMagicDamage(1, 1);
        ball.onAcquire();
        return ball;
    }


    Ball createBall(Vector2 pos, float radius, Vector2 direction, float speed)
    {
        return createBall(typeof(Ball), pos, radius, direction, speed);
    }

    T createBall<T>(Vector2 pos, float radius, Vector2 direction, float speed) where T : Ball
    {
        return createBall(typeof(T), pos, radius, direction, speed) as T;
    }

    Ball createBall(Type type, Vector2 pos, float radius, Vector2 direction, float speed)
    {
        var id = generateGUID();

        if (ballGUIDList.ContainsKey(id))
        {
            logError("there is a ball id : " + id + "! can not create again!");
            return null;
        }

        var ball = CLASS<Ball>(type);
        ball.setName($"Ball_{activeBalls.Count + 1}");
        ball.setBallType(type);
        ball.setOnDead(ballDead);

        // 将角色挂接到管理器下
        ball.setID(id);

        var path = $"{GAMEPLAY_PATH}/Prefabs/Play/Ball_0.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        ball.setObject(o);
        ball.setTeleportPosition(pos);
        ball.setRadius(radius);
        ball.setShootDirection(direction);
        ball.setSpeed(speed);
        ball.setPhysicDamage(1, 1);
        ball.setMagicDamage(1, 1);
        ball.setPlayer(playerManager.getPlayer());
        ball.setEnabled(true);

        ball.init();
        addBallToList(ball);
        return ball;
    }

    void onBallDead(Ball ball)
    {
        releaseBall(ball);
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
        UN_CLASS_LIST(ballGUIDList);
        ballTypeList.Clear();
        ballUpdateList.clear();
        ballFixedUpdateList.clear();
    }

    void destroyBall(Ball ball)
    {
        if (ball == null)
            return;

        mPrefabPoolManager.destroyObject(ball.getObject(), false);

        long guid = ball.getGUID();
        // 从角色分类列表中移除
        ballTypeList.get(ball.getType())?.Remove(guid);
        // 从ID索引表中移除
        ballGUIDList.Remove(guid);

        ballUpdateList.remove(ball);
        ballFixedUpdateList.remove(ball);

        UN_CLASS(ref ball);
    }

    public void destroyBallList<T>(IList<T> characterList) where T : Ball
    {
        foreach (T ball in characterList.safe())
        {
            long guid = ball.getGUID();
            // 从角色分类列表中移除
            ballTypeList.get(ball.getType())?.Remove(guid);
            // 从ID索引表中移除
            ballGUIDList.Remove(guid);

            ballUpdateList.remove(ball);
            ballFixedUpdateList.remove(ball);
        }

        UN_CLASS_LIST(characterList);
    }

    public void destroyBallList<T0, T1>(IDictionary<T0, T1> characterList) where T1 : Ball
    {
        foreach (T1 ball in (characterList?.Values).safe())
        {
            long guid = ball.getGUID();
            // 从角色分类列表中移除
            ballTypeList.get(ball.getType())?.Remove(guid);
            // 从ID索引表中移除
            ballGUIDList.Remove(guid);

            ballUpdateList.remove(ball);
            ballFixedUpdateList.remove(ball);
        }

        UN_CLASS_LIST(characterList);
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
}