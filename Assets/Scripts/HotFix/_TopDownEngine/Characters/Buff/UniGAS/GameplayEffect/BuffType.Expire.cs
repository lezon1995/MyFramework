using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    public partial class Buff
    {
        public ExpireConfig expire;

        [Serializable, HideLabel]
        [BoxGroup("Expire", order: 6), HideIfGroup("Expire/Toggle", Condition = INSTANT)]
        public class ExpireConfig
        {
            public Data[] ExpiredBuffs;
            public Data[] PreExpiredBuffs;
        }
    }
}