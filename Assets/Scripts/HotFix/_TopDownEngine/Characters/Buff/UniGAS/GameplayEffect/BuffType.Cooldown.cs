using System;
using Sirenix.OdinInspector;

namespace MoreMountains
{
    public partial class Buff
    {
        public CooldownConfig cooldown;

        [Serializable, HideLabel]
        [BoxGroup("Cooldown", order: 8), ShowIfGroup("Cooldown/Toggle", Condition = COOLDOWN)]
        public class CooldownConfig
        {
            [BoxGroup("Duration", false), InlineProperty]
            public Mag Duration;

            public Actors CooldownAt;
        }
    }
}