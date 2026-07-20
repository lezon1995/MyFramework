using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class EnemyBrain : AIBrain
    {
        public Brick character;
        
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
            owner.TryGetComponent(out character);
        }
    }
}