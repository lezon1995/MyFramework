using UnityEngine;

namespace MarbleHero;

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

// public struct HealTextEvent
// {
//     public Heal Heal;
//     public Transform Target;
//
//     public HealTextEvent(Heal heal, Transform target)
//     {
//         Heal = heal;
//         Target = target;
//     }
// }

public partial class Brick : IEventRouter
{
    public IEventRouter eventRouter => this;

    public virtual void onHitEnter(Ball ball, Vector2 normal)
    {
    }
}