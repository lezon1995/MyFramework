using UnityEngine;

namespace MoreMountains
{
    [RequireComponent(typeof(Ball))]
    public class BallRenderer : MonoBehaviour
    {
        static readonly int PixelOutlineFade = Shader.PropertyToID("_PixelOutlineFade");
        static readonly int PixelOutlineColor = Shader.PropertyToID("_PixelOutlineColor");

        // TrailRenderer trailRenderer;
        Ball ball;
        GameObject renderer;
        SmoothTrail trailRenderer;
        SpriteRenderer spriteRenderer;

        ParticleSystem fxDead;

        void Awake()
        {
            TryGetComponent(out ball);
            var obj = ball.gameObject;
            obj.find(out renderer, "Renderer");
            obj.find(out spriteRenderer, "ball_sprite");
            obj.find(out trailRenderer);
            obj.find(out fxDead, "FxDead");
        }

        public void setRendererActive(bool active)
        {
            renderer.gameObject.SetActive(active);
        }

        public void setLevel(int level)
        {
            var material = spriteRenderer.material;
            if (level > 1)
            {
                var rarity = Mathf.Clamp(level - 1, 0, 3);
                var rarityColor = gameDesign.getRarityColor((ItemRarity)rarity);
                var color = rarityColor.title;
                material.SetColor(PixelOutlineColor, color);
                material.SetFloat(PixelOutlineFade, 1F);
                
                trailRenderer.setColor(color);
            }
            else
            {
                material.SetColor(PixelOutlineColor, Color.clear);
                material.SetFloat(PixelOutlineFade, 0F);
                
                trailRenderer.setColor(Color.clear);
            }
        }

        public void playFxDead()
        {
            fxDead?.Play();
        }

        public void clearTrail()
        {
            trailRenderer.clearTrail();
        }
    }
}