using UnityEngine;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Actions are behaviours and describe what your character is doing. Examples include patrolling, shooting, jumping, etc. 
    /// </summary>
    public abstract class AIAction : MonoBehaviour
    {
        public enum InitializationModes
        {
            EveryTime,
            OnlyOnce,
        }

        public InitializationModes InitializationMode;
        protected bool _initialized;

        [Tooltip("a label you can set to organize your AI Actions, not used by anything else")]
        public string Label;

        public virtual bool ActionInProgress { get; set; }
        protected AIBrain _brain;

        protected virtual bool ShouldInitialize => InitializationMode switch
        {
            InitializationModes.EveryTime => true,
            InitializationModes.OnlyOnce => _initialized == false,
            _ => true
        };

        protected virtual void Awake() => _brain = GetComponentInParent<AIBrain>();
        public virtual void Initialization() => _initialized = true;
        public abstract void PerformAction();
        public virtual void OnEnterState() => ActionInProgress = true;
        public virtual void OnExitState() => ActionInProgress = false;
    }
}