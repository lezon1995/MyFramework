using UnityEngine;

namespace MarbleHero;

public class NormalBall : Ball
{
    protected override void onHitEnterBorderBot(Collider2D c, Vector2 normal)
    {
        ballManager.destroyBall(this);
    }
}