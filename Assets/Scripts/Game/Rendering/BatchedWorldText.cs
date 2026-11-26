using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>Retains TMP layout and SDF styling while sharing draw meshes across world labels.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(TextMeshPro))]
    [DefaultExecutionOrder(10500)]
    public sealed class BatchedWorldText : MonoBehaviour
    {
        TextMeshPro text;
        MeshRenderer source;
        BrickRenderBatchGroup group;
        MaterialPropertyBlock properties;
        bool originalForceOff;
        bool changed = true;
        Matrix4x4 lastMatrix;
        bool lastVisible;
        int lastLayer, lastOrder, lastGameLayer;
        int lastPriority;
        bool lastHasOverrides;
        internal int Priority => group && group.sortingGroup ? group.sortingGroup.sortingOrder : 0;
        readonly Dictionary<Renderer, bool> hidden = new();
        readonly List<MeshRenderer> renderers = new();
        internal readonly List<Part> parts = new();

        internal struct Part
        {
            public Mesh mesh;
            public Material material;
            public Matrix4x4 matrix;
            public int layer, order, gameLayer;
            public uint renderingLayers;
        }

        void OnEnable()
        {
            text = GetComponent<TextMeshPro>();
            source = GetComponent<MeshRenderer>();
            group = GetComponentInParent<BrickRenderBatchGroup>();
            properties = new MaterialPropertyBlock();
            originalForceOff = source.forceRenderingOff;
            text.OnPreRenderText += OnTextUpdated;
            changed = true;
            WorldTextBatchRenderer.Register(this);
        }

        void OnTextUpdated(TMP_TextInfo _) => changed = true;

        internal bool Collect()
        {
            if (!text || !source) return false;
            source.GetPropertyBlock(properties);
            bool hasOverrides = !properties.isEmpty;
            bool visible = isActiveAndEnabled && text.isActiveAndEnabled && source.enabled && !originalForceOff && !hasOverrides;
            bool rebuilt = text.havePropertiesChanged;
            if (visible && (rebuilt || changed)) text.ForceMeshUpdate();
            var matrix = transform.localToWorldMatrix;
            bool dirty = changed || rebuilt || matrix != lastMatrix || visible != lastVisible ||
                lastLayer != source.sortingLayerID || lastOrder != source.sortingOrder || lastGameLayer != gameObject.layer ||
                Priority != lastPriority || hasOverrides != lastHasOverrides;
            if (!dirty) return false;
            parts.Clear();
            if (hasOverrides)
                foreach (var pair in hidden)
                    if (pair.Key) pair.Key.forceRenderingOff = pair.Value;
            if (visible)
            {
                var info = text.textInfo;
                for (int i = 0; i < info.materialCount; i++)
                {
                    var meshInfo = info.meshInfo[i];
                    if (meshInfo.vertexCount == 0 || !meshInfo.mesh || !meshInfo.material) continue;
                    parts.Add(new Part
                    {
                        mesh = meshInfo.mesh, material = meshInfo.material, matrix = matrix,
                        layer = source.sortingLayerID, order = source.sortingOrder,
                        gameLayer = gameObject.layer, renderingLayers = source.renderingLayerMask
                    });
                }
                if (rebuilt || changed || renderers.Count == 0) text.GetComponentsInChildren(true, renderers);
                foreach (var renderer in renderers)
                {
                    if (!hidden.ContainsKey(renderer)) hidden.Add(renderer, renderer.forceRenderingOff);
                    renderer.forceRenderingOff = true;
                }
            }
            changed = false;
            lastMatrix = matrix;
            lastVisible = visible;
            lastLayer = source.sortingLayerID;
            lastOrder = source.sortingOrder;
            lastGameLayer = gameObject.layer;
            lastPriority = Priority;
            lastHasOverrides = hasOverrides;
            return true;
        }

        void OnDisable()
        {
            WorldTextBatchRenderer.Unregister(this);
            if (text) text.OnPreRenderText -= OnTextUpdated;
            foreach (var pair in hidden)
                if (pair.Key) pair.Key.forceRenderingOff = pair.Value;
            hidden.Clear();
            if (source) source.forceRenderingOff = originalForceOff;
            parts.Clear();
        }
    }
}
