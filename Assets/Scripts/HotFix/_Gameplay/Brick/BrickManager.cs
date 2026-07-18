using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains;

// 角色管理器
public class BrickManager : FrameSystem
    , IEvent<OnBrickDeath>
    , IEvent<OnBrickDeathTotally>
{
    //key: gameObject.GetInstanceID()
    protected Dictionary<int, Brick> activeBricks = new();
    protected List<Brick> activeBrickList = new();
    protected Dictionary<Type, Dictionary<long, Brick>> brickTypeList = new(); // 角色分类列表
    protected Dictionary<long, Brick> brickGUIDList = new(); // 角色ID索引表

    protected Dictionary<(Type, Vector2Int), ObjectPool<Brick>> brickPools = new();

    public BrickGridLayout brickLayout;

    public BrickManager()
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
        destroyAllBrick();
        brickTypeList = null;
        brickGUIDList = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
    }

    public override void fixedUpdate(float elapsedTime)
    {
        base.fixedUpdate(elapsedTime);
    }

    public void load()
    {
        brickLayout = new(levelManager.getBorderSize(), levelManager.cols, levelManager.rows);
    }

    public Brick getBrick(long id)
    {
        return brickGUIDList.get(id);
    }

    public Brick getActiveBrick(int instanceID)
    {
        return activeBricks.get(instanceID);
    }

    public bool getActiveBrick(int instanceID, out Brick brick)
    {
        brick = activeBricks.get(instanceID);
        return brick != null;
    }

    public bool getRandomActiveBrick(out Brick randomBrick, Brick except = null)
    {
        using var _ = new ListScope<Brick>(out var list);
        list.addRange(activeBricks.Values);
        if (except)
            list.Remove(except);

        var randomIndex = randomInt(0, list.Count - 1);
        randomBrick = list.get(randomIndex);
        return randomBrick != null;
    }

    public bool getRandomActiveBrick(out Brick randomBrick, List<Brick> excepts, Vector2 excludeCenter, float excludeRange = 0F)
    {
        using var _ = new ListScope<Brick>(out var list);
        list.addRange(activeBricks.Values);
        if (excepts.count() > 0)
        {
            foreach (var except in excepts)
                list.Remove(except);
        }

        if (excludeRange > 0F)
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                Vector2 pos = list[i].getWorldPosition();
                if ((excludeCenter - pos).sqrMagnitude <= excludeRange * excludeRange)
                {
                    list.RemoveAt(i);
                }
            }
        }

        var randomIndex = randomInt(0, list.Count - 1);
        randomBrick = list.get(randomIndex);
        return randomBrick != null;
    }

    public bool getRandomActiveBricks(ref List<Brick> randomBricks, int count, Brick except = null)
    {
        using var _ = new ListScope<Brick>(out var list);
        list.addRange(activeBricks.Values);
        if (except)
            list.Remove(except);

        using var __ = new ListScope<int>(out var selectedIndexes);
        randomSelect(list.Count, count, selectedIndexes);
        foreach (var index in selectedIndexes)
            randomBricks.addUnique(list.get(index));

        return randomBricks.any();
    }

    public Dictionary<int, Brick> getActiveBricks()
    {
        return activeBricks;
    }

    public List<Brick> getActiveBrickList()
    {
        return activeBrickList;
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

    public Brick acquireBrick(Vector2 pos, Vector2Int size)
    {
        return acquireBrick(typeof(Brick), pos, size);
    }

    public Brick acquireBrick(Type type, Vector2 pos, Vector2Int size)
    {
        if (!brickPools.TryGetValue((type, size), out var pool))
        {
            pool = new(
                createFunc: () => createBrick(type, size),
                actionOnGet: brick =>
                {
                    brick.setActive(true);
                    // brick.setEnabled(true);
                    activeBricks[brick.instanceID] = brick;
                },
                actionOnRelease: brick =>
                {
                    brick.setActive(false);
                    // brick.setEnabled(false);
                    activeBricks.Remove(brick.instanceID);
                },
                actionOnDestroy: destroyBrick,
                collectionCheck: true,
                defaultCapacity: 1000,
                maxSize: 1000);

            brickPools.add((type, size), pool);
        }

        var brick = pool.Get();
        brick.setWorldPosition(pos);

        var sortingOrder = brickLayout.getSortingOrderAtPosY(pos.y);
        brick.setSortingOrder(sortingOrder);
        brick.onAcquire();
        brick.RespawnAt(pos);

        activeBrickList.add(brick);
        return brick;
    }

    Brick createBrick(Vector2Int size) => createBrick(typeof(Brick), size);

    T createBrick<T>(Vector2Int size) where T : Brick => createBrick(typeof(T), size) as T;

    Brick createBrick(Type type, Vector2Int size)
    {
        var id = generateGUID();

        if (brickGUIDList.ContainsKey(id))
        {
            logError("there is a brick id : " + id + "! can not create again!");
            return null;
        }

        var path = $"{GAMEPLAY_PATH}/Bricks/Brick_{size.x}x{size.y}.prefab";
        var o = prefabPool.createObject(path);
        o.TryGetComponent<Brick>(out var brick);
        brick.setName($"Brick_{activeBricks.Count + 1}");
        brick.setSize(size);
        brick.setID(id);

        brick.Event.addListener<OnBrickDeath>(this);
        brick.Event.addListener<OnBrickDeathTotally>(this);

        addBrickToList(brick);
        return brick;
    }

    public void onEvent(OnBrickDeath e)
    {
        activeBricks.Remove(e.brick.instanceID);
        activeBrickList.Remove(e.brick);
    }

    public void onEvent(OnBrickDeathTotally e)
    {
        releaseBrick(e.brick);
    }

    public void releaseBrick(Brick brick)
    {
        if (brickPools.TryGetValue((brick.getType(), brick.getSize()), out var pool))
        {
            brick.onRelease();
            pool.Release(brick);
        }
    }

    public void destroyAllBrick()
    {
        brickTypeList.Clear();
    }

    void destroyBrick(Brick brick)
    {
        if (brick == null)
            return;

        prefabPool.destroyObject(brick.gameObject, false);

        long guid = brick.getGUID();
        // 从角色分类列表中移除
        brickTypeList.get(brick.getType())?.Remove(guid);
        // 从ID索引表中移除
        brickGUIDList.Remove(guid);

        brick.Event.removeListener<OnBrickDeath>(this);
        brick.Event.removeListener<OnBrickDeathTotally>(this);
    }

    public void destroyBrickList<T>(List<T> characterList) where T : Brick
    {
        foreach (T brick in characterList.safe())
        {
            long guid = brick.getGUID();
            // 从角色分类列表中移除
            brickTypeList.get(brick.getType())?.Remove(guid);
            // 从ID索引表中移除
            brickGUIDList.Remove(guid);
        }
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
    }

    public bool containsBrickAt(Rect rect)
    {
        foreach (var brick in activeBrickList)
        {
            if (brick.getRect().Overlaps(rect))
            {
                return true;
            }
        }

        return false;
    }

    const int NUMBER = 1000;

    public void refreshBrickSortingOrder()
    {
        using var _ = new ListScope<Brick>(out var list);
        list.setRange(activeBrickList);
        list.Sort((b1, b2) =>
        {
            var b2Pos = b2.getWorldPosition();
            var b1Pos = b1.getWorldPosition();

            int b2Y = (int)(b2Pos.y * NUMBER);
            int b1Y = (int)(b1Pos.y * NUMBER);
            var result = b2Y.CompareTo(b1Y);
            if (result == 0)
            {
                int b2X = (int)(b2Pos.x * NUMBER);
                int b1X = (int)(b1Pos.x * NUMBER);
                return b1X.CompareTo(b2X);
            }

            return result;
        });

        for (var i = 0; i < list.Count; i++)
        {
            var brick = list[i];
            var posY = brick.getWorldPosition().y;
            var sortingOrder = brickLayout.getSortingOrderAtPosY(posY);
            brick.setSortingOrder(sortingOrder);
        }
    }
}