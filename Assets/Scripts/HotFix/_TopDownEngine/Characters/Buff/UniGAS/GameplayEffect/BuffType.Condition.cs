using System;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
    public partial class BuffType : SerializedScriptableObject
    {
        public ConditionConfig condition;

        [Serializable, HideLabel]
        [BoxGroup("Condition", order: 7), HideIfGroup("Condition/Toggle", Condition = INSTANT)]
        public class ConditionConfig
        {
            public ApplyCondition[] ApplyConditions;
        }
    }
}