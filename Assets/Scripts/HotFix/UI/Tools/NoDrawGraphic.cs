using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class NoDrawGraphic : Graphic
    {
        protected override void Awake()
        {
            base.Awake();

            if (!TryGetComponent(out CanvasRenderer _))
            {
                gameObject.AddComponent<CanvasRenderer>();
            }
        }

        public override void SetVerticesDirty()
        {
        }

        public override void SetMaterialDirty()
        {
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}