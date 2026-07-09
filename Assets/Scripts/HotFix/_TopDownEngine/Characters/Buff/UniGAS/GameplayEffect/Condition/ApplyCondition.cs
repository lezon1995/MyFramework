using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
    public abstract class ApplyCondition : SerializedScriptableObject
    {
        public abstract bool CanApply(Buff buff);
    }
}