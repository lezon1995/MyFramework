using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A simple object pool outputting a single type of objects
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMSimpleObjectPooler")]
    public class MMSimpleObjectPooler : MMObjectPooler
    {
        /// the game object we'll instantiate 
        public GameObject GameObjectToPool;

        /// the number of objects we'll add to the pool
        public int PoolSize = 20;

        /// if true, the pool will automatically add objects to the itself if needed
        public bool PoolCanExpand = true;

        public virtual List<MMSimpleObjectPooler> Owner { get; set; }

        private void OnDestroy()
        {
            Owner?.Remove(this);
        }

        /// <summary>
        /// Fills the object pool with the gameobject type you've specified in the inspector
        /// </summary>
        public override void FillObjectPool()
        {
            if (GameObjectToPool == null)
                return;

            // if we've already created a pool, we exit
            if (_objectPool && _objectPool.PooledGameObjects.Count > PoolSize)
                return;

            CreateWaitingPool();

            int objectsToSpawn = PoolSize;

            if (_objectPool)
            {
                objectsToSpawn -= _objectPool.PooledGameObjects.Count;
            }

            // we add to the pool the specified number of objects
            for (int i = 0; i < objectsToSpawn; i++)
            {
                AddOneObjectToThePool();
            }
        }

        /// <summary>
        /// Determines the name of the object pool.
        /// </summary>
        /// <returns>The object pool name.</returns>
        protected override string DetermineObjectPoolName() => "[Pooler] " + GameObjectToPool.name;

        /// <summary>
        /// This method returns one inactive object from the pool
        /// </summary>
        /// <returns>The pooled game object.</returns>
        public override GameObject GetPooledGameObject()
        {
            // we go through the pool looking for an inactive object
            var list = _objectPool.PooledGameObjects;
            for (int i = 0; i < list.Count; i++)
            {
                var o = list[i];
                if (!o.gameObject.activeInHierarchy)
                    return o; // if we find one, we return it
            }

            // if we haven't found an inactive object (the pool is empty), and if we can extend it, we add one new object to the pool, and return it		
            if (PoolCanExpand)
                return AddOneObjectToThePool();

            // if the pool is empty and can't grow, we return nothing.
            return null;
        }

        /// <summary>
        /// Adds one object of the specified type (in the inspector) to the pool.
        /// </summary>
        /// <returns>The one object to the pool.</returns>
        protected virtual GameObject AddOneObjectToThePool()
        {
            if (GameObjectToPool == null)
            {
                Debug.LogWarning("The " + gameObject.name + " ObjectPooler doesn't have any GameObjectToPool defined.", gameObject);
                return null;
            }

            bool initialStatus = GameObjectToPool.activeSelf;
            GameObjectToPool.SetActive(false);
            GameObject newGameObject = Instantiate(GameObjectToPool);
            GameObjectToPool.SetActive(initialStatus);
            SceneManager.MoveGameObjectToScene(newGameObject, gameObject.scene);
            if (NestWaitingPool)
            {
                newGameObject.transform.SetParent(_objectPool.transform);
            }

            newGameObject.name = GameObjectToPool.name + "-" + _objectPool.PooledGameObjects.Count;

            _objectPool.PooledGameObjects.Add(newGameObject);

            return newGameObject;
        }
    }
}