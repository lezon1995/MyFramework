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
        SmoothTrail smoothTrail;
        TrailRenderer trailRenderer;
        SpriteRenderer spriteRenderer;
        ParticleSystem particleRenderer;

        ParticleSystem fxDead;

        void Awake()
        {
            TryGetComponent(out ball);
            var obj = ball.gameObject;
            obj.find(out renderer, "Renderer");
            obj.find(out spriteRenderer, "ball_sprite");
            obj.find(out particleRenderer, "ball_particle");
            obj.find(out smoothTrail);
            obj.find(out fxDead, "FxDead");
            obj.find(out trailRenderer, "Trail");
        }

        public void setRendererActive(bool active)
        {
            renderer.gameObject.SetActive(active);
        }

        public void setLevel(int level)
        {
            Material material = null;
            if (spriteRenderer.gameObject.activeSelf)
            {
                material = spriteRenderer.material;
            }
            else if (particleRenderer)
            {
                var renderer = particleRenderer.GetComponent<ParticleSystemRenderer>();
                material = renderer.material;
            }

            if (!material)
                return;

            if (level > 1)
            {
                var rarity = Mathf.Clamp(level - 1, 0, 3);
                var rarityColor = gameDesign.getRarityColor((ItemRarity)rarity);
                var color = rarityColor.title;
                material.SetColor(PixelOutlineColor, color);
                material.SetFloat(PixelOutlineFade, 1F);

                smoothTrail.setGradientColor(color);
            }
            else
            {
                material.SetColor(PixelOutlineColor, Color.clear);
                material.SetFloat(PixelOutlineFade, 0F);
                smoothTrail.setGradientColor(Color.clear);
            }
        }

        public void playFxDead()
        {
            fxDead?.Play();
        }

        public void clearTrail()
        {
            smoothTrail?.clearTrail();
            trailRenderer?.Clear();
        }
    }
}