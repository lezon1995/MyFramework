using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace MyGame.Performance
{
    /// <summary>
    /// 通用对象池封装（基于 UnityEngine.Pool）。用于子弹、特效、UI 项等高频实例化对象。
    /// </summary>
    /// <typeparam name="T">池化对象类型</typeparam>
    public class GameObjectPool<T> where T : Component
    {
        private readonly ObjectPool<T> _pool;
        private readonly Transform _root;

        public GameObjectPool(Transform root, int defaultCapacity = 16, int maxSize = 128, bool collectionCheck = true)
        {
            _root = root;
            _pool = new ObjectPool<T>(
                createFunc: OnCreate,
                actionOnGet: OnGet,
                actionOnRelease: OnRelease,
                actionOnDestroy: OnDestroy,
                collectionCheck: collectionCheck,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        public int CountAll => _pool.CountAll;
        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T item)
        {
            if (item == null)
            {
                return;
            }

            _pool.Release(item);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        private T OnCreate()
        {
            // 预制体由外部注入，此处返回占位实现
            return default;
        }

        private void OnGet(T item)
        {
            if (item == null)
            {
                return;
            }

            item.gameObject.SetActive(true);
            item.transform.SetParent(_root, false);
        }

        private void OnRelease(T item)
        {
            if (item == null)
            {
                return;
            }

            item.gameObject.SetActive(false);
            item.transform.SetParent(_root, true);
        }

        private void OnDestroy(T item)
        {
            if (item != null)
            {
                Object.Destroy(item.gameObject);
            }
        }
    }

    /// <summary>
    /// 预制体驱动的对象池示例（推荐用法）。
    /// </summary>
    public class PrefabPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private int defaultCapacity = 16;
        [SerializeField] private int maxSize = 128;
        [SerializeField] private Transform poolRoot;

        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            if (poolRoot == null)
            {
                var rootGo = new GameObject("PoolRoot");
                rootGo.transform.SetParent(transform, false);
                poolRoot = rootGo.transform;
            }

            _pool = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab, poolRoot),
                actionOnGet: go =>
                {
                    go.SetActive(true);
                    go.transform.SetParent(null);
                },
                actionOnRelease: go =>
                {
                    go.SetActive(false);
                    go.transform.SetParent(poolRoot, true);
                },
                actionOnDestroy: go => Destroy(go.gameObject),
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            var go = _pool.Get();
            go.transform.SetPositionAndRotation(position, rotation);
            return go;
        }

        public void Release(GameObject go)
        {
            if (go != null)
            {
                _pool.Release(go);
            }
        }
    }
}
