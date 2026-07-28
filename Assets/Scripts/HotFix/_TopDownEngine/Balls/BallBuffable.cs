using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallBuffable : Buffable
    {
        Ball ball;
        public override Character Character => ball.Player;
        public override IEventRouter Event => ball.Player.Event;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out ball);
        }
    }
}