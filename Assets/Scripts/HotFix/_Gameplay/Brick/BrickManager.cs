using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MarbleHero;

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
    protected SafeList<Brick> brickUpdateList = new(); // 用于更新角色的列表
    protected SafeList<Brick> brickFixedUpdateList = new(); // 需要在FixedUpdate中更新的列表,如果直接使用mBrickGUIDList,会非常慢,而很多时候其实并不需要进行物理更新,所以单独使用一个列表存储

    protected Dictionary<Type, ObjectPool<Brick>> brickPools = new();

    protected Sprite[] brickSprites;
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
        brickFixedUpdateList = null;
    }

    public override void update(float elapsedTime)
    {
        base.update(elapsedTime);
        using var a = new SafeListReader<Brick>(brickUpdateList);
        foreach (var brick in a.mReadList)
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
        using var a = new SafeListReader<Brick>(brickUpdateList);
        foreach (var brick in a.mReadList)
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
        using var a = new SafeListReader<Brick>(brickFixedUpdateList);
        foreach (var brick in a.mReadList)
        {
            if (brick && brick.isActiveInHierarchy())
            {
                var dt = !brick.isIgnoreTimeScale() ? elapsedTime : Time.fixedUnscaledDeltaTime;
                brick.fixedUpdate(dt);
            }
        }
    }

    public void load()
    {
        brickSprites = new Sprite[26];

        for (int i = 0; i < brickSprites.Length; i++)
        {
            var path = $"{GAMEPLAY_PATH}/Sprites/Play/_Blocks/box_{i}.png";
            var sprite = mResourceManager.loadGameResource<Sprite>(path);
            brickSprites[i] = sprite.getResource();
        }

        brickLayout = new(levelManager.getBorderSize(), 6, 10);
        // var grids = brickGrid.getGrids();
        // for (var i = 0; i < grids.Count; i++)
        // {
        //     var grid = grids[i];
        //     createBrick<NormalBrick>("Brick", grid.center, grid.size, 60);
        // }
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

    public Sprite getBrickSprite(int index)
    {
        if (brickSprites.tryGet(index, out var sprite))
            return sprite;

        return null;
    }

    public Sprite getBrickSpriteByHealth(int health)
    {
        int value = health;
        int count = brickSprites.Length;
        int index;
        int v = clampMin(value - 1) / count;
        if (v == 0)
        {
            index = clampMin(value - 1) % count;
        }
        else
        {
            index = (value - 2) % (count - 1) + 1;
        }

        if (brickSprites.tryGet(index, out var sprite))
            return sprite;

        return null;
    }

    public Brick acquireBrick(Vector2 pos, Vector2 size, int health)
    {
        return acquireBrick(typeof(Brick), pos, size, health);
    }

    public Brick acquireBrick(Type type, Vector2 pos, Vector2 size, int health)
    {
        if (!brickPools.TryGetValue(type, out var pool))
        {
            pool = new(
                createFunc: () =>
                {
                    return createBrick(type, pos, size, health);
                },
                actionOnGet: brick =>
                {
                    brick.setActive(true);
                    // brick.setEnabled(true);

                    brickUpdateList.add(brick);
                    brickFixedUpdateList.add(brick);
                    activeBricks[brick.instanceID] = brick;
                },
                actionOnRelease: brick =>
                {
                    brick.setActive(false);
                    // brick.setEnabled(false);

                    brickUpdateList.remove(brick);
                    brickFixedUpdateList.remove(brick);
                    activeBricks.Remove(brick.instanceID);
                },
                actionOnDestroy: brick =>
                {
                    destroyBrick(brick);
                },
                collectionCheck: true,
                defaultCapacity: 1000,
                maxSize: 1000);

            brickPools.add(type, pool);
        }

        var brick = pool.Get();
        brick.setWorldPosition(pos);
        brick.setInitialHealth(health);
        brick.setMaxHealth(health);
        // brick.setSize(1.14F, 0.82F);
        brick.setSize(size);
        brick.onAcquire();

        activeBrickList.add(brick);
        return brick;
    }

    Brick createBrick(Vector2 pos, Vector2 size, int health)
    {
        return createBrick(typeof(Brick), pos, size, health);
    }

    T createBrick<T>(Vector2 pos, Vector2 size, int health) where T : Brick
    {
        return createBrick(typeof(T), pos, size, health) as T;
    }

    Brick createBrick(Type type, Vector2 pos, Vector2 size, int health)
    {
        var id = generateGUID();

        if (brickGUIDList.ContainsKey(id))
        {
            logError("there is a brick id : " + id + "! can not create again!");
            return null;
        }

        var brick = CLASS<Brick>(type);
        brick.setName($"Brick_{activeBricks.Count + 1}");
        brick.setBrickType(type);

        // 将角色挂接到管理器下
        brick.setID(id);

        var path = $"{GAMEPLAY_PATH}/Prefabs/Play/Brick.prefab";
        var o = mPrefabPoolManager.createObject(path, 0, false, true);
        brick.setManager(this);
        brick.setObject(o);
        brick.init();

        brick.setWorldPosition(pos);
        brick.setInitialHealth(health);
        brick.setMaxHealth(health);
        // brick.setSize(1.14F, 0.82F);
        brick.setSize(size);

        brick.eventRouter.addListener<OnBrickDeath>(this);
        brick.eventRouter.addListener<OnBrickDeathTotally>(this);
        
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
        if (brickPools.TryGetValue(brick.getType(), out var pool))
        {
            brick.onRelease();
            pool.Release(brick);
        }
    }

    public void destroyAllBrick()
    {
        UN_CLASS_LIST(brickGUIDList);
        brickTypeList.Clear();
        brickUpdateList.clear();
        brickFixedUpdateList.clear();
    }

    void destroyBrick(Brick brick)
    {
        if (brick == null)
            return;

        mPrefabPoolManager.destroyObject(brick.gameObject, false);

        long guid = brick.getGUID();
        // 从角色分类列表中移除
        brickTypeList.get(brick.getType())?.Remove(guid);
        // 从ID索引表中移除
        brickGUIDList.Remove(guid);
        brickUpdateList.remove(brick);
        brickFixedUpdateList.remove(brick);

        brick.eventRouter.removeListener<OnBrickDeath>(this);
        brick.eventRouter.removeListener<OnBrickDeathTotally>(this);

        UN_CLASS(ref brick);
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
            brickUpdateList.remove(brick);
            brickFixedUpdateList.remove(brick);
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
}