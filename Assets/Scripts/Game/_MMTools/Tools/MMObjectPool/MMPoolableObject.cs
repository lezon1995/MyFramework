using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Add this class to an object that you expect to pool from an objectPooler. 
    /// Note that these objects can't be destroyed by calling Destroy(), they'll just be set inactive (that's the whole point).
    /// </summary>
    [AddComponentMenu("More Mountains/Tools/Object Pool/MMPoolableObject")]
    public class MMPoolableObject : MMObjectBounds
    {
        [ShowInInspector]
        public bool InUse { get; set; }
        public UnityEvent ExecuteOnEnable;
        public UnityEvent ExecuteOnDisable;

        public event Action OnSpawnComplete;

        // The life-time, in seconds, of the object.
        // If set to 0 it'll live forever.
        // if set to any positive value it'll be set inactive after that time.
        public float LifeTime;
        Timer _timer;

        public void Release()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// When the objects get enabled (usually after having been pooled from an ObjectPooler, we initiate its death countdown.
        /// </summary>
        protected virtual void OnEnable()
        {
            Size = GetBounds().extents * 2;
            ExecuteOnEnable?.Invoke();

            _timer.kill();
            if (LifeTime > 0F)
            {
                _timer = LifeTime;
            }
        }

        void FixedUpdate()
        {
            var dt = Time.fixedDeltaTime;
            if (_timer.update(dt))
            {
                Release();
            }
        }

        /// <summary>
        /// When the object gets disabled (maybe it got out of bounds), we cancel its programmed death
        /// </summary>
        protected virtual void OnDisable()
        {
            ExecuteOnDisable?.Invoke();
            CancelInvoke();
        }

        /// <summary>
        /// Triggers the on spawn complete event
        /// </summary>
        public virtual void TriggerOnSpawnComplete()
        {
            OnSpawnComplete?.Invoke();
        }
    }
}