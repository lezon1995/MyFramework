using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MoreMountains
{
    /// <summary>
    /// Draws the brick field in shared visual stages instead of repeating all stages per brick.
    /// Health bars and labels are overlays above every brick body. Local transforms are untouched.
    /// </summary>
    [DisallowMultipleComponent, DefaultExecutionOrder(9000)]
    public sealed class BrickRenderBatchGroup : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public Renderer renderer;
            public int order;
        }

        public SortingGroup sortingGroup;
        public Entry[] entries = Array.Empty<Entry>();
        int[] originalLayers, originalOrders;
        bool groupWasEnabled;

        // Keep the existing per-row priority within each stage without giving every brick
        // a different sortingOrder (which would split instancing again). The game is on XY.
        public float DepthOffset => sortingGroup ? -sortingGroup.sortingOrder * 0.0001f : 0;

        public static int GetStage(string name) => name switch
        {
            "SpriteShadow" => 0,
            "SpriteBlock" => 10,
            "SpriteUnitShadow" => 20,
            "SpriteUnit" => 30,
            "Shield" => 40,
            "ShieldAmount" => 45,
            "Back" => 50,
            "HealthBarRenderer" => 60,
            "Frame" => 70,
            "Health" => 80,
            _ => -1
        };

        void OnEnable()
        {
            originalLayers = new int[entries.Length];
            originalOrders = new int[entries.Length];
            groupWasEnabled = sortingGroup && sortingGroup.enabled;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].renderer)
                {
                    originalLayers[i] = entries[i].renderer.sortingLayerID;
                    originalOrders[i] = entries[i].renderer.sortingOrder;
                }
            Apply();
        }

        void LateUpdate() => Apply();

        public void Apply()
        {
            if (!sortingGroup) return;
            // Leaving the component present keeps BrickRenderer's existing API valid.
            // Its per-row order must not split the shared stages into individual bricks again.
            sortingGroup.enabled = false;
            foreach (var entry in entries)
                if (entry.renderer)
                {
                    entry.renderer.sortingLayerID = sortingGroup.sortingLayerID;
                    entry.renderer.sortingOrder = entry.order;
                }
        }

        void OnDisable()
        {
            if (originalLayers == null) return;
            for (int i = 0; i < entries.Length; i++)
                if (entries[i].renderer)
                {
                    entries[i].renderer.sortingLayerID = originalLayers[i];
                    entries[i].renderer.sortingOrder = originalOrders[i];
                }
            if (sortingGroup) sortingGroup.enabled = groupWasEnabled;
        }
    }
}
