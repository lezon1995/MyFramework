using UnityEngine;

namespace MoreMountains
{
    public partial class Brick
    {
        public virtual void onHitEnter(Ball ball, Vector2 normal)
        {
        }

        public override void onEvent(DoDashDodge e)
        {
            brickRenderer.playFxDodge();
        }

        public override void onEvent(DoChanceDodge e)
        {
            brickRenderer.playFxDodge();
        }
    }
}