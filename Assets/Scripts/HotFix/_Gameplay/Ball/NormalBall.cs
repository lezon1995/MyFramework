using UnityEngine;

namespace MarbleHero;

public class NormalBall : Ball
{
    protected override bool onHitEnter(BorderBot border, Vector2 normal)
    {
        base.onHitEnter(border, normal);
        // ballManager.destroyBall(this);
        return true;
    }
    
}