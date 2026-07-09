using System;
using Sirenix.OdinInspector;

namespace MoreMountains.TopDownEngine
{
    public partial class BuffType : SerializedScriptableObject
    {
        public ExpireConfig expire;

        [Serializable, HideLabel]
        [BoxGroup("Expire", order: 6), HideIfGroup("Expire/Toggle", Condition = INSTANT)]
        public class ExpireConfig
        {
            public Buff.Data[] ExpiredBuffs;
            public Buff.Data[] PreExpiredBuffs;
        }
    }
}