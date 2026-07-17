using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains
{
    public class Obstacle : MonoBehaviour, IResetProperty
        , IHittable
        , IEventRouter
    {
        public IEventRouter Event => this;

        public void resetProperty()
        {
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            onTriggerEnter(other);
        }

        void OnTriggerExit2D(Collider2D other)
        {
            onTriggerExit(other);
        }

        protected void onTriggerEnter(Collider2D c)
        {
            if (c.TryGetComponent(out Ball ball))
                onBallEnter(ball);
        }

        protected void onTriggerExit(Collider2D c)
        {
            if (c.TryGetComponent(out Ball ball))
                onBallExit(ball);
        }

        protected virtual void onBallEnter(Ball ball)
        {
        }

        protected virtual void onBallExit(Ball ball)
        {
        }
    }
}