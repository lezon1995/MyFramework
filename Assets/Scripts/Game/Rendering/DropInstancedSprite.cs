using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    /// <summary>Registers a drop's existing sprite with the shared mesh-particle GPU renderer.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public sealed class DropInstancedSprite : MonoBehaviour
    {
        [Tooltip("Original material whose appearance the instanced template reproduces.")]
        public Material sourceMaterial;
        public Material instancedMaterial;
        internal SpriteRenderer source;
        internal DropInstanceRenderer.Batch batch;
        internal int slot;
        bool originalForceOff;
        SortingGroup sortingGroup;
        MaterialPropertyBlock properties;
        static readonly int MainTex = Shader.PropertyToID("_MainTex");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int RendererColor = Shader.PropertyToID("_RendererColor");
        static readonly int Flip = Shader.PropertyToID("_Flip");
        static readonly int ExternalAlpha = Shader.PropertyToID("_EnableExternalAlpha");
        static readonly int[] CoinProperties =
        {
            Shader.PropertyToID("_ColumnsX"), Shader.PropertyToID("_RowsY"),
            Shader.PropertyToID("_AnimationSpeed"), Shader.PropertyToID("_Cutoff")
        };

        void OnEnable()
        {
            source = GetComponent<SpriteRenderer>();
            originalForceOff = source.forceRenderingOff;
            sortingGroup = GetComponentInParent<SortingGroup>();
            DropInstanceRenderer.Register(this);
        }

        void OnDisable()
        {
            DropInstanceRenderer.Unregister(this);
            Restore();
        }

        void OnTransformParentChanged() => sortingGroup = GetComponentInParent<SortingGroup>();

        internal void Restore()
        {
            if (source) source.forceRenderingOff = originalForceOff;
        }

        bool HasUnsupportedOverrides()
        {
            if (!source.HasPropertyBlock()) return false;
            properties ??= new MaterialPropertyBlock();
            source.GetPropertyBlock(properties);
            // SpriteRenderer retains its internal texture/color/flip block after SetPropertyBlock(null).
            // Only overrides actually read by these two known source shaders affect their appearance.
            if (properties.HasTexture(MainTex) && properties.GetTexture(MainTex) != source.sprite.texture) return true;
            if (sourceMaterial.shader.name == "Game/FlipBook (Cutout)")
            {
                foreach (int id in CoinProperties)
                    if (properties.HasFloat(id) && properties.GetFloat(id) != sourceMaterial.GetFloat(id)) return true;
                return false;
            }
            if (sourceMaterial.shader.name != "Sprites/Default") return true;
            if (properties.HasColor(ColorId) && properties.GetColor(ColorId) != sourceMaterial.GetColor(ColorId)) return true;
            if (properties.HasColor(RendererColor) && properties.GetColor(RendererColor) != source.color) return true;
            if (properties.HasVector(Flip))
            {
                var flip = properties.GetVector(Flip);
                if (flip.x != (source.flipX ? -1 : 1) || flip.y != (source.flipY ? -1 : 1)) return true;
            }
            return properties.HasFloat(ExternalAlpha) && properties.GetFloat(ExternalAlpha) != 0;
        }

        internal bool TryGetParticle(out ParticleSystem.Particle particle)
        {
            particle = default;
            if (!source || !isActiveAndEnabled || !source.enabled || originalForceOff || !source.sprite ||
                !SystemInfo.supportsInstancing || !sourceMaterial || !instancedMaterial || !instancedMaterial.shader.isSupported ||
                source.sharedMaterial != sourceMaterial || source.drawMode != SpriteDrawMode.Simple ||
                source.maskInteraction != SpriteMaskInteraction.None || source.sprite.associatedAlphaSplitTexture)
                return false;
            // These two drop prefabs have no SortingGroup or property overrides. Preserve native rendering
            // if a future effect introduces either instead of silently discarding its semantics.
            if (sortingGroup && sortingGroup.enabled) return false;
            if (HasUnsupportedOverrides()) return false;
            var matrix = source.localToWorldMatrix;
            Vector3 x = matrix.GetColumn(0), y = matrix.GetColumn(1);
            float sx = x.magnitude, sy = y.magnitude;
            if (sx < 0.00001f || sy < 0.00001f) return false;
            // Mesh particles reproduce the XY rotations/scales used by Coin/ExpOrb. Shear, mirrored
            // hierarchies and out-of-plane rotations fall back to SpriteRenderer without changing them.
            if (Mathf.Abs(x.z) > 0.00001f || Mathf.Abs(y.z) > 0.00001f ||
                Mathf.Abs(Vector3.Dot(x / sx, y / sy)) > 0.0001f || Vector3.Cross(x, y).z <= 0)
                return false;
            particle.position = matrix.GetColumn(3);
            particle.startSize3D = new Vector3(sx, sy, 1);
            particle.rotation3D = new Vector3(0, 0, Mathf.Atan2(x.y, x.x) * Mathf.Rad2Deg);
            particle.startColor = source.color;
            particle.startLifetime = 1000000;
            particle.remainingLifetime = 1000000;
            particle.randomSeed = 1;
            return true;
        }
    }
}
