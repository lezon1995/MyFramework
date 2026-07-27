using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Pool;

namespace MoreMountains
{
    // 角色管理器
    public class BallManager : MainManagerBehaviour
        , IEvent<OnBallDeath>
        , IEvent<OnBallDeathTotally>
    {
        //key: gameObject.GetInstanceID()
        public HashSet<Ball> activeBalls = new(); //发射后运动中的Ball
        protected HashSet<Ball> inactiveBalls = new(); //回到底板后待发射的Ball
        protected Dictionary<BallType, Dictionary<long, Ball>> ballTypeList = new(); // 角色分类列表
        protected Dictionary<long, Ball> ballGUIDList = new(); // 角色ID索引表
        protected Dictionary<BallType, ObjectPool<Ball>> ballPools = new();

        protected override void OnDestroy()
        {
            destroyAllBall();
            ballTypeList = null;
            ballGUIDList = null;
            base.OnDestroy();
        }

        public Ball getBall(long id)
        {
            return ballGUIDList.get(id);
        }

        public Dictionary<long, Ball> getBallList()
        {
            return ballGUIDList;
        }

        public Dictionary<long, Ball> getBallListByType(BallType type)
        {
            return ballTypeList.get(type);
        }
    
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
                    actionOnGet: ball =>
                    {
                        ball.setActive(true);
                        ball.setEnabled(true);

                        activeBalls.Add(ball);
                        inactiveBalls.Remove(ball);
                    },
                    actionOnRelease: ball =>
                    {
                        ball.setActive(false);
                        ball.setEnabled(false);

                        activeBalls.Remove(ball);
                        inactiveBalls.Add(ball);
                    },
                    actionOnDestroy: destroyBall,
                    collectionCheck: true,
                    defaultCapacity: 10,
                    maxSize: 100);

                ballPools.add(ballType, pool);
            }

            var ball = pool.Get();
            ball.setTeleportPosition(pos, BORDER_BOT_LAYER_MASK);
            ball.setShootDirection(direction);
            ball.setInitialHealth(int.MaxValue);
            ball.setRendererActive(true);
            ball.SetColliderEnabled(true);
            ball.onAcquire();
        
            ball.Event.addListener<OnBallDeath>(this);
            ball.Event.addListener<OnBallDeathTotally>(this);
            return ball;
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
            ball.setPlayer(player);
            ball.setEnabled(true);
            addBallToList(ball);
            ball.setName($"{ballName}_{ballTypeList[ball.BallType].Count}");
            return ball;
        }

        public void releaseBall(Ball ball)
        {
            if (ballPools.TryGetValue(ball.BallType, out var pool))
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
            ballTypeList.get(ball.BallType)?.Remove(guid);
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
                ballTypeList.get(ball.BallType)?.Remove(guid);
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
            ballTypeList.getOrAddNew(ball.BallType).Add(guid, ball);
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
            activeBalls.Remove(e.ball);
        }

        public void onEvent(OnBallDeathTotally e)
        {
            releaseBall(e.ball);
        }
    }
}