using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallBuffable : Buffable
    {
        Ball ball;
        public override Character Character => ball.character;
        public override IEventRouter Event => ball.character.Event;

        protected override void Awake()
        {
            base.Awake();
            TryGetComponent(out ball);
        }
    }
}