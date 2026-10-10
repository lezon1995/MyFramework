using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class EnemyBrain : AIBrain
    {
        public Brick brick;
        public APlayer TargetPlayer;

        protected override void Start()
        {
            if (ResetBrainOnStart)
            {
                ResetBrain();
            }
        }

        public override void SetOwner(GameObject owner)
        {
            Owner = owner;
            owner.TryGetComponent(out brick);
        }

        public override void SetTarget(Transform target)
        {
            base.SetTarget(target);
            if (target)
            {
                target.TryGetComponent(out TargetPlayer);
            }
            else
            {
                TargetPlayer = null;
            }
        }

        protected override void StoreLastKnownPosition()
        {
            if (NoTarget)
                return;

            if (TargetPlayer.inStealth)
                return;

            LastKnownTargetPosition = Target.transform.position;
        }
    }
}