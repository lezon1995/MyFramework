using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public abstract class EnemyDecision : AIDecision
    {
        protected Brick brick;
        protected EnemyBrain _enemyBrain;
        protected override void Awake()
        {
            base.Awake();
            _enemyBrain = _brain as EnemyBrain;
        }

        public override void Initialization()
        {
            base.Initialization();
            if (brick == null)
                this.TryGetComponentInParent(out brick);
        }
        
        protected Vector3 getTargetPosition()
        {
            var p = _enemyBrain.TargetPlayer;
            var targetPos = p.inStealth ? _enemyBrain.LastKnownTargetPosition : p.getWorldPosition();
            return targetPos;
        }
    }
}