using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarbleHero;

// 角色管理器
public class BrickManager : FrameSystem
{
    //key: gameObject.GetInstanceID()
    protected Dictionary<int, Brick> bricks = new();
    protected Dictionary<Type, Dictionary<long, Brick>> brickTypeList = new(); // 角色分类列表
    protected Dictionary<long, Brick> brickGUIDList = new(); // 角色ID索引表
    protected SafeDictionary<long, Brick> brickUpdateList = new(); // 用于更新角色的列表
    protected SafeDictionary<long, Brick> brickFixedUpdateList = new(); // 需要在FixedUpdate中更新的列表,如果直接使用mBrickGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储

    Action<GameObject, Brick> brickObjectSet;
    Action<Brick> brickDead;

    public BrickManager()
    {
        mCreateObject = true;
        brickObjectSet = onBrickObjectSet;
        brickDead = onBrickDead;
    }

    public override void init()
    {
        base.init();
    }

    public override void destroy()
    {
        base.destroy();
        destroyAllBrick();
        brickTypeList = null;
        brickGUIDList = null;
        brickFixedUpdateList = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        using var a = new SafeDictionaryReader<long, Brick>(brickUpdateList);
        foreach (var brick in a.mReadList.Values)
        {
            if (brick && brick.isActiveInHierarchy())
            {
                var dt = !brick.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                brick.update(dt);
            }
        }
    }

    public override void lateUpdate(float elapsedTime)
    {
        base.lateUpdate(elapsedTime);
        using var a = new SafeDictionaryReader<long, Brick>(brickUpdateList);
        foreach (var brick in a.mReadList.Values)
        {
            if (brick && brick.isActiveInHierarchy())
            {
                var dt = !brick.isIgnoreTimeScale() ? elapsedTime : Time.unscaledDeltaTime;
                brick.lateUpdate(dt);
            }
        }
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
        using var a = new SafeDictionaryReader<long, Brick>(brickFixedUpdateList);
        foreach (var brick in a.mReadList.Values)
        {
            if (brick && brick.isActiveInHierarchy())
            {
                var dt = !brick.isIgnoreTimeScale() ? elapsedTime : Time.fixedUnscaledDeltaTime;
                brick.fixedUpdate(dt);
            }
        }
    }

    public Brick getBrick(long id)
    {
        return brickGUIDList.get(id);
    }

    public Brick getBrick(int instanceID)
    {
        return bricks.get(instanceID);
    }

    public bool getBrick(int instanceID, out Brick brick)
    {
        brick = bricks.get(instanceID);
        return brick != null;
    }

    public Dictionary<long, Brick> getBrickList()
    {
        return brickGUIDList;
    }

    public Dictionary<long, Brick> getBrickListByType<T>() where T : Brick
    {
        return brickTypeList.get(typeof(T));
    }

    public Dictionary<long, Brick> getBrickListByType(Type type)
    {
        return brickTypeList.get(type);
    }

    public T createBrick<T>(string name, Vector2 pos) where T : Brick
    {
        return createBrick(name, typeof(T), pos) as T;
    }

    public Brick createBrick(string name, Type type, Vector2 pos)
    {
        var id = generateGUID();

        if (brickGUIDList.ContainsKey(id))
        {
            logError("there is a brick id : " + id + "! can not create again!");
            return null;
        }

        var brick = CLASS<Brick>(type);
        brick.setName(name);
        brick.setBrickType(type);
        brick.setOnObjectSet(brickObjectSet);
        brick.setOnDead(brickDead);

        // 将角色挂接到管理器下
        brick.setID(id);

        var path = $"{GAMEPLAY_PATH}/{name}.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        brick.setObject(o);
        brick.setPosition(pos);
        brick.setHealth(20);
        brick.setMaxHealth(20);

        brick.init();
        addBrickToList(brick);
        return brick;
    }

    void onBrickObjectSet(GameObject obj, Brick brick)
    {
        bricks[brick.instanceID] = brick;
    }

    void onBrickDead(Brick brick)
    {
        destroyBrick(brick);
    }

    public void destroyAllBrick()
    {
        UN_CLASS_LIST(brickGUIDList);
        brickTypeList.Clear();
        brickUpdateList.clear();
        brickFixedUpdateList.clear();
    }

    public void destroyBrick(long id)
    {
        destroyBrick(getBrick(id));
    }

    public void destroyBrick(Brick brick)
    {
        if (brick == null)
            return;

        mPrefabPoolManager.destroyObject(brick.getObject(), false);

        long guid = brick.getGUID();
        // 从角色分类列表中移除
        brickTypeList.get(brick.getType())?.Remove(guid);
        // 从ID索引表中移除
        brickUpdateList.remove(guid);
        brickGUIDList.Remove(guid);
        brickFixedUpdateList.remove(guid);

        UN_CLASS(ref brick);
    }

    public void destroyBrickList<T>(IList<T> characterList) where T : Brick
    {
        foreach (T brick in characterList.safe())
        {
            long guid = brick.getGUID();
            // 从角色分类列表中移除
            brickTypeList.get(brick.getType())?.Remove(guid);
            // 从ID索引表中移除
            brickUpdateList.remove(guid);
            brickGUIDList.Remove(guid);
            brickFixedUpdateList.remove(guid);
        }

        UN_CLASS_LIST(characterList);
    }

    public void destroyBrickList<T0, T1>(IDictionary<T0, T1> characterList) where T1 : Brick
    {
        foreach (T1 brick in (characterList?.Values).safe())
        {
            long guid = brick.getGUID();
            // 从角色分类列表中移除
            brickTypeList.get(brick.getType())?.Remove(guid);
            // 从ID索引表中移除
            brickUpdateList.remove(guid);
            brickGUIDList.Remove(guid);
            brickFixedUpdateList.remove(guid);
        }

        UN_CLASS_LIST(characterList);
    }

    //------------------------------------------------------------------------------------------------------------------------------
    protected void addBrickToList(Brick brick)
    {
        if (brick == null)
            return;

        long guid = brick.getGUID();
        // 加入到角色分类列表
        brickTypeList.getOrAddNew(brick.getType()).Add(guid, brick);
        // 加入ID索引表
        if (!brickGUIDList.TryAdd(guid, brick))
        {
            logError("there is a brick id : " + guid + ", can not add again!");
        }

        brickUpdateList.add(guid, brick);
        if (brick.isEnableFixedUpdate())
        {
            brickFixedUpdateList.add(guid, brick);
        }
    }
}