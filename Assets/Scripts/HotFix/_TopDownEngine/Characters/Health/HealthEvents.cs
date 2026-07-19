using UnityEngine;

namespace MoreMountains
{
    public struct DmgTextEvent
    {
        public Dmg Dmg;
        public Transform Target;

        public DmgTextEvent(Dmg dmg, Transform target)
        {
            Dmg = dmg;
            Target = target;
        }
    }

    public struct HealTextEvent
    {
        public Heal Heal;
        public Transform Target;

        public HealTextEvent(Heal heal, Transform target)
        {
            Heal = heal;
            Target = target;
        }
    }

    public struct GainCoinTextEvent
    {
        public int Value;
        public Transform Target;

        public GainCoinTextEvent(int v, Transform target)
        {
            Value = v;
            Target = target;
        }
    }
}