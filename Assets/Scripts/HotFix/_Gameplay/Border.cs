using MoreMountains.Tools;

namespace MoreMountains
{
    public class Border : MainActorBehaviour, IResetProperty
        , IHittable
        , IEventRouter
    {
        public IEventRouter Event => this;

        public void resetProperty()
        {
        }
    }

    public class HBorder : Border
    {
        public void setWidth(float width)
        {
        }
    }

    public class VBorder : Border
    {
        public void setHeight(float height)
        {
        }
    }
}