using MoreMountains.Tools;

namespace MoreMountains
{
    public class Obstacle : MainActorBehaviour, IResetProperty
        , IHittable
        , IEventRouter
    {
        public IEventRouter Event => this;

        public void resetProperty()
        {
        }
    }
}