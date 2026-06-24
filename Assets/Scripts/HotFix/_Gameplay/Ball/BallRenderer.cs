using UnityEngine;

namespace MarbleHero;

public class BallRenderer : GameComponent
{
    // TrailRenderer trailRenderer;
    GameObject renderer;
    SmoothTrail trailRenderer;
    
    ParticleSystem fxDead;
    
    public override void init(ComponentOwner owner)
    {
        base.init(owner);
        if (owner is Ball ball)
        {
            var obj = ball.gameObject;
            obj.find(out renderer, "Renderer");
            obj.find(out trailRenderer);
            obj.find(out fxDead, "FxDead");
        }
    }
    
    public override void resetProperty()
    {
        base.resetProperty();
        renderer = null;
        trailRenderer = null;
        fxDead = null;
    }
    
    public void setRendererActive(bool active)
    {
        renderer.gameObject.SetActive(active);
    }

    public void playFxDead()
    {
        fxDead.Play();
    }

    public void clearTrail()
    {
        trailRenderer.clearTrail();
    }

    public void setRadius(float diameter)
    {
        renderer.transform.localScale = new(diameter, diameter, 1);
    }
}