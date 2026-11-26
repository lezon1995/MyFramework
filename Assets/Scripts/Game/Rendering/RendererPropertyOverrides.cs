using UnityEngine;

namespace MoreMountains
{
    /// <summary>Per-renderer effects without allocating a private material. Preserves other overrides.</summary>
    public sealed class RendererPropertyOverrides
    {
        readonly Renderer renderer;
        readonly MaterialPropertyBlock block = new();

        public RendererPropertyOverrides(Renderer renderer) => this.renderer = renderer;

        public void SetFloat(int property, float value)
        {
            if (!renderer) return;
            renderer.GetPropertyBlock(block);
            block.SetFloat(property, value);
            renderer.SetPropertyBlock(block);
        }

        public void SetColor(int property, Color value)
        {
            if (!renderer) return;
            renderer.GetPropertyBlock(block);
            block.SetColor(property, value);
            renderer.SetPropertyBlock(block);
        }
    }
}
