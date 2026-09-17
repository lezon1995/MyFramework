using UnityEngine;

namespace MoreMountains
{
    public sealed partial class BallInventorySlot
    {
        public bool ReadyToShoot { get; set; } = true;
        public Ball BallInstance { get; set; }
        
        public bool TryShoot(APlayer p, Vector3 pos, out Ball ballInstance)
        {
            if (IsEmpty)
            {
                ballInstance = null;
                return false;
            }

            ballInstance = p.BallManagement.Instance.acquireBall(Item.Type, pos, Item.Level, int.MaxValue);
            var valid = ballInstance != null;
            if (valid)
            {
                ReadyToShoot = false;
                BallInstance = ballInstance;
                return true;
            }

            return false;
        }

        public bool TryReload(APlayer p, Ball ballInstance)
        {
            if (ballInstance == null)
                return false;

            if (IsEmpty)
                return false;

            p.BallManagement.Instance.releaseBall(ballInstance);

            ReadyToShoot = true;
            BallInstance = null;
            return true;
        }
    }
}