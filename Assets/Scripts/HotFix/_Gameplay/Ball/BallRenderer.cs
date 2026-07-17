using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallRenderer : MonoBehaviour
    {
        // TrailRenderer trailRenderer;
        Ball ball;
        GameObject renderer;
        SmoothTrail trailRenderer;
    
        ParticleSystem fxDead;

        void Awake()
        {
            TryGetComponent(out ball);
            var obj = ball.gameObject;
            obj.find(out renderer, "Renderer");
            obj.find(out trailRenderer);
            obj.find(out fxDead, "FxDead");
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
}