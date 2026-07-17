using Sirenix.OdinInspector;

namespace MoreMountains
{
    public abstract class ApplyCondition : SerializedScriptableObject
    {
        public abstract bool CanApply(Buff buff);
    }
}