using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    /// <summary>
    /// 球实例服务
    /// </summary>
    public sealed class BallInstanceService :
        IEvent<OnBallDeath>
        , IEvent<OnBallDeathTotally>
        , IEvent<OnBallExpired>
    {
        BallManagementSystem _owner;

        public BallInstanceService(BallManagementSystem owner)
        {
            _owner = owner;
        }

        HashSet<Ball> activeBalls = new(); //发射后运动中的Ball
        HashSet<Ball> inactiveBalls = new(); //回到底板后待发射的Ball
        Dictionary<BallType, ObjectPool<Ball>> ballPools = new();
        Dictionary<BallType, Dictionary<long, Ball>> ballTypeList = new(); // 角色分类列表
        Dictionary<long, Ball> ballGUIDList = new(); // 角色ID索引表

        public Ball acquireBall(BallType ballType)
        {
            return acquireBall(ballType, Vector2.zero, Vector2.up);
        }

        public Ball acquireBall(BallType ballType, Vector2 pos)
        {
            return acquireBall(ballType, pos, Vector2.up);
        }

        public Ball acquireBall(BallType ballType, Vector2 pos, Vector2 direction)
        {
            if (!ballPools.TryGetValue(ballType, out var pool))
            {
                pool = new(
                    createFunc: () => createBall(ballType),
                    actionOnGet: ball => { },
                    actionOnRelease: ball => { },
                    actionOnDestroy: destroyBall,
                    collectionCheck: true,
                    defaultCapacity: 10,
                    maxSize: 100);

                ballPools.add(ballType, pool);
            }

            var ball = pool.Get();
            prepareToShoot(ball, pos, direction);
            return ball;
        }

        void prepareToShoot(Ball ball, Vector2 pos, Vector2 direction)
        {
            ball.setActive(true);
            ball.setEnabled(true);
            ball.setTeleportPosition(pos, BORDER_BOT_LAYER_MASK);
            ball.setShootDirection(direction);
            ball.refreshInitialHealth();
            ball.setRendererActive(true);
            ball.SetColliderEnabled(true);
            ball.refreshDuration();
            ball.onAcquire();

            ball.Event.addListener<OnBallDeath>(this);
            ball.Event.addListener<OnBallDeathTotally>(this);
            ball.Event.addListener<OnBallExpired>(this);

            activeBalls.Add(ball);
            inactiveBalls.Remove(ball);
        }

        Ball createBall() => createBall(BallType.Normal);

        T createBall<T>() where T : Ball => createBall(BallType.Normal) as T;

        Ball createBall(BallType type)
        {
            var id = generateGUID();

            if (ballGUIDList.ContainsKey(id))
            {
                logError("there is a ball id : " + id + "! can not create again!");
                return null;
            }

            string ballName = type switch
            {
                BallType.Normal => "Ball_Normal",
                BallType.LaserBeam => "Ball_Laser",
                BallType.LaserBullet => "Ball_LaserBullet",
                BallType.LightningStrike => "Ball_LightningStrike",
                BallType.ElectricityStrike => "Ball_ElectricityStrike",
                BallType.RockQuake => "Ball_RockQuake",
                BallType.FireBurning => "Ball_FireBurning",
                BallType.PoisonBurning => "Ball_PoisonBurning",
                _ => "Ball_Normal"
            };
            var path = $"{GAMEPLAY_PATH}/Balls/{ballName}.prefab";
            var o = prefabPool.createObject(path);
            o.TryGetComponent(out Ball ball);
            // 将角色挂接到管理器下
            ball.setID(id);
            if (ballManager.getDef(type, out var def))
            {
                ball.setDef(def);
            }

            ball.setPlayer(player);
            ball.setEnabled(true);
            addBallToList(ball);
            ball.setName($"{ballName}_{ballTypeList[ball.BallType].Count}");
            return ball;
        }

        public void releaseBall(Ball ball)
        {
            ball.onRelease();
            ball.setActive(false);
            ball.setEnabled(false);

            activeBalls.Remove(ball);
            inactiveBalls.Add(ball);

            if (ballPools.TryGetValue(ball.BallType, out var pool))
            {
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
            ballTypeList.get(ball.BallType)?.Remove(guid);
            // 从ID索引表中移除
            ballGUIDList.Remove(guid);

            ball.Event.removeListener<OnBallDeath>(this);
            ball.Event.removeListener<OnBallDeathTotally>(this);
            ball.Event.removeListener<OnBallExpired>(this);
        }

        public void destroyBallList<T>(List<T> characterList) where T : Ball
        {
            foreach (T ball in characterList.safe())
            {
                long guid = ball.getGUID();
                // 从角色分类列表中移除
                ballTypeList.get(ball.BallType)?.Remove(guid);
                // 从ID索引表中移除
                ballGUIDList.Remove(guid);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------
        void addBallToList(Ball ball)
        {
            if (ball == null)
                return;

            long guid = ball.getGUID();
            // 加入到角色分类列表
            ballTypeList.getOrAddNew(ball.BallType).Add(guid, ball);
            // 加入ID索引表
            if (!ballGUIDList.TryAdd(guid, ball))
            {
                logError("there is a ball id : " + guid + ", can not add again!");
            }
        }

        public HashSet<Ball> getActiveBalls()
        {
            return activeBalls;
        }

        public bool anyActiveBall()
        {
            return activeBalls.Count > 0;
        }

        public void onEvent(OnBallDeath e)
        {
            activeBalls.Remove(e.ball);
        }

        public void onEvent(OnBallDeathTotally e)
        {
            releaseBall(e.ball);
        }

        public void onEvent(OnBallExpired e)
        {
            releaseBall(e.ball);
        }
    }
}