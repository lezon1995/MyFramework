using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

namespace MarbleHero;

// 角色管理器
public class BallManager : FrameSystem
{
    protected Dictionary<int, Ball> balls = new();
    protected Dictionary<Type, Dictionary<long, Ball>> ballTypeList = new(); // 角色分类列表
    protected SafeDictionary<long, Ball> ballUpdateList = new(); // 用于更新角色的列表
    protected Dictionary<long, Ball> ballGUIDList = new(); // 角色ID索引表
    protected SafeDictionary<long, Ball> ballFixedUpdateList = new(); // 需要在FixedUpdate中更新的列表,如果直接使用mBallGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储

    Action<GameObject, Ball> ballObjectSet;

    public BallManager()
    {
        mCreateObject = true;
        ballObjectSet = onBallObjectSet;
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
        using var a = new SafeDictionaryReader<long, Ball>(ballUpdateList);
        foreach (var ball in a.mReadList.Values)
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
        using var a = new SafeDictionaryReader<long, Ball>(ballUpdateList);
        foreach (var ball in a.mReadList.Values)
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
        using var a = new SafeDictionaryReader<long, Ball>(ballFixedUpdateList);
        foreach (var ball in a.mReadList.Values)
        {
            if (ball && ball.isActiveInHierarchy())
            {
                ball.fixedUpdate(elapsedTime);
            }
        }
    }

    public Ball getBall(long id)
    {
        return ballGUIDList.get(id);
    }

    public Ball getBall(int instanceID)
    {
        return balls.get(instanceID);
    }

    public bool getBall(int instanceID, out Ball ball)
    {
        ball = balls.get(instanceID);
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

    public T createBall<T>(string name, Vector2 pos, float radius, Vector2 direction, float speed) where T : Ball
    {
        return createBall(name, typeof(T), pos, radius, direction, speed) as T;
    }

    public Ball createBall(string name, Type type, Vector2 pos, float radius, Vector2 direction, float speed)
    {
        var id = generateGUID();

        if (ballGUIDList.ContainsKey(id))
        {
            logError("there is a ball id : " + id + "! can not create again!");
            return null;
        }

        var ball = CLASS<Ball>(type);
        ball.setName(name);
        ball.setBallType(type);
        ball.setOnObjectSet(ballObjectSet);

        // 将角色挂接到管理器下
        ball.setID(id);

        var path = $"{GAMEPLAY_PATH}/{name}.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        ball.setObject(o);
        ball.setPosition(pos);
        ball.setRadius(radius);
        ball.setDirection(direction);
        ball.setSpeed(speed);

        ball.init();
        addBallToList(ball);
        return ball;
    }

    void onBallObjectSet(GameObject obj, Ball ball)
    {
        balls[obj.GetInstanceID()] = ball;
    }

    public void destroyAllBall()
    {
        UN_CLASS_LIST(ballGUIDList);
        ballTypeList.Clear();
        ballUpdateList.clear();
        ballFixedUpdateList.clear();
    }

    public void destroyBall(long id)
    {
        destroyBall(getBall(id));
    }

    public void destroyBall(Ball ball)
    {
        if (ball == null)
            return;

        mPrefabPoolManager.destroyObject(ball.getObject(), false);

        long guid = ball.getGUID();
        // 从角色分类列表中移除
        ballTypeList.get(ball.getType())?.Remove(guid);
        // 从ID索引表中移除
        ballUpdateList.remove(guid);
        ballGUIDList.Remove(guid);
        ballFixedUpdateList.remove(guid);

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
            ballUpdateList.remove(guid);
            ballGUIDList.Remove(guid);
            ballFixedUpdateList.remove(guid);
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
            ballUpdateList.remove(guid);
            ballGUIDList.Remove(guid);
            ballFixedUpdateList.remove(guid);
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

        ballUpdateList.add(guid, ball);
        if (ball.isEnableFixedUpdate())
        {
            ballFixedUpdateList.add(guid, ball);
        }
    }
}