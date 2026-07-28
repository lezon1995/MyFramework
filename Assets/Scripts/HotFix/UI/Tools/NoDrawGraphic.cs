using UnityEngine.UI;

namespace MoreMountains
{
    public class NoDrawGraphic : Graphic
    {
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