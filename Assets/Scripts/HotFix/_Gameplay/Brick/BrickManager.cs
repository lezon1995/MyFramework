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

    // ---------------------------------------------------------------
    // 网格占用跟踪 (由 WaveManager 等使用, 防止在已被占的 cell 上生成新砖块)
    // ---------------------------------------------------------------

    protected bool[] _cellStates;

    /// <summary>占用的 cell -> 该 cell 上的 brick (同一格上最后一个注册者).</summary>
    protected Dictionary<Vector2Int, Brick> _cellToBrick = new();

    /// <summary>brick -> 它所占的 cell 集合 (用于在 brick 死亡时批量解除).</summary>
    protected Dictionary<Brick, List<Vector2Int>> _brickToCells = new();

    /// <summary>所有当前被占用的 cell (与 _cellToBrick.Keys 等价, 单独缓存方便遍历).</summary>
    public IReadOnlyDictionary<Vector2Int, Brick> OccupiedCells => _cellToBrick;

    Action<Brick> onBrickBornCompleted;

    public BrickManager()
    {
        mCreateObject = true;
        onBrickBornCompleted = OnBrickBornCompleted;
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
        _cellToBrick.Clear();
        _brickToCells.Clear();
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
        brickLayout = new(new(18.9F, 10.8F), 28, 16);
        int cols = brickLayout.getCols();
        int rows = brickLayout.getRows();
        _cellStates = new bool[cols * rows];
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

    /// <summary>
    /// 从形状库生成一组砖块（用于 ShapeEntry 的批量生成）。
    /// </summary>
    /// <param name="shapeOrigin">形状锚点在世界坐标系中的位置（即形状放置的原点）</param>
    /// <param name="bricks">形状中每个砖块的局部坐标 (col/row 为起点的局部坐标, width/height 为尺寸)</param>
    /// <param name="pivot">形状锚点位置（影响局部坐标如何映射到世界坐标）</param>
    /// <returns>生成的砖块列表</returns>
    public bool acquireShape(Vector2Int shapeOrigin, List<GridUnitBrick> bricks, ref List<BrickTemplate> result)
    {
        if (bricks == null || bricks.Count == 0)
            return false;

        if (brickLayout == null)
            return false;

        result.Clear();
        foreach (var b in bricks)
        {
            // 该砖块左下角在世界中的坐标
            var brickPivotCoord = shapeOrigin + b.PivotCell;
            var brickPivotPos = brickLayout.getPos(brickPivotCoord);
            result.Add(new(brickPivotPos, b.Size));
        }

        return true;
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
        var sortingOrder = brickLayout.getSortingOrderAtPosY(pos.y);
        brick.setSortingOrder(sortingOrder);
        brick.setWorldPosition(pos);
        brick.onAcquire();
        brick.RespawnAt(pos);

        activeBrickList.add(brick);

        // 根据当前世界坐标自动注册 cell 占用 (以 brick 左下角对齐到对应网格 cell)
        RegisterOccupancyFromWorld(brick);

        return brick;
    }

    // ---------------------------------------------------------------
    // 占用注册 / 解除
    // ---------------------------------------------------------------

    /// <summary>
    /// 根据 brick 的世界位置和 size 注册它在网格上占的 cells.
    /// 砖块自身的世界坐标视为"中心", size 是 width × height,
    /// 占的矩形 = [center.x - w/2, center.x + w/2) × [center.y - h/2, center.y + h/2).
    ///
    /// 注意: 落在网格边界外的格子会被忽略, 不记录.
    /// </summary>
    public void RegisterOccupancyFromWorld(Brick brick)
    {
        if (brick == null || brickLayout == null)
            return;

        var pos = brick.getWorldPosition();
        var coord = brickLayout.getCoord(pos);
        var size = brick.getSize();
        RegisterOccupancy(brick, coord.x, coord.y, size.x, size.y);
    }

    /// <summary>
    /// 显式注册 brick 在 cells [col, col+w) × [row, row+h) 上的占用 (单位: cell).
    /// 用于 spawn 时已知 col/row 的场景, 精度高于从世界坐标反算.
    ///
    /// 越界自动 clamp: 落在 [0, cols) × [0, rows) 内的格子才会被记录.
    /// </summary>
    public void RegisterOccupancy(Brick brick, int col, int row, int width, int height)
    {
        if (brick == null || brickLayout == null)
            return;

        if (width <= 0 || height <= 0)
            return;

        // 先把同一块 brick 旧的占用清掉 (防止注册两次造成脏数据)
        UnregisterOccupancy(brick);

        int cols = brickLayout.getCols();
        int rows = brickLayout.getRows();

        if (!_brickToCells.TryGetValue(brick, out var list))
        {
            list = ListPool<Vector2Int>.Get();
            _brickToCells[brick] = list;
        }

        for (int dy = 0; dy < height; dy++)
        {
            for (int dx = 0; dx < width; dx++)
            {
                int x = col + dx;
                int y = row + dy;
                if (x < 0 || x >= cols || y < 0 || y >= rows)
                    continue;
                var cell = new Vector2Int(x, y);
                list.Add(cell);
                _cellToBrick[cell] = brick;
                _cellStates[cell.ToIndex()] = true;
            }
        }
    }

    /// <summary>解除 brick 所占的所有 cell.</summary>
    public void UnregisterOccupancy(Brick brick)
    {
        if (brick == null)
            return;

        if (!_brickToCells.TryGetValue(brick, out var list))
            return;

        for (int i = 0; i < list.Count; i++)
        {
            var cell = list[i];
            //if (_cellToBrick.TryGetValue(cell, out var owner) && owner == brick)
            //{
                _cellToBrick.Remove(cell);
                _cellStates[cell.ToIndex()] = false;
            //}
        }

        list.Clear();
        ListPool<Vector2Int>.Release(list);
        _brickToCells.Remove(brick);
    }

    /// <summary>给定 cell 是否被任意 brick 占用.</summary>
    public bool IsCellOccupied(Vector2Int cell)
    {
        return _cellToBrick.ContainsKey(cell);
    }

    /// <summary>给定 cell 是否空闲.</summary>
    public bool IsCellEmpty(Vector2Int cell)
    {
        return !_cellStates[cell.ToIndex()];
    }

    /// <summary>查询 cell 上的 brick.</summary>
    public bool TryGetBrickAtCell(Vector2Int cell, out Brick brick)
    {
        return _cellToBrick.TryGetValue(cell, out brick);
    }

    /// <summary>
    /// 检查形状能否放置在网格的指定 origin 位置。
    /// shapeOrigin 是形状锚点的世界坐标。
    /// </summary>
    public bool CanPlaceShapeAtCell(Vector2Int cell, List<GridUnitBrick> bricks)
    {
        if (brickLayout == null || bricks == null || bricks.Count == 0)
            return false;

        int cols = brickLayout.getCols();
        int rows = brickLayout.getRows();
        foreach (var b in bricks)
        {
            // 该砖块的中心 cell（左下角 + 宽高）
            var curCell = cell + b.PivotCell;
            // 检查所有被该砖块覆盖的 cell
            for (int dy = 0; dy < b.height; dy++)
            {
                for (int dx = 0; dx < b.width; dx++)
                {
                    int cx = curCell.x + dx;
                    int cy = curCell.y + dy;
                    if (cx < 0 || cx >= cols || cy < 0 || cy >= rows)
                        return false;
                    if (IsCellOccupied(new(cx, cy)))
                        return false;
                }
            }
        }

        return true;
    }

    /// <summary>查询 shape origin 附近在形状占据范围内的第一个空置位置。</summary>
    public bool FindEmptyCellForShape(Vector2Int targetCell, List<GridUnitBrick> bricks, out Vector2Int result, int maxRetries = 20)
    {
        result = default;
        if (brickLayout == null || bricks == null || bricks.Count == 0)
            return false;

        // 从 nearPosition 的 grid cell 开始，向外扩散搜索
        int startCol = targetCell.x;
        int startRow = targetCell.y;

        // 优先搜索边缘附近的空位（模仿 edge-biased 行为）
        for (int r = 0; r < maxRetries; r++)
        {
            // 在半径 r 的环形区域搜索
            int minC = Math.Max(0, startCol - r);
            int maxC = Math.Min(brickLayout.getCols() - 1, startCol + r);
            int minR2 = Math.Max(0, startRow - r);
            int maxR2 = Math.Min(brickLayout.getRows() - 1, startRow + r);

            // 收集这一圈的所有候选 cell
            using var _ = new ListScope<Vector2Int>(out var candidates);
            for (int c = minC; c <= maxC; c++)
            {
                candidates.Add(new(c, minR2));
                if (minR2 != maxR2)
                    candidates.Add(new(c, maxR2));
            }

            for (int r2 = minR2 + 1; r2 < maxR2; r2++)
            {
                candidates.Add(new(minC, r2));
                if (minC != maxC)
                    candidates.Add(new(maxC, r2));
            }

            // 随机打乱顺序
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            foreach (var cell in candidates)
            {
                if (CanPlaceShapeAtCell(cell, bricks))
                {
                    result = cell;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>把所有当前空置的 cell 收集到 output (output 不清空, 用前请自行 Clear).</summary>
    public void CollectEmptyCells(ref List<Vector2Int> emptyList)
    {
        emptyList.Clear();
        for (var i = 0; i < _cellStates.Length; i++)
        {
            if (_cellStates[i])
                continue;
            emptyList.Add(i.ToCoord());
        }
    }

    /// <summary>把当前所有被占的 cell 拷贝到 output (HashSet).</summary>
    public void CollectOccupiedCells(HashSet<Vector2Int> output)
    {
        if (output == null)
            return;

        foreach (var c in _cellToBrick.Keys)
            output.Add(c);
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

        var countAll = brickPools[(type, size)].CountAll;
        brick.setName($"Brick_{size.x}x{size.y}_{countAll}");
        brick.setSize(size);
        brick.setID(id);
        brick.setOnBornCompleted(onBrickBornCompleted);

        brick.Event.addListener<OnBrickDeath>(this);
        brick.Event.addListener<OnBrickDeathTotally>(this);

        addBrickToList(brick);
        return brick;
    }

    void OnBrickBornCompleted(Brick b)
    {
        activeBricks[b.instanceID] = b;
    }

    public void onEvent(OnBrickDeath e)
    {
        activeBricks.Remove(e.brick.instanceID);
        activeBrickList.Remove(e.brick);
        // 砖块死亡: 释放它占用的所有 cell, 让后续的 spawn 可以重新落位.
        UnregisterOccupancy(e.brick);
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
        _cellToBrick.Clear();

        foreach (var (key, list) in _brickToCells)
            ListPool<Vector2Int>.Release(list);

        _brickToCells.Clear();
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