using System;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    // 角色管理器
    public class BrickManager : MainManagerBehaviour
        , IEvent<OnBrickDeath>
        , IEvent<OnBrickDeathTotally>
    {
        [SerializeField]
        List<BrickDef> Defs = new();

        Dictionary<Vector2Int, List<BrickDef>> defDict;
        Dictionary<BrickType, BrickDef> brickDefDict = new();

        //key: gameObject.GetInstanceID()
        protected Dictionary<int, Brick> activeBricks = new();
        protected List<Brick> activeBrickList = new();
        protected Dictionary<Type, Dictionary<long, Brick>> brickTypeList = new(); // 角色分类列表
        protected Dictionary<long, Brick> brickGUIDList = new(); // 角色ID索引表

        protected Dictionary<BrickDef, ObjectPool<Brick>> brickPools = new();

        public BrickGridLayout brickLayout;
        Action<Brick> onBrickBornCompleted;
        public SafeDictionary<Brick, (Ball, MTimer)> brickDamageTimers = new();

        static Comparison<Brick> nearestComparison = Comparison;

        public BrickManager()
        {
            onBrickBornCompleted = OnBrickBornCompleted;
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            defDict = Defs
                .GroupBy(def => def.Size)
                .ToDictionary(g => g.Key, g => g.ToList());

            brickDefDict.Clear();
            foreach (var def in Defs)
                brickDefDict[def.BrickType] = def;
        }

        public override void OnFixedUpdate(float dt)
        {
            base.OnFixedUpdate(dt);

            if (brickDamageTimers.count() > 0)
            {
                foreach (var (brick, (ball, timer)) in brickDamageTimers)
                {
                    if (timer.update(dt))
                    {
                        brickDamageTimers.remove(brick);
                        if (ball is IEvent<OnBrickDeath> e)
                        {
                            brick.Event.removeListener(e);
                        }

                        timer.release();
                    }
                }
            }
        }

        public BrickDef GetRandomDef(Vector2Int size)
        {
            if (defDict.TryGetValue(size, out var list))
            {
                return list.random();
            }

            return null;
        }

        public BrickDef GetDef(BrickType t)
        {
            if (brickDefDict.TryGetValue(t, out var def))
                return def;

            return null;
        }

        protected override void OnDestroy()
        {
            destroyAllBrick();
            brickTypeList = null;
            brickGUIDList = null;
            base.OnDestroy();
        }

        public void load(float cellSize, int col, int row)
        {
            brickLayout = BrickGridLayout.New(cellSize, col, row);
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

        static Vector3 playerPos;

        public bool getNearestActiveBrick(Vector3 pos, out Brick nearestBrick, List<Brick> excepts = null)
        {
            playerPos = pos;
            using var _ = new ListScope<Brick>(out var list);
            list.addRange(activeBricks.Values);
            if (excepts.count() > 0)
            {
                foreach (var except in excepts)
                    list.Remove(except);
            }
            
            list.Sort(nearestComparison);
            nearestBrick = list.get(0);
            return nearestBrick != null;
        }

        static int Comparison(Brick b1, Brick b2)
        {
            var d1 = (b1.getWorldPosition() - playerPos).sqrMagnitude;
            var d2 = (b2.getWorldPosition() - playerPos).sqrMagnitude;
            return d1.CompareTo(d2);
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

        /// <summary>
        /// 从形状库生成一组砖块（用于 ShapeEntry 的批量生成）。
        /// </summary>
        /// <param name="shapeOrigin">形状锚点在世界坐标系中的位置（即形状放置的原点）</param>
        /// <param name="bricks">形状中每个砖块的局部坐标 (col/row 为起点的局部坐标, width/height 为尺寸)</param>
        /// <param name="curWave"></param>
        /// <param name="result"></param>
        /// <param name="pivot">形状锚点位置（影响局部坐标如何映射到世界坐标）</param>
        /// <returns>生成的砖块列表</returns>
        public bool acquireShape(Vector2Int shapeOrigin, List<GridUnitBrick> bricks, WaveConfig wave, ref List<BrickTemplate> result)
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
                BrickDef def = null;
                bool found = false;
                for (var i = 0; i < wave.availableMonsters.Count; i++)
                {
                    if (wave.availableMonsters[i].monsterDef.Size == b.Size)
                    {
                        def = wave.availableMonsters[i].monsterDef;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    def = GetRandomDef(b.Size);
                }

                result.Add(new(brickPivotPos, def));
            }

            return true;
        }

        public Brick acquireBrick(BrickDef def, Vector2 pos)
        {
            if (!brickPools.TryGetValue(def, out var pool))
            {
                pool = new(
                    createFunc: () => createBrick(def),
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

                brickPools.add(def, pool);
            }

            var brick = pool.Get();
            var sortingOrder = brickLayout.getSortingOrderAtPosY(pos.y);
            brick.setSortingOrder(sortingOrder);
            brick.setWorldPosition(pos);
            brick.onAcquire();
            brick.RespawnAt(pos);

            activeBrickList.add(brick);
            return brick;
        }

        static List<Collider2D> collider2Ds = new();

        public bool IsCellEmpty(Vector2 cellPos)
        {
            var filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(BRICK_LAYER_MASK);
            var count = Physics2D.OverlapBox(cellPos, Vector2.one * BrickUnitSize, 0, filter, collider2Ds);
            return count == 0;
        }

        Brick createBrick(BrickDef def)
        {
            var id = generateGUID();

            if (brickGUIDList.ContainsKey(id))
            {
                logError("there is a brick id : " + id + "! can not create again!");
                return null;
            }

            var size = def.Size;
            string path = def.Type switch
            {
                SpawnEnemyType.Boss => $"{GAMEPLAY_PATH}/Bricks/{def.PrefabName}.prefab",
                SpawnEnemyType.Obstacle => $"{GAMEPLAY_PATH}/Bricks/{def.PrefabName}.prefab",
                _ => $"{GAMEPLAY_PATH}/Bricks/Brick_{size.x}x{size.y}.prefab"
            };

            var o = prefabPool.createObject(path);
            o.TryGetComponent(out Brick brick);
            var countAll = brickPools[def].CountAll;
            brick.setBrickDef(def);
            brick.setName($"Brick_{def.name}_{countAll}");
            brick.setSize(size);
            brick.setID(id);
            brick.setOnBornCompleted(onBrickBornCompleted);
            brick.setPlayerAsTarget(player);
            brick.setupWeapon(def.AIHandleWeapon);
            brick.setupBrain(def.AIBehaviour);

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
        }

        public void onEvent(OnBrickDeathTotally e)
        {
            releaseBrick(e.brick);
        }

        public void releaseBrick(Brick brick)
        {
            if (brickPools.TryGetValue(brick.getDef(), out var pool))
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
}