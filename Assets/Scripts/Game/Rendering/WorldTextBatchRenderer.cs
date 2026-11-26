using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    /// <summary>Combines TMP meshes by shared font material, stage and camera layer; preserves SDF UV channels.</summary>
    [DefaultExecutionOrder(11000)]
    public sealed class WorldTextBatchRenderer : MonoBehaviour
    {
        static WorldTextBatchRenderer instance;
        readonly List<BatchedWorldText> sources = new();
        readonly Dictionary<(Material, int, int, int, uint), Batch> batches = new();
        readonly List<(Material, int, int, int, uint)> empty = new();
        bool dirty;
        public int ActiveBatchCount { get; private set; }
        public int VisibleLabelCount { get; private set; }

        sealed class Batch
        {
            public GameObject gameObject;
            public Mesh mesh;
            public MeshRenderer renderer;
            public readonly List<CombineInstance> instances = new();
            public CombineInstance[] upload;
        }

        public static void Register(BatchedWorldText source)
        {
            if (!instance)
            {
                var go = new GameObject("World Text Batches") { hideFlags = HideFlags.DontSave };
                instance = go.AddComponent<WorldTextBatchRenderer>();
                if (Application.isPlaying) DontDestroyOnLoad(go);
            }
            if (!instance.sources.Contains(source)) instance.sources.Add(source);
            instance.dirty = true;
        }

        public static void Unregister(BatchedWorldText source)
        {
            if (!instance) return;
            instance.sources.Remove(source);
            instance.dirty = true;
            // Immediately rebuild to avoid a pooled/dead label lingering until the next frame.
            instance.Rebuild();
        }

        void LateUpdate() => Rebuild();

        public void Rebuild()
        {
            foreach (var source in sources)
                if (source) dirty |= source.Collect();
            if (!dirty) return;
            foreach (var batch in batches.Values) batch.instances.Clear();
            sources.Sort((left, right) => left.Priority.CompareTo(right.Priority));
            VisibleLabelCount = 0;
            foreach (var source in sources)
            {
                if (!source || source.parts.Count == 0) continue;
                VisibleLabelCount++;
                foreach (var part in source.parts)
                {
                    var key = (part.material, part.layer, part.order, part.gameLayer, part.renderingLayers);
                    if (!batches.TryGetValue(key, out var batch))
                    {
                        var go = new GameObject("Text Batch: " + part.material.name) { hideFlags = HideFlags.DontSave, layer = part.gameLayer };
                        go.transform.SetParent(transform, false);
                        var mesh = new Mesh { name = go.name, hideFlags = HideFlags.DontSave, indexFormat = IndexFormat.UInt32 };
                        mesh.MarkDynamic();
                        go.AddComponent<MeshFilter>().sharedMesh = mesh;
                        var renderer = go.AddComponent<MeshRenderer>();
                        renderer.sharedMaterial = part.material;
                        renderer.sortingLayerID = part.layer;
                        renderer.sortingOrder = part.order;
                        renderer.renderingLayerMask = part.renderingLayers;
                        renderer.shadowCastingMode = ShadowCastingMode.Off;
                        renderer.receiveShadows = false;
                        renderer.lightProbeUsage = LightProbeUsage.Off;
                        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                        batches.Add(key, batch = new Batch { gameObject = go, mesh = mesh, renderer = renderer });
                    }
                    batch.instances.Add(new CombineInstance { mesh = part.mesh, transform = part.matrix, subMeshIndex = 0 });
                }
            }
            empty.Clear();
            foreach (var pair in batches)
            {
                var batch = pair.Value;
                if (batch.instances.Count == 0) { empty.Add(pair.Key); continue; }
                if (batch.upload == null || batch.upload.Length != batch.instances.Count)
                    batch.upload = new CombineInstance[batch.instances.Count];
                batch.instances.CopyTo(batch.upload);
                batch.mesh.Clear();
                batch.mesh.CombineMeshes(batch.upload, true, true, false);
                batch.renderer.enabled = true;
            }
            foreach (var key in empty)
            {
                var batch = batches[key];
                batch.renderer.enabled = false;
                DisposeObject(batch.mesh);
                DisposeObject(batch.gameObject);
                batches.Remove(key);
            }
            ActiveBatchCount = batches.Count;
            dirty = false;
        }

        void OnDestroy()
        {
            if (instance == this) instance = null;
            foreach (var batch in batches.Values) DisposeObject(batch.mesh);
            batches.Clear();
        }

        internal static void DisposeObject(Object value)
        {
            if (Application.isPlaying) Destroy(value);
            else DestroyImmediate(value);
        }
    }
}
