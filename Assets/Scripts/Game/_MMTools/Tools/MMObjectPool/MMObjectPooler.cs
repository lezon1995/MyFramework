using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A base class, meant to be extended depending on the use (simple, multiple object pooler), and used as an interface by the spawners.
    /// Still handles common stuff like singleton and initialization on start().
    /// DO NOT add this class to a prefab, nothing would happen. Instead, add SimpleObjectPooler or MultipleObjectPooler.
    /// </summary>
    public abstract class MMObjectPooler : MonoBehaviour
    {
        /// singleton pattern
        public static MMObjectPooler Instance;

        /// if this is true, the pool will try not to create a new waiting pool if it finds one with the same name.
        public bool MutualizeWaitingPools;

        /// if this is true, all waiting and active objects will be regrouped under an empty game object. Otherwise they'll just be at top level in the hierarchy
        public bool NestWaitingPool = true;

        /// if this is true, the waiting pool will be nested under this object
        [MMCondition(nameof(NestWaitingPool), true)]
        public bool NestUnderThis;

        protected MMObjectPool _objectPool { get; private set; }

        protected bool _onSceneLoadedRegistered;

        public static List<MMObjectPool> _pools { get; } = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        protected static void InitializeStatics()
        {
            Instance = null;
        }

        /// <summary>
        /// Adds a pooler to the static list if needed
        /// </summary>
        /// <param name="pool"></param>
        public static void AddPool(MMObjectPool pool)
        {
            if (!_pools.Contains(pool))
            {
                _pools.Add(pool);
            }
        }

        /// <summary>
        /// Removes a pooler from the static list
        /// </summary>
        /// <param name="pool"></param>
        public static void RemovePool(MMObjectPool pool)
        {
            _pools.Remove(pool);
        }

        /// <summary>
        /// On awake we fill our object pool
        /// </summary>
        protected virtual void Awake()
        {
            Instance = this;
            FillObjectPool();
        }

        /// <summary>
        /// Creates the waiting pool or tries to reuse one if there's already one available
        /// </summary>
        protected bool CreateWaitingPool()
        {
            var poolName = DetermineObjectPoolName();

            if (!MutualizeWaitingPools)
            {
                // we create a container that will hold all the instances we create
                NewPool();
                return true;
            }

            MMObjectPool objectPool = ExistingPool(poolName);
            if (objectPool)
            {
                _objectPool = objectPool;
                return false;
            }

            NewPool();

            AddPool(_objectPool);
            return true;

            void NewPool()
            {
                var pool = new GameObject(poolName);
                SceneManager.MoveGameObjectToScene(pool, gameObject.scene);
                _objectPool = pool.AddComponent<MMObjectPool>();
                _objectPool.PooledGameObjects = new List<GameObject>();
                ApplyNesting();
            }
        }

        /// <summary>
        /// Looks for an existing pooler for the same object, returns it if found, returns null otherwise
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public virtual MMObjectPool ExistingPool(string poolName)
        {
            if (_pools.Count == 0)
            {
                var pools = FindObjectsOfType<MMObjectPool>();
                if (pools.Length > 0)
                {
                    _pools.AddRange(pools);
                }
            }

            foreach (MMObjectPool pool in _pools)
            {
                if (pool && pool.name == poolName /* && (pool.gameObject.scene == this.gameObject.scene)*/)
                {
                    return pool;
                }
            }

            return null;
        }

        /// <summary>
        /// If needed, nests the waiting pool under this object
        /// </summary>
        protected virtual void ApplyNesting()
        {
            if (NestWaitingPool && NestUnderThis && _objectPool)
            {
                _objectPool.transform.SetParent(transform);
            }
        }

        /// <summary>
        /// Determines the name of the object pool.
        /// </summary>
        /// <returns>The object pool name.</returns>
        protected virtual string DetermineObjectPoolName()
        {
            return "[Pooler] " + name;
        }

        /// <summary>
        /// Implement this method to fill the pool with objects
        /// </summary>
        public virtual void FillObjectPool()
        {
        }

        /// <summary>
        /// Implement this method to return a game object
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public abstract GameObject GetPooledGameObject();

        /// <summary>
        /// Destroys the object pool
        /// </summary>
        public virtual void DestroyObjectPool()
        {
            if (_objectPool)
            {
                Destroy(_objectPool.gameObject);
            }
        }

        /// <summary>
        /// On enable we register to the scene loaded hook
        /// </summary>
        protected virtual void OnEnable()
        {
            if (!_onSceneLoadedRegistered)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        /// <summary>
        /// OnSceneLoaded we recreate 
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="loadSceneMode"></param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            if (this == null)
                return;

            if (_objectPool == null)
            {
                if (this)
                {
                    FillObjectPool();
                }
            }
        }

        /// <summary>
        /// On Destroy we remove ourselves from the list of poolers 
        /// </summary>
        private void OnDestroy()
        {
            if (_objectPool && NestUnderThis)
            {
                RemovePool(_objectPool);
            }

            if (_onSceneLoadedRegistered)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                _onSceneLoadedRegistered = false;
            }
        }
    }
}