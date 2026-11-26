using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains
{
    /// <summary>
    /// Shares immutable effect materials between renderers with the same base material and effect values.
    /// Unlike per-renderer custom property blocks, equal effect states retain native sprite batching eligibility.
    /// Owners must Dispose on destruction. Sprite textures and colors remain controlled by SpriteRenderer.
    /// </summary>
    public sealed class RendererPropertyOverrides : IDisposable
    {
        readonly Renderer renderer;
        readonly Material original;
        readonly SortedDictionary<int, Value> values = new();
        static readonly Dictionary<Key, Entry> cache = new();
        Key current;
        bool disposed;

        readonly struct Value : IEquatable<Value>
        {
            public readonly Vector4 data;
            public readonly bool color;
            public Value(Vector4 data, bool color) { this.data = data; this.color = color; }
            public bool Equals(Value other) => color == other.color && data.Equals(other.data);
            public override int GetHashCode() => HashCode.Combine(data, color);
        }

        sealed class Key : IEquatable<Key>
        {
            public readonly Material original;
            public readonly KeyValuePair<int, Value>[] values;
            readonly int hash;
            public Key(Material original, SortedDictionary<int, Value> state)
            {
                this.original = original;
                values = new KeyValuePair<int, Value>[state.Count];
                int i = 0;
                hash = original.GetInstanceID();
                foreach (var pair in state)
                {
                    values[i++] = pair;
                    hash = unchecked(hash * 31 + HashCode.Combine(pair.Key, pair.Value));
                }
            }
            public bool Equals(Key other)
            {
                if (other == null || original != other.original || values.Length != other.values.Length) return false;
                for (int i = 0; i < values.Length; i++)
                    if (values[i].Key != other.values[i].Key || !values[i].Value.Equals(other.values[i].Value)) return false;
                return true;
            }
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => hash;
        }

        sealed class Entry { public Material material; public int references; }

        public RendererPropertyOverrides(Renderer renderer)
        {
            this.renderer = renderer;
            original = renderer ? renderer.sharedMaterial : null;
        }

        public void SetFloat(int property, float value) => Set(property, new Value(new Vector4(value, 0, 0, 0), false));
        public void SetColor(int property, Color value) => Set(property, new Value(value, true));

        void Set(int property, Value value)
        {
            if (disposed || !renderer || !original || !original.HasProperty(property)) return;
            var baseline = value.color ? new Value(original.GetColor(property), true) :
                new Value(new Vector4(original.GetFloat(property), 0, 0, 0), false);
            if (value.Equals(baseline))
            {
                if (!values.Remove(property)) return;
            }
            else
            {
                if (values.TryGetValue(property, out var old) && old.Equals(value)) return;
                values[property] = value;
            }
            Key next = values.Count == 0 ? null : new Key(original, values);
            Material material = original;
            if (next != null)
            {
                if (!cache.TryGetValue(next, out var entry))
                {
                    material = new Material(original) { name = original.name + " (Shared Effect)", hideFlags = HideFlags.DontSave };
                    foreach (var pair in next.values)
                        if (pair.Value.color) material.SetColor(pair.Key, (Color)pair.Value.data);
                        else material.SetFloat(pair.Key, pair.Value.data.x);
                    cache.Add(next, entry = new Entry { material = material });
                }
                material = entry.material;
                entry.references++;
            }
            renderer.sharedMaterial = material;
            Release();
            current = next;
        }

        void Release()
        {
            if (current == null) return;
            var entry = cache[current];
            if (--entry.references == 0)
            {
                cache.Remove(current);
                if (Application.isPlaying) UnityEngine.Object.Destroy(entry.material);
                else UnityEngine.Object.DestroyImmediate(entry.material);
            }
            current = null;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            if (renderer && current != null && renderer.sharedMaterial == cache[current].material)
                renderer.sharedMaterial = original;
            Release();
            values.Clear();
        }
    }
}
